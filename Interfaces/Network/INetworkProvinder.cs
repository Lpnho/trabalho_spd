namespace Freeway.Interfaces.Network;
public interface INetworkProvinder
{
    INetworkClient GetClient();
    INetworkServer GetServer();
}
