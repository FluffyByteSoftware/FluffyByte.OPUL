using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

public class Broadcaster(Watcher watcher)
{
    public Watcher Watcher { get; private set; } = watcher;

    private readonly Sentinel _sentinelRef = watcher.SentinelReference;

    /// <summary>
    /// Asynchronously broadcasts a message to all connected clients.
    /// </summary>
    /// <remarks>This method sends the specified message to each client currently connected. It logs the
    /// message before broadcasting. Ensure that the message is properly formatted and non-empty to avoid unexpected
    /// behavior.</remarks>
    /// <param name="message">The message to be sent to all clients. Cannot be null or empty.</param>
    public async Task BroadcastMessageToAll(string message)
    {
        Scribe.Info($"Broadcasting Message: {message}");

        foreach(var client in Watcher.FluffyClientsConnected.Snapshot())
        { 
            await client.TextIO.WriteLineAsync(message);
        }
    }
}
