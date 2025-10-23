using System.Net;
using System.Net.Sockets;
using System.Text;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.FluffyClients;

public class FluffyRawClient : IFluffyClient
{
    // ===== STATIC =====
    private static int _id = 0;

    // ===== PUBLIC PROPERTIES =====
    public int Id { get; private set; }
    public string Name { get; private set; } = "RawClient";
    public Guid Guid { get; private set; }

    public string Address { get; private set; } = "0.0.0.1";
    public IPAddress IPAddress { get; private set; }
    public string HostName { get; private set; } = "unknown.com.unk";
    public bool IsConnected { get; private set; } = false;

    public int LastPing { get; private set; }
    public double AveragePing { get; private set; }

    public DateTime LastResponseTime { get; private set; }
    public DateTime ConnectedAtTime { get; private set; }
    public DateTime LastActivityTime { get; private set; }

    public string DisconnectReason { get; private set; } = "no reason";

    // ===== PRIVATE FIELDS =====
    private bool _disconnecting = false;

    private readonly CancellationToken _shutdownToken;
    private readonly TcpClient _tcpClient;
    private readonly Sentinel _sentinelRef;

    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;

    // ===== CONSTRUCTOR =====
    public FluffyRawClient(TcpClient tcpClient, Sentinel sentinel, CancellationToken ct)
    {
        // Increment and assign unique ID
        _id++;
        Id = _id;
        Guid = Guid.NewGuid(); // Fixed: use NewGuid() instead of new()

        // Parse address (TODO: extract from tcpClient.Client.RemoteEndPoint)
        IPAddress = IPAddress.Parse(Address);
        IsConnected = true;

        // Initialize timestamps
        DateTime now = DateTime.Now;
        ConnectedAtTime = now;
        LastActivityTime = now;
        LastResponseTime = now;

        // Initialize ping metrics
        LastPing = 0;
        AveragePing = 0;

        // Store dependencies
        _sentinelRef = sentinel;
        _shutdownToken = ct;

        // Setup TCP client and streams
        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();

        // Text-based I/O
        _reader = new StreamReader(_stream, encoding: Encoding.UTF8);
        _writer = new StreamWriter(_stream, encoding: Encoding.UTF8) { AutoFlush = true };

        // Binary I/O (leaveOpen prevents premature stream closure)
        _binaryReader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
        _binaryWriter = new BinaryWriter(_stream, Encoding.UTF8, leaveOpen: true);

        // Register with the watcher
        _sentinelRef.Watcher.RegisterRawClient(this);

        Scribe.Info($"Client {Id} connected from {Address}");
    }

    // ===== PUBLIC METHODS - METADATA =====

    public void SetName(string name)
    {
        Scribe.Debug($"Client {Id} name changed: '{Name}' -> '{name}'");
        Name = name;
    }

    public void SetDisconnectReason(string reason)
    {
        DisconnectReason = reason;
    }

    // ===== PUBLIC METHODS - BINARY I/O =====

    /// <summary>
    /// Writes binary data to the client with length-prefix framing.
    /// Format: [4-byte int32 length][N bytes payload]
    /// </summary>
    public async Task WriteBinaryAsync(byte[] data)
    {
        if (_disconnecting)
            return;

        if (data == null || data.Length == 0)
        {
            Scribe.Warning($"Attempted to send empty binary data to {Name}");
            return;
        }

        try
        {
            // Write length prefix (4 bytes)
            _binaryWriter.Write(data.Length);

            // Write payload
            _binaryWriter.Write(data);

            // Flush to ensure immediate transmission
            await _stream.FlushAsync(_shutdownToken);

            LastActivityTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            await HandleNetworkExceptionAsync(ex, nameof(WriteBinaryAsync));
        }
    }

    /// <summary>
    /// Reads binary data from the client using length-prefix framing.
    /// Returns empty array on error or disconnection.
    /// </summary>
    public async Task<byte[]> ReadBinaryAsync()
    {
        if (_disconnecting)
            return [];

        try
        {
            // Read length prefix (4 bytes)
            int messageLength = _binaryReader.ReadInt32();

            // Validate length to prevent memory exhaustion attacks
            const int MAX_MESSAGE_SIZE = 1048576; // 1MB limit
            if (messageLength <= 0 || messageLength > MAX_MESSAGE_SIZE)
            {
                Scribe.Warning($"Client {Name} sent invalid message length: {messageLength} bytes");
                await DisconnectAsync();
                return [];
            }

            // Allocate buffer
            byte[] buffer = new byte[messageLength];

            // Read exact number of bytes (handle TCP fragmentation)
            int totalBytesRead = 0;
            while (totalBytesRead < messageLength)
            {
                _shutdownToken.ThrowIfCancellationRequested();

                int bytesRead = await _stream.ReadAsync(
                    buffer.AsMemory(totalBytesRead, messageLength - totalBytesRead),
                    _shutdownToken
                );

                // Connection closed mid-message
                if (bytesRead == 0)
                {
                    Scribe.Debug($"Client {Name} disconnected mid-message (received {totalBytesRead}/{messageLength} bytes)");
                    await DisconnectAsync();
                    return [];
                }

                totalBytesRead += bytesRead;
            }

            // Update activity timestamps
            LastActivityTime = DateTime.Now;
            LastResponseTime = DateTime.Now;

            return buffer;
        }
        catch (Exception ex)
        {
            await HandleNetworkExceptionAsync(ex, nameof(ReadBinaryAsync));
            return [];
        }
    }

    // ===== PUBLIC METHODS - TEXT I/O =====

    /// <summary>
    /// Writes a text line to the client (TCP messaging layer).
    /// </summary>
    public async Task WriteLineAsync(string message, bool omitNewLine = false)
    {
        if (_disconnecting)
            return;

        if (string.IsNullOrEmpty(message))
        {
            Scribe.Warning($"Attempted to send empty message to {Name}");
            return;
        }

        try
        {
            if (omitNewLine)
            {
                await _writer.WriteAsync(message.ToCharArray(), _shutdownToken);
            }
            else
            {
                await _writer.WriteLineAsync(message.ToCharArray(), _shutdownToken);
            }

            LastActivityTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            await HandleNetworkExceptionAsync(ex, nameof(WriteLineAsync));
        }
    }

    /// <summary>
    /// Reads a text line from the client (TCP messaging layer).
    /// Returns empty string on error or disconnection.
    /// </summary>
    public async Task<string> ReadLineAsync()
    {
        if (_disconnecting)
            return string.Empty;

        try
        {
            string? response = await _reader.ReadLineAsync(_shutdownToken);

            if (response != null)
            {
                LastActivityTime = DateTime.Now;
                LastResponseTime = DateTime.Now;
            }

            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            await HandleNetworkExceptionAsync(ex, nameof(ReadLineAsync));
            return string.Empty;
        }
    }

    // ===== PUBLIC METHODS - CONNECTION MANAGEMENT =====

    /// <summary>
    /// Tests if the connection is still alive.
    /// TODO: Implement actual connectivity test (ping/pong protocol)
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        if (_disconnecting || !IsConnected)
            return false;

        // TODO: Send a ping packet and wait for pong response
        // For now, just check if the socket is connected

        await Task.CompletedTask;

        try
        {
            return _tcpClient.Connected && _stream.CanRead && _stream.CanWrite;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gracefully disconnects the client (async version).
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_disconnecting)
            return;

        _disconnecting = true;
        IsConnected = false;

        HandleDisconnect();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Gracefully disconnects the client (sync version).
    /// </summary>
    public void Disconnect()
    {
        if (_disconnecting)
            return;

        _disconnecting = true;
        IsConnected = false;

        HandleDisconnect();
    }

    // ===== PRIVATE METHODS - ERROR HANDLING =====

    /// <summary>
    /// Centralized exception handler for network operations.
    /// Logs appropriate messages and triggers disconnection when needed.
    /// </summary>
    private async Task HandleNetworkExceptionAsync(Exception ex, string operationName)
    {
        switch (ex)
        {
            case IOException ioEx:
                Scribe.Debug($"Client {Name} disconnecting during {operationName}: {ioEx.Message}");
                await DisconnectAsync();
                break;

            case ObjectDisposedException:
                Scribe.Debug($"Client {Name} stream already disposed during {operationName}");
                break;

            case OperationCanceledException:
                Scribe.Debug($"Operation {operationName} cancelled for {Name} (server shutdown)");
                break;

            default:
                Scribe.Error($"Unexpected error in {operationName} for client {Name}", ex);
                await DisconnectAsync();
                break;
        }
    }

    /// <summary>
    /// Internal cleanup logic for disconnection.
    /// </summary>
    private void HandleDisconnect()
    {
        try
        {
            Scribe.Info($"Client {Name} (ID: {Id}) disconnected. Reason: {DisconnectReason}");

            // Close and dispose TCP client
            _stream?.Close();
            _tcpClient?.Close();
            _tcpClient?.Dispose();

            // Unregister from watcher
            _sentinelRef.Watcher.RemoveRawClient(this);
        }
        catch (Exception ex)
        {
            Scribe.Error($"Error during disconnect cleanup for {Name}", ex);
        }
    }
}