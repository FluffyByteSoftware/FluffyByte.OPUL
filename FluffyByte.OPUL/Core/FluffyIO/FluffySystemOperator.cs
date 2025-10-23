using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO;

public class FluffySystemOperator
{
    private static readonly Lazy<FluffySystemOperator> _instance = new(() => new());
    public static FluffySystemOperator Instance => _instance.Value;


    public List<IFluffyCoreProcess> CoreProcesses { get; private set; } = [];
    private readonly List<IFluffyCoreProcess> _coreProcessesStarted = [];

    private readonly CancellationTokenSource _shutdownTokenSource = new();
    
    private FluffySystemOperator() { }

    public void RegisterCore(IFluffyCoreProcess coreProcess)
    {
        if(CoreProcesses.Any(p => p.Name == coreProcess.Name))
        {
            throw new InvalidOperationException($"Core process '{coreProcess.Name}' is already registered.");
        }

        CoreProcesses.Add(coreProcess);
        Scribe.Debug($"Registered core process: {coreProcess.Name}");
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
    }

    private void ClearLists()
    {
        CoreProcesses.Clear();
        _coreProcessesStarted.Clear();
    }
}
