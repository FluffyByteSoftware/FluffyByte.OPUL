using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;
using FluffyByte.OPUL.Core.FluffyIO.Networking.NetClient;
using FluffyByte.OPUL.Tools.FluffyTypes;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;

/// <summary>
/// Manages and monitors the connection of Fluffy clients, providing functionality to register and unregister clients.
/// </summary>
/// <remarks>The <see cref="Watcher"/> class is responsible for maintaining a list of connected Fluffy clients and
/// provides methods to register and unregister these clients. It extends the <see cref="FluffyCoreProcessBase"/> class,
/// inheriting its lifecycle management methods.</remarks>
/// <param name="sentinel">Pass a reference to the Sentinel currently running</param>
public class Watcher(Sentinel sentinel)
{
    public ThreadSafeList<FluffyClient> FluffyClientsConnected { get; private set; } = new();

    public readonly Sentinel SentinelReference = sentinel;

    public int GetClientsConnected
    {
        get
        {
            return FluffyClientsConnected.Count;
        }
    }

    public void RegisterClient(FluffyClient client)
    {
        try
        {
            FluffyClientsConnected.Add(client);
        }
        catch(Exception ex)
        {
            Scribe.Error($"[Watcher] - RegisterClient", ex);
        }
    }


    public void UnregisterClient(FluffyClient client)
    {
        try
        {
            FluffyClientsConnected.Remove(client);
            
        }
        catch(Exception ex)
        {
            Scribe.Error($"[Watcher] - UnregisterClient", ex);
        }
    }

}
