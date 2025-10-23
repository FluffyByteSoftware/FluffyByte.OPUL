using System.Net.Sockets;
using System.Net;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;
using FluffyByte.OPUL.Core.FluffyIO.Networking.FluffyClients;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

public class Sentinel : FluffyCoreProcessBase
{
    public override string Name => "Sentinel";

    public Watcher Watcher { get; private set; }

    private TcpListener _listener;
    
    private readonly string _listenAddress = "10.0.0.84";
    private readonly int _port = 9997;

    public Sentinel(string hostAddress, int port)
    {
        _listenAddress = hostAddress;
        _port = port;

        _listener = new(IPAddress.Parse(_listenAddress), _port);

        Watcher = new(this);
        Watcher.ClearAll();
    }

    protected override async Task OnStartAsync()
    {

        _listener = new(IPAddress.Parse(_listenAddress), _port);

        _listener.Start();

        Scribe.Info($"Listening for connections on {_listenAddress}:{_port}");

        if(_internalCancellation == null)
        {
            Scribe.Warning("MASTER CANCELLATION TOKEN IS NULL!!!!!!!!!");
            return;
        }

        while (!_internalCancellation.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(_internalCancellation.Token);

                var client = new FluffyRawClient(tcpClient, this, _internalCancellation.Token);
            }
            catch(Exception ex)
            {
                Scribe.Error("Error in OnStartAsync()", ex);
                return;
            }
        }
        await Task.CompletedTask;
    }

    protected override async Task OnStopAsync()
    {
        await Task.CompletedTask;
    }
}
