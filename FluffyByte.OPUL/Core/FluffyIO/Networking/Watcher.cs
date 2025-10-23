using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPUL.Core.FluffyIO.Networking.FluffyClients;

namespace FluffyByte.OPUL.Core.FluffyIO.Networking;
public class Watcher(Sentinel sentinel) : FluffyCoreProcessBase
{
    public List<FluffyRawClient> RawClientsConnected { get; private set; } = [];
    public List<IFluffyClient> AllFluffyClientsConnected { get; private set; } = [];

    private readonly Sentinel _sentinelRef = sentinel;

    public override string Name => "Watcher";

    protected override async Task OnStartAsync()
    {
        await Task.CompletedTask;
    }


    protected override async Task OnStopAsync()
    {
        await Task.CompletedTask;
    }

    public void ClearAll()
    {
        
        RawClientsConnected.Clear();
        AllFluffyClientsConnected.Clear();
    }
}
