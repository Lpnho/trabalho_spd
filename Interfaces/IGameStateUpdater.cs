using Freeway.Models.Actions;

namespace Freeway.Interfaces;

public interface IGameStateUpdater
{
    void UpdateCarMovement(byte elementId, MovementAction action);
    void UpdatePlayerMovement(byte elementId, MovementAction action);
    byte ConnectPlayer();
    void DisconnectPlayer(byte playerId);
    int DifficultyLevel { get; set; }
}
