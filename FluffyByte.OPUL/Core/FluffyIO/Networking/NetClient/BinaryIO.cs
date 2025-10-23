using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;

/// <summary>
/// Provides methods for reading and writing binary data over a network stream.
/// </summary>
/// <remarks>This class is designed to handle binary data transmission in a networked environment, ensuring that
/// data is read and written asynchronously. It manages message size validation to prevent memory exhaustion and handles
/// network exceptions gracefully.</remarks>
/// <param name="parent"></param>
/// <param name="parentStream"></param>
/// <param name="ct"></param>
public class BinaryIO(FluffyClient parent, NetworkStream parentStream, 
    CancellationToken ct)
{
    private readonly FluffyClient _parent = parent;

    private readonly BinaryReader _reader = new(parentStream, Encoding.UTF8, true);
    private readonly BinaryWriter _writer = new(parentStream, Encoding.UTF8, true);

    private readonly CancellationToken _shutdownToken = ct;

    private const int MAX_MESSAGE_SIZE = 1048576;

    /// <summary>
    /// Asynchronously writes binary data to the network stream.
    /// </summary>
    /// <remarks>If the parent is in the process of disconnecting, the method will return immediately without
    /// sending data. Logs a warning if the provided data is null or empty.</remarks>
    /// <param name="data">The binary data to be sent. Must not be null or empty.</param>
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
}
