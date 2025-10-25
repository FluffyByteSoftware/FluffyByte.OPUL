using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;
using FluffyByte.OPUL.Core.FluffyIO.Networking;

namespace FluffyByte.OPUL.Core.FluffyIO;


/// <summary>
/// Manages the lifecycle of core processes within the Fluffy system, providing functionality to start and shut down all
/// processes in a controlled manner.
/// </summary>
/// <remarks>This singleton class ensures that all core processes are initialized and terminated properly,
/// maintaining system stability. It provides thread-safe operations for managing the processes and handles exceptions
/// during shutdown to ensure all processes are stopped gracefully.</remarks>
public class FluffySystemOperator
{
    private static readonly Lazy<FluffySystemOperator> _instance = new(() => new());
    public static FluffySystemOperator Instance => _instance.Value;

    public List<IFluffyCoreProcess> CoreProcesses { get; private set; } = [];

    public Sentinel? Sentinel { get; private set; }

    private readonly List<IFluffyCoreProcess> _coreProcessesStarted = [];

    private readonly CancellationTokenSource _shutdownTokenSource = new();

    private readonly Lock _lock = new();

    private FluffySystemOperator() 
    {
    }

    public async Task StartAllAsync()
    {
        ClearLists();

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

        Sentinel = new(_shutdownTokenSource.Token);

        CoreProcesses.Add(Sentinel);
    }
}
