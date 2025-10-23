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

    /// <summary>
    /// Initializes a new instance of the <see cref="TextIO"/> class for reading from and writing to a network stream.
    /// </summary>
    /// <remarks>The <see cref="TextIO"/> class provides UTF-8 encoded text reading and writing capabilities
    /// over the specified network stream. The <see cref="StreamWriter"/> is configured to automatically flush its
    /// buffer after each write operation.</remarks>
    /// <param name="parent">The <see cref="FluffyClient"/> instance that owns this <see cref="TextIO"/> object.</param>
    /// <param name="parentStream">The <see cref="NetworkStream"/> used for communication.</param>
    /// <param name="shutdownToken">A <see cref="CancellationToken"/> used to signal shutdown operations.</param>
    public TextIO(FluffyClient parent, NetworkStream parentStream, CancellationToken shutdownToken)
    {
        _parent = parent;
        _parentStream = parentStream;
        _shutdownToken = shutdownToken;

        _reader = new StreamReader(parentStream, encoding: Encoding.UTF8, false);
        _writer = new StreamWriter(parentStream, encoding: Encoding.UTF8) { AutoFlush = true };
    }

    /// <summary>
    /// Asynchronously writes a line of text to the underlying stream.
    /// </summary>
    /// <remarks>If the parent is in the process of disconnecting, the method returns immediately without
    /// writing. Handles any network-related exceptions by invoking the parent's exception handler.</remarks>
    /// <param name="message">The message to write. Cannot be null.</param>
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

    /// <summary>
    /// Asynchronously reads a line of text from the underlying stream.
    /// </summary>
    /// <remarks>This method returns an empty string if the parent is disconnecting or if the read line is
    /// null or whitespace. It handles network exceptions internally and logs them using the parent's exception handling
    /// mechanism.</remarks>
    /// <returns>A task representing the asynchronous read operation. The task result contains the line of text read from the
    /// stream, or an empty string if no valid line is read.</returns>
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
