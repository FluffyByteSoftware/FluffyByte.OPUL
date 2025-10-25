using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

public class FluffyClient
{
    public TcpIO TcpIO { get; private set; }
    public UdpIO UdpIO { get; private set; }

    public string Name { get; set; } = "UnnamedClient";

    public int Id { get; private set; }
    public Guid Guid { get; private set; } = Guid.NewGuid();

    private static int _id = 0;

    private CancellationToken _shutdownToken;
    private Sentinel _sentinelRef;
    internal TcpClient _tcpClient;
    internal UdpClient _udpClient;

    public FluffyClient(TcpClient tcpClient, Sentinel sentinel, CancellationToken shutdownToken)
    {
        _tcpClient = tcpClient;
        _sentinelRef = sentinel;
        _shutdownToken = shutdownToken;

        TcpIO = new(this, _shutdownToken);
        UdpIO = new(this, _shutdownToken);

        Name = $"Client_{Id}";
    }

    internal bool TestConnection()
    {
        if (_tcpClient.Client == null || !_tcpClient.Connected)
            return false;

        try
        {
            if(_tcpClient.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1];
                if(_tcpClient.Client.Receive(buffer, SocketFlags.Peek) == 0)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

}
