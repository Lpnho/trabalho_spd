using System.Net;
using System.Net.Sockets;

namespace Freeway.Interfaces.Network;
public interface INetworkClient : INetworkHandler
{
    void Connect(IPEndPoint endPoint, CancellationToken cancellationToken);
    void Connect(IPAddress address, int port, CancellationToken cancellationToken);
    void Connect(Socket socket, CancellationToken cancellationToken);
    void Disconnect();
}
