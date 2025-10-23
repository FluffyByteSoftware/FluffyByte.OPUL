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

    /// <summary>
    /// Asynchronously reads a binary message from the stream, returning the message as a byte array.
    /// </summary>
    /// <remarks>This method reads a 4-byte length prefix to determine the size of the incoming message. It
    /// validates the message length to prevent memory exhaustion. If the message length is invalid, the connection is
    /// terminated. The method handles network exceptions and supports cancellation through a shutdown token.</remarks>
    /// <returns>A task representing the asynchronous operation. The task result contains a byte array with the message data, or
    /// an empty array if the connection is disconnecting or an error occurs.</returns>
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
