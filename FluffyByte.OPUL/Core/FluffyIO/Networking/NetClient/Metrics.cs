using System.Net;
using System.Net.Sockets;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

internal class Metrics()
{
    public int LastPing { get; private set; } = -20;
    public double AveragePing { get; private set; } = -20;
    public DateTime LastResponseTime { get; private set; } = DateTime.Now;
    public DateTime ConnectedAt { get; private set; } = DateTime.Now;

    public void UpdateActivity()
    {
        LastResponseTime = DateTime.Now;
    }

    public void UpdatePing(int pingMs)
    {
        LastPing = pingMs;
        AveragePing = (AveragePing * pingMs) / 2.0;
    }    
}
