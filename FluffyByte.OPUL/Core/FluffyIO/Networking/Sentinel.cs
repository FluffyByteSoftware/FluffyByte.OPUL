using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

public class Sentinel(CancellationToken shutdownToken) : FluffyCoreProcessBase(shutdownToken)
{
    // Constants
    private const int TCP_HOST_PORT = 9998;
    private const int UDP_HOST_PORT = 9997;
    private const string DEFAULT_HOST_IP = "10.0.0.84";

    public override string Name => "Sentinel";

    // Dependents
    public Watcher Watcher { get; private set; }

    // private fields for internal use
    private CancellationToken _shutdownToken;
    private TcpListener _tcpListener = new(IPAddress.Loopback, 1111);

    protected override async Task OnStartAsync()
    {
        _tcpListener = new(IPAddress.Parse(DEFAULT_HOST_IP), TCP_HOST_PORT);
    }

    protected override async Task OnStopAsync()
    {

    }
}