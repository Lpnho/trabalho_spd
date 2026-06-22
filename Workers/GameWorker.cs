using Freeway.Core;
using Freeway.Interfaces;
using Freeway.Models;
using Freeway.Singleton;

namespace Freeway.Workers;

public class GameWorker : Worker
{
    private IStateLockManager _stateLock;

    public GameWorker(IStateLockManager stateLock) : base("Game Worker")
    {
        _stateLock = stateLock;
    }

    protected override void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _stateLock.UnBlockPriority();
            Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / ConfigurationSingleton.FPS));
            GameState gameState = _stateLock.BlockPriorityGetState();
            OnStateUpdated?.Invoke(null, gameState);
        }
    }
    public event EventHandler<GameState>? OnStateUpdated;
}
