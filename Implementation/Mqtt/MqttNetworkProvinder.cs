using Freeway.Interfaces.Network;

namespace Freeway.Implementation.Sockets;
public class MqttNetworkProvinder : INetworkProvinder
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
