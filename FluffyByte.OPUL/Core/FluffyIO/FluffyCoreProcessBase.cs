using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL.Core.FluffyIO;

public abstract class FluffyCoreProcessBase() : IFluffyCoreProcess
{
    public FluffyProcessState State => _state;

    public abstract string Name { get; }

    protected CancellationTokenSource? _internalCancellation = new();

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
           
            _runningTask = Task.Run(async () =>
            {
                try
                {
                    await OnStartAsync();
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
            Scribe.Error($"Failed to start {Name}", ex);
            throw;
        }

        await Task.CompletedTask;
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

            await OnStopAsync();

            _state = FluffyProcessState.Stopped;
            Scribe.Info($"{Name} stopped successfully.");
        }
        catch(Exception ex)
        {
            _state = FluffyProcessState.Error;
            Scribe.Error($"Error stopping: {Name}", ex);
            throw;
        }
    }

    protected abstract Task OnStartAsync();
    protected abstract Task OnStopAsync();
}
