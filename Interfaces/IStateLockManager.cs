using Freeway.Models;

namespace Freeway.Interfaces;

public interface IStateLockManager
{
    void BlockPriority();
    GameState BlockPriorityGetState();
    void UnBlockPriority();
}
