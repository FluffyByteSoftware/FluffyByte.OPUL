using System.Net;
using System.Net.Sockets;
using System.Text;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

public class FluffyClient
{
    // ===== STATIC =====
    private static int _id = 0;

    // ===== PUBLIC PROPERTIES =====
    public int Id { get; private set; }
    public string Name { get; private set; } = "RawClient";
    public Guid Guid { get; private set; }

    public BinaryIO BinaryIO { get; private set; }
    public TextIO TextIO { get; private set; }

    public bool Disconnecting => _disconnecting;
    public bool IsConnected => _connected;

    public string DisconnectReason { get; private set; } = "No Reason";

    private bool _connected;
    private bool _disconnecting;

    private readonly TcpClient TcpClient;
    private readonly NetworkStream _stream;
    private readonly Sentinel _sentinelReference;
    private readonly IPEndPoint? _remoteEndpoint;

    public FluffyClient(TcpClient tcpClient, Sentinel sentinelReference, CancellationToken ct)
    {
        _id++;
        Id = _id;

        _sentinelReference = sentinelReference;

        _stream = tcpClient.GetStream();

        BinaryIO = new(this, _stream, ct);
        TextIO = new(this, _stream, ct);

        TcpClient = tcpClient;

        _remoteEndpoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;

        if(_remoteEndpoint != null)
        {
            Name = $"Client_{_remoteEndpoint.Address}";
        }
        else
        {
            Scribe.Warning("Couldn't determine remote end point for a client.");
            Name = $"Client_{Id}";
        }

    }

    /// <summary>
    /// Gracefully disconnects the client (async version).
    /// </summary>
    public async Task DisconnectAsync(string reason = "no reason given")
    {
        if (_disconnecting)
            return;

        DisconnectReason = reason;

        _disconnecting = true;
        _connected = false;

        HandleDisconnect();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Gracefully disconnects the client (sync version).
    /// </summary>
    public void Disconnect(string reason = "no reason given")
    {
        if (_disconnecting)
            return;

        DisconnectReason = reason;

        _disconnecting = true;
        _connected = false;

        HandleDisconnect();
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
            TcpClient?.Close();
            TcpClient?.Dispose();

            // Unregister from watcher
            
        }
        catch (Exception ex)
        {
            Scribe.Error($"Error during disconnect cleanup for {Name}", ex);
        }
    }

    internal async Task HandleNetworkExceptionAsync(Exception ex, string operationName)
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
}