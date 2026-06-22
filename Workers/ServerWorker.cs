using Freeway.Core;
using Freeway.Handlers;
using Freeway.Interfaces;
using Freeway.Interfaces.Network;
using Freeway.Models.Network;
using Freeway.Services;
using Freeway.Singleton;
using System.Net;

namespace Freeway.Workers;

public class ServerWorker : Worker
{
    public ServerWorker() : base("Server Worker")
    {
    }

    protected override void Run(CancellationToken cancellationToken)
    {
        INetworkServer server = NetworkProvinderSingleton.Instance!.GetServer();
        server.StartService(new IPEndPoint(IPAddress.Loopback, ConfigurationSingleton.DefaultPort), cancellationToken);


        GameStateHandler gameState = new();
        GameWorker gameWorker = new(gameState);
        InputWorker inputWorker = new(gameState);
        CarService carService = new(gameState);

        server.OnConnect += () => Packet.Create(GameMessage.CreateConnectResponse(gameState.ConnectPlayer()));
        server.OnReceive += (o, arg) => inputWorker.AddElement(arg.GameMessage, cancellationToken);
        gameWorker.OnStateUpdated += (o, arg) => server.Send(arg, cancellationToken);

        gameWorker.Start(cancellationToken);
        inputWorker.Start(cancellationToken);
        carService.Start(cancellationToken);
        cancellationToken.WaitHandle.WaitOne();
    }
}
