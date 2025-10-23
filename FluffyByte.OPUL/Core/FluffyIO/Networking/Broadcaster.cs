using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

public class Broadcaster(Watcher watcher)
{
    public Watcher Watcher { get; private set; } = watcher;

    private readonly Sentinel _sentinelRef = watcher.SentinelReference;

    public async Task BroadcastMessageToAll(string message)
    {
        Scribe.Info($"Broadcasting Message: {message}");

        foreach(var client in Watcher.FluffyClientsConnected.Snapshot())
        { 
            await client.TextIO.WriteLineAsync(message);
        }
    }
}
