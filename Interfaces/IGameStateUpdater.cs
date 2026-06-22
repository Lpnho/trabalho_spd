using Freeway.Models;
using Freeway.Models.Actions;
using Freeway.Singleton;

namespace Freeway.Interfaces;

public interface IGameStateUpdater
{
    void UpdateCarMovement(byte elementId, MovementAction action);
    void UpdatePlayerMovement(byte elementId, MovementAction action);
    byte ConnectPlayer();
    public byte ConnectCar();
    void DisconnectPlayer(byte playerId);
    public void DisconnectCar(byte playerId);

    int DifficultyLevel { get; set; }
}
