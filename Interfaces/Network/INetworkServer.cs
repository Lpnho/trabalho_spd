using System.Net;

namespace Freeway.Interfaces.Network;
internal interface INetworkServer : INetworkHandler
{
    void StartService(IPEndPoint endPoint, CancellationToken cancellationToken);
    void StartService(IPAddress address, int port, CancellationToken cancellationToken);
    void StopService();
}
