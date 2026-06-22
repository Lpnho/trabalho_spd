using Freeway.Core;
using Freeway.Interfaces;
using Freeway.Models;
using Freeway.Models.Actions;
using System.Collections.Concurrent;

namespace Freeway.Workers;

public class InputWorker : Worker
{
    private BlockingCollection<GameMessage> _buffer = new(new ConcurrentQueue<GameMessage>());
    private IGameStateUpdater _stateHandler;
    public InputWorker(IGameStateUpdater stateHandler) : base("Input Worker")
    {
        _stateHandler = stateHandler;
    }

    public void AddElement(GameMessage action, CancellationToken cancellationToken)
    {
        _buffer.Add(action, cancellationToken);
    }
    public GameMessage Pop(CancellationToken cancellationToken)
    {
        return _buffer.Take(cancellationToken);
    }

    protected override void Run(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                GameMessage data = Pop(cancellationToken);
                byte playerId = data.PlayerId;

                if (data.StateAction == StateAction.Disconnected)
                {
                    _stateHandler.DisconnectPlayer(playerId);
                }
                else
                {
                    _stateHandler.UpdatePlayerMovement(playerId, data.MovementAction);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

}
