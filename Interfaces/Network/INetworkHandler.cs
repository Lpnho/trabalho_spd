using Freeway.Models;
using Freeway.Models.Network;

namespace Freeway.Interfaces.Network;

internal interface INetworkHandler : IDisposable
{
    void Send(byte action);
    void Send(GameState state);

    event EventHandler<Packet> OnReceive;

}