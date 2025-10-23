using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;
using FluffyByte.OPUL.Core.FluffyIO.Networking;

namespace FluffyByte.OPUL.Core.FluffyIO;

public class FluffySystemOperator
{
    private static readonly Lazy<FluffySystemOperator> _instance = new(() => new());
    public static FluffySystemOperator Instance => _instance.Value;

    public List<IFluffyCoreProcess> CoreProcesses { get; private set; } = [];


    // Processes
    public Sentinel Sentinel { get; private set; } = new();

    private readonly List<IFluffyCoreProcess> _coreProcessesStarted = [];

    private readonly CancellationTokenSource _shutdownTokenSource = new();

    private readonly Lock _lock = new();

    private FluffySystemOperator() 
    {
        lock (_lock)
        {
            CoreProcesses.Add(Sentinel);
        }
    }

    public async Task StartAllAsync()
    {
        Scribe.Info("System Operator initializing all core processes...");

        

        foreach(var process in CoreProcesses)
        {
            await process.StartAsync(_shutdownTokenSource.Token);
            _coreProcessesStarted.Add(process);
        }
    }
    
    public async Task ShutdownAsync()
    {
        Scribe.Info($"System Operator is shutting down all core processes...");

        await _shutdownTokenSource.CancelAsync();

        foreach(var process in _coreProcessesStarted.AsEnumerable().Reverse())
        {
            try
            {
                await process.StopAsync();
            }
            catch(Exception ex)
            {
                Scribe.Error($"Error stopping process '{process.Name}'", ex);
                throw;
            }
        }

        ClearLists();
    }

    private void ClearLists()
    {
        CoreProcesses.Clear();
        _coreProcessesStarted.Clear();
    }
}
