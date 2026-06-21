using Freeway.Interfaces.Network;

namespace Freeway.Implementation.Sockets;
internal class SocketNetworkProvinder : INetworkProvinder
{
    public INetworkClient GetClient()
    {
        return new SocketNetworkClient();
    }

    public INetworkServer GetServer()
    {
        return new SocketNetworkServer();
    }
}
