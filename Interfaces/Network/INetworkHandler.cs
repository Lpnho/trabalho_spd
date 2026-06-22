using Freeway.Core;
using Freeway.Models;
using Freeway.Models.Network;

namespace Freeway.Interfaces.Network;

public interface INetworkHandler : IDisposable
{
    void Send(Packet data, CancellationToken cancellationToken);
    void Send(GameState data, CancellationToken cancellationToken);
    void Send(GameMessage data, CancellationToken cancellationToken);
    void Send(byte[] data, CancellationToken cancellationToken);

    event EventHandler<Packet> OnReceive;
    event Action OnDisconnect;
    event Func<Packet> OnConnect;
}