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
public class Watcher(Sentinel sentinel) : FluffyCoreProcessBase
{
    public ThreadSafeList<FluffyClient> FluffyClientsConnected { get; private set; } = new();

    public readonly Sentinel SentinelReference = sentinel;


    public override string Name => "Watcher";

    protected override async Task OnStartAsync()
    {
        FluffyClientsConnected.Clear();
        await Task.CompletedTask;
    }


    protected override async Task OnStopAsync()
    {
        await Task.CompletedTask;
    }

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
            Scribe.Error($"[{Name}] - RegisterClient", ex);
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
            Scribe.Error($"[{Name}] - UnregisterClient", ex);
        }
    }

}
