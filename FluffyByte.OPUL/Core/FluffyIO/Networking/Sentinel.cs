using System.Net.Sockets;
using System.Net;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

public class Sentinel : FluffyCoreProcessBase
{
    public override string Name => "Sentinel";

    private TcpListener _listener;

    public Sentinel()
    {
        _listener = new(IPAddress.Parse("10.0.0.84"), 9997);
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
        _listener = new(IPAddress.Parse("10.0.0.84"), 9997);

        _listener.Start();

        await Task.CompletedTask;
    }

    protected override async Task OnStopAsync()
    {
        await Task.CompletedTask;
    }
}
