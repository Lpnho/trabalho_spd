using Freeway.Interfaces.Network;

namespace Freeway.Singleton;
internal class NetworkProvinderSingleton
{
    public static INetworkProvinder? Instance { get; private set; }
    public NetworkProvinderSingleton(INetworkProvinder connectionProvinder)
    {
        Instance = connectionProvinder;
    }

}
