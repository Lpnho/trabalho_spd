using Freeway.Core;
using Freeway.Interfaces;
using Freeway.Singleton;
using Freeway.Workers;

namespace Freeway.Services;

internal class CarService : Worker
{
    private IGameStateUpdater _stateUpdater;
    private CarWorker[] _carWorkers = new CarWorker[ConfigurationSingleton.MaxCarCount];

    public CarService(IGameStateUpdater stateUpdater) : base("Car Service")
    {
        _stateUpdater = stateUpdater;
    }
    protected override void Run(CancellationToken cancellationToken)
    {
        for (int i = 0; i < ConfigurationSingleton.MaxCarCount; ++i)
        {
            _carWorkers[i] = new CarWorker(_stateUpdater, (byte)i, "Car Worker");
            _carWorkers[i].Start(cancellationToken);
        }
        cancellationToken.WaitHandle.WaitOne();
        for (int i = 0; i < ConfigurationSingleton.MaxCarCount; ++i)
        {
            _carWorkers[i].Stop();
        }
    }

}
