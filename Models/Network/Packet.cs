using Freeway.Core;

namespace Freeway.Models.Network;

internal class Packet
{
    public GameMessage GameMessage { get; set; }
    public GameState? GameState { get; set; }
}