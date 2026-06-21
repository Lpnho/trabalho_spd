using Freeway.Models.Actions;

namespace Freeway.Interfaces;

internal interface IGameStateUpdater
{
    void UpdateCarMovement(byte elementId, MovementAction action);
    void UpdatePlayerMovement(byte elementId, MovementAction action);
    void UpdatePlayerState(byte elementId, StateAction action);
    int DifficultyLevel { get; set; }
}
