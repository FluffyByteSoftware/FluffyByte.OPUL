using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

public class BinaryIO(FluffyClient parent, NetworkStream parentStream, 
    CancellationToken ct)
{
    private readonly FluffyClient _parent = parent;

    private readonly BinaryReader _reader = new(parentStream, Encoding.UTF8, true);
    private readonly BinaryWriter _writer = new(parentStream, Encoding.UTF8, true);

    private readonly CancellationToken _shutdownToken = ct;

    private const int MAX_MESSAGE_SIZE = 1048576;

    public async Task WriteBinaryAsync(byte[] data)
    {
        if (_parent.Disconnecting)
        {
            return;
        }

        if(data == null || data.Length == 0)
        {
            Scribe.Warning($"Attempted to send empty binary data.");
            return;
        }

        try
        {

        }
        catch(Exception ex)
        {
            await _parent.HandleNetworkExceptionAsync(ex, "WriteBinaryAsync()");
        }
    }

    public async Task<byte[]?> ReadBinaryAsync()
    {
        if (_parent.Disconnecting)
        {
            return [];
        }

        try
        {
            // Read length prefix (4 bytes)
            int messageLength = _reader.ReadInt32();

            // Validate length to prevent memory exhaustion
            if (messageLength <= 0 || messageLength > MAX_MESSAGE_SIZE)
            {
                Scribe.Warning($"Client sent invalid message length: {messageLength} bytes");
                await _parent.DisconnectAsync("Invalid message length in header.");
                
                return [];
            }

            byte[] buffer = new byte[messageLength];

            int totalBytesRead = 0;

            while(totalBytesRead < messageLength)
            {
                _shutdownToken.ThrowIfCancellationRequested();

                int bytesRead = await parentStream.
                    ReadAsync(
                    buffer.AsMemory(
                    totalBytesRead, 
                    (messageLength - totalBytesRead)),
                    _shutdownToken);
            }
            
            
        }
        catch(Exception ex)
        {
            await _parent.HandleNetworkExceptionAsync(ex, "ReadBinaryAsync()");
        }

        return [];
    }
}
