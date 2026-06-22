using System.Net;

namespace Freeway.Interfaces.Network;
public interface INetworkServer : INetworkHandler
{
    void StartService(IPEndPoint endPoint, CancellationToken cancellationToken);
    void StartService(IPAddress address, int port, CancellationToken cancellationToken);
    void StopService();
}
