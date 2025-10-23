using System.Net.Sockets;
using System.Net;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;
using FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

public class Sentinel : FluffyCoreProcessBase
{
    public override string Name => "Sentinel";

    public Watcher Watcher { get; private set; }

    private TcpListener _listener;
    public bool IsListening = false;
    
    private const string LISTENADDRESS = "10.0.0.84";
    private const int LISTENPORT = 9997;

    public Sentinel()
    {
        _listener = new(IPAddress.Parse(LISTENADDRESS), LISTENPORT);

        Watcher = new(this);  
    }

    protected override async Task OnStartAsync()
    {
        _listener = new(IPAddress.Parse(LISTENADDRESS), LISTENPORT);
        _ = StartListeningAsync();

        await Task.CompletedTask;
        return;
    }

    protected override async Task OnStopAsync()
    {
        await Task.CompletedTask;
    }

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


