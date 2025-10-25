using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

public class TcpIO
{
    private FluffyClient _parent;
    private CancellationToken _shutdownToken;

    private NetworkStream _tcpStream;
    private StreamReader _tcpTxtReader;
    private StreamWriter _tcpTxtWriter;
    private Stream _stream;
    private BinaryReader _binReader;
    private BinaryWriter _binWriter;



    public TcpIO(FluffyClient parent, CancellationToken shutdownToken)
    {
        _parent = parent;
        _tcpStream = _parent._tcpClient.GetStream();
        _stream = _parent._tcpClient.GetStream();
        _tcpTxtReader = new StreamReader(_tcpStream, Encoding.UTF8);
        _tcpTxtWriter = new StreamWriter(_tcpStream, Encoding.UTF8) { AutoFlush = true };

        _binReader = new BinaryReader(_stream, Encoding.UTF8, true);
        _binWriter = new BinaryWriter(_stream, Encoding.UTF8, true);
    }

    public async Task WriteTextAsync(string message, bool removeNewLine = false)
    {
        if(_parent.TestConnection() == false)
        {
            return;
        }
    }
}
