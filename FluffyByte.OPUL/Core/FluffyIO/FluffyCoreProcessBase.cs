using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO;

public abstract class FluffyCoreProcessBase : IFluffyCoreProcess
{
    public FluffyProcessState State => _state;

    public abstract string Name { get; }

    private CancellationTokenSource? _internalCancellation;

    private FluffyProcessState _state = FluffyProcessState.Stopped;
    private Task? _runningTask;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        switch (_state)
        {
            case FluffyProcessState.Starting:
                Scribe.Warning($"{Name} was already starting!");
                return;
            case FluffyProcessState.Running:
                Scribe.Warning($"{Name} was already running!");
                return;
        }

        try
        {
            _state = FluffyProcessState.Starting;
            Scribe.Info($"Starting {Name}...");

            _internalCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            await OnStartAsync(cancellationToken);

            _runningTask = Task.Run(async () =>
            {
                try
                {
                    await OnStartAsync(_internalCancellation.Token);
                }
                catch(Exception)
                {
                    // Expected during shutdown
                    Scribe.Debug("Exception detected during _runningTask.  Ignoring due to shutdown.");
                }
            }, cancellationToken);

            _state = FluffyProcessState.Running;
            Scribe.Info($"{Name} started successfully.");
        }
        catch(Exception ex)
        {
            _state = FluffyProcessState.Error;
            Scribe.Error(ex, $"Failed to start {Name}");
            throw;
        }
    }

    public async Task StopAsync()
    {
        switch (_state)
        {
            case FluffyProcessState.Stopped:
                Scribe.Warning($"{Name} is already stopped.");
                return;
            case FluffyProcessState.Stopping:
                Scribe.Warning($"{Name} is already stopping.");
                return;
        }

        try
        {
            _state = FluffyProcessState.Stopping;
            Scribe.Info($"Stopping {Name}...");

            _internalCancellation?.Cancel();

            if(_runningTask != null)
            {
                await _runningTask;
            }

            _state = FluffyProcessState.Stopped;
            Scribe.Info($"{Name} stopped successfully.");
        }
        catch(Exception ex)
        {
            _state = FluffyProcessState.Error;
            Scribe.Error(ex, $"Error stopping: {Name}");
            throw;
        }
    }

    protected abstract Task OnStartAsync(CancellationToken ct);
}
