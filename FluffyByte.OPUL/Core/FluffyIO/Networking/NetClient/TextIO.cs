using System.Net.Sockets;
using System.Text;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

public class TextIO
{
    private readonly FluffyClient _parent;
    private readonly CancellationToken _shutdownToken;
    private readonly NetworkStream _parentStream;

    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    public TextIO(FluffyClient parent, NetworkStream parentStream, CancellationToken shutdownToken)
    {
        _parent = parent;
        _parentStream = parentStream;
        _shutdownToken = shutdownToken;

        _reader = new StreamReader(parentStream, encoding: Encoding.UTF8, false);
        _writer = new StreamWriter(parentStream, encoding: Encoding.UTF8) { AutoFlush = true };
    }

    public async Task WriteLineAsync(string message)
    {
        if (_parent.Disconnecting) 
            return;

        try
        {
            await _writer.WriteLineAsync(message);
        }
        catch(Exception ex)
        {
            await _parent.HandleNetworkExceptionAsync(ex, "WriteLineAsync");
        }
    }

    public async Task<string> ReadLineAsync()
    {
        string? response = string.Empty;

        if (_parent.Disconnecting)
            return response;

        try
        {
            response = await _reader.ReadLineAsync(_shutdownToken);
        }
        catch(Exception ex)
        {
            await _parent.HandleNetworkExceptionAsync(ex, "ReadLineAsync");
        }

        if (string.IsNullOrWhiteSpace(response)) response = string.Empty;

        return response;
    }

}
