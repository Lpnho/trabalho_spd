using Freeway.Models;

namespace Freeway.Interfaces;

internal interface IStateLockManager
{
    void BlockPriority();
    GameState BlockPriorityGetState();
    void UnBlockPriority();
}
