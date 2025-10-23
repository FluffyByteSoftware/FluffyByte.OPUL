using System.Net.Sockets;
using System.Net;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;
using FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

/// <summary>
/// Represents a process that listens for incoming TCP connections and handles client interactions.
/// </summary>
/// <remarks>The <see cref="Sentinel"/> class is responsible for managing a TCP listener that accepts client
/// connections and processes them asynchronously. It uses a <see cref="Watcher"/> to monitor its state and
/// operations.</remarks>
public class Sentinel : FluffyCoreProcessBase
{
    public override string Name => "Sentinel";

    public Watcher Watcher { get; private set; }

    private TcpListener _listener;
    public bool IsListening = false;
    
    private const string LISTENADDRESS = "10.0.0.84";
    private const int LISTENPORT = 9997;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sentinel"/> class.
    /// </summary>
    /// <remarks>This constructor sets up the network listener on the specified address and port, and
    /// initializes the watcher component to monitor the sentinel's activities.</remarks>
    public Sentinel()
    {
        _listener = new(IPAddress.Parse(LISTENADDRESS), LISTENPORT);

        Watcher = new(this);  
    }

    /// <summary>
    /// Initializes and starts the asynchronous listening process for incoming network connections.
    /// </summary>
    /// <remarks>This method sets up a listener on the specified IP address and port, and begins listening for
    /// incoming connections asynchronously.</remarks>
    protected override async Task OnStartAsync()
    {
        _listener = new(IPAddress.Parse(LISTENADDRESS), LISTENPORT);
        _ = StartListeningAsync();

        await Task.CompletedTask;
        return;
    }

    /// <summary>
    /// Executes the operations required to stop the service asynchronously.
    /// </summary>
    /// <remarks>This method is called when the service is stopping. Override this method to implement any 
    /// custom shutdown logic. The default implementation completes immediately.</remarks>
    protected override async Task OnStopAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously starts the TCP listener to accept incoming client connections.
    /// </summary>
    /// <remarks>This method initiates the listener and begins accepting client connections asynchronously. It
    /// sets the <see cref="IsListening"/> property to <see langword="true"/> when the listener starts successfully. If
    /// the listener is already active, the method logs a warning and returns without making changes.</remarks>
    private async Task StartListeningAsync()
    {
        if(_internalCancellation == null)
        {
            Scribe.Warning($"[{Name}] Internal Cancellation Token didn't pass to Sentinel.");
            return;
        }

        if (IsListening)
        {
            Scribe.Warning($"[{Name}] was requested to start listening, but is already in that state?");
            return;
        }

        try
        {
            _listener.Start();
            IsListening = true;
            Scribe.Info($"[{Name}]: Now listening on: {LISTENADDRESS}:{LISTENPORT}");

            while (!_internalCancellation.IsCancellationRequested)
            {
                TcpClient newTcpClient = await _listener.AcceptTcpClientAsync(_internalCancellation.Token);
                
                Scribe.Info($"[{Name}] New Client Joined!");
                
                _ = HandleClientAsync(newTcpClient);
            }
        }
        catch(Exception ex)
        {
            Scribe.Error($"[{Name}] Listener crashed.", ex);
            IsListening = false;
        }
    }

    /// <summary>
    /// Asynchronously handles communication with a connected TCP client.
    /// </summary>
    /// <remarks>This method initializes a new <see cref="FluffyClient"/> to manage the client connection and
    /// performs a simple read/write operation. It sends a greeting message to the client and waits for a response. If
    /// the response is empty or whitespace, a default message is logged. The client is then disconnected.</remarks>
    /// <param name="client">The <see cref="TcpClient"/> representing the connected client to handle.</param>
    private async Task HandleClientAsync(TcpClient client)
    {
        if(_internalCancellation == null)
        {
            Scribe.Warning($"[{Name}] Cancellation Token was not passed to _internalCancellation, could not HandleClientAsync()");
            
            return;
        }

        try
        {
            FluffyClient newClient = new(client, this, _internalCancellation.Token);

            await newClient.TextIO.WriteLineAsync("Hello World!");
            string response = await newClient.TextIO.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(response))
            {
                response = "NO STRING FOUND!!!@@@@";
            }

            Scribe.Info($"Received response: {response}");
            await newClient.DisconnectAsync();
        }
        catch(Exception ex)
        {
            Scribe.Error($"[{Name}] HandleClientAsync() crashed!", ex);
            IsListening = false;
        }
    }
}


