using Freeway.Interfaces.Network;

namespace Freeway.Implementation.GoogleRpc;
public class GrpcNetworkProvinder : INetworkProvinder
{
    public INetworkClient GetClient()
    {
        return new GrpcNetworkClient();
    }

    public INetworkServer GetServer()
    {
        return new GrpcNetworkServer();
    }
}
