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
    public Metrics Metrics { get; private set; }

    public bool Disconnecting => _disconnecting;
    public bool IsConnected => _connected;

    public string DisconnectReason { get; private set; } = "No Reason";

    private bool _connected;
    private bool _disconnecting;

    private readonly TcpClient TcpClient;
    private readonly NetworkStream _stream;
    private readonly Sentinel _sentinelReference;
    private readonly IPEndPoint? _remoteEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluffyClient"/> class using the specified TCP client, sentinel
    /// reference, and cancellation token.
    /// </summary>
    /// <remarks>The constructor sets up the network stream and initializes input/output operations for both
    /// binary and text data. It also assigns a unique identifier to the client and attempts to determine the remote
    /// endpoint's address. If the remote endpoint cannot be determined, a warning is logged, and a default name is
    /// assigned to the client.</remarks>
    /// <param name="tcpClient">The <see cref="TcpClient"/> used to establish the network connection. Must not be null.</param>
    /// <param name="sentinelReference">A reference to the <see cref="Sentinel"/> associated with this client. Used for monitoring and control purposes.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel operations associated with this client.</param>
    public FluffyClient(TcpClient tcpClient, Sentinel sentinelReference, CancellationToken ct)
    {
        _id++;
        Id = _id;

        _sentinelReference = sentinelReference;

        _stream = tcpClient.GetStream();

        BinaryIO = new(this, _stream, ct);
        TextIO = new(this, _stream, ct);
        Metrics = new(this);

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
    /// Asynchronously disconnects the current connection, specifying a reason for the disconnection.
    /// </summary>
    /// <remarks>This method sets the connection state to disconnected and triggers any necessary
    /// disconnection handling. If a disconnection is already in progress, the method returns immediately without
    /// performing any action.</remarks>
    /// <param name="reason">The reason for the disconnection. Defaults to "no reason given" if not specified.</param>
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
    /// Disconnects the current connection, specifying a reason for the disconnection.
    /// </summary>
    /// <remarks>This method sets the connection state to disconnected and triggers any necessary
    /// disconnection handling. If the connection is already in the process of disconnecting, the method returns
    /// immediately without further action.</remarks>
    /// <param name="reason">The reason for the disconnection. Defaults to "no reason given" if not specified.</param>
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
    /// Handles the disconnection of a client by performing necessary cleanup operations.
    /// </summary>
    /// <remarks>This method logs the disconnection event, closes and disposes of the TCP client and its
    /// associated stream, and unregisters the client from any watchers. It ensures that resources are properly released
    /// to prevent resource leaks. If an error occurs during cleanup, it logs the error details.</remarks>
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

    /// <summary>
    /// Internal task available to the children of FluffyClient to handle network exceptions in a standardized way.
    /// </summary>
    /// <param name="ex">Exception to be examined</param>
    /// <param name="operationName">The caller or some other information to inform 
    /// the administrator of the error location</param>
    /// <returns></returns>
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