using Freeway.Core;
using Freeway.Interfaces;
using Freeway.Models.Actions;
using Freeway.Singleton;

namespace Freeway.Workers;

public class CarWorker(IGameStateUpdater _stateUpdater, string? threadName = null) : Worker(threadName)
{
    protected override void Run(CancellationToken cancellationToken)
    {
        byte _carId = _stateUpdater.ConnectCar();
        if (_carId == ConfigurationSingleton.MaxCarCount) return;
        while (!cancellationToken.IsCancellationRequested)
        {
            int difficulty = _stateUpdater.DifficultyLevel;

            if (Random.Shared.Next(1, 6) <= difficulty)
            {
                _stateUpdater.UpdateCarMovement(_carId, MovementAction.Right);
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / ConfigurationSingleton.FPS));
        }
    }

}
