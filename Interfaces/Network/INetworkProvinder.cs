namespace Freeway.Interfaces.Network;
internal interface INetworkProvinder
{
    INetworkClient GetClient();
    INetworkServer GetServer();
}
