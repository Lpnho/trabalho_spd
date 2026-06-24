using Freeway.Core;
using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Network;
using MQTTnet.Server;
using System.Net;
using System.Net.Sockets;

namespace Freeway.Implementation.Sockets;

public class MqttNetworkServer : INetworkServer
{
    public event EventHandler<Packet>? OnReceive;
    public event Action? OnDisconnect;
    public event Func<Packet>? OnConnect;

    private bool _running = false;
    private bool _disposed = false;

    private MqttServer? _mqttServer;
    private Thread? _serverThread;

    private CancellationTokenSource? _publicTokenSource = new();
    private CancellationTokenSource? _linkedTokenSource = new();

    private List<SocketNetworkClient> _clients = new();

    public void StartService(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        if (_running) return;
        var factory = new MqttServerFactory();
        var options = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();
        _mqttServer = factory.CreateMqttServer(options);

        _publicTokenSource = new CancellationTokenSource();
        _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_publicTokenSource.Token, cancellationToken);

        _running = true;
        _serverThread = new Thread(Run);
        _serverThread.Name = "Mqtt Server Thread";
        _serverThread.Start();
    }
    public void StartService(IPAddress address, int port, CancellationToken cancellationToken = default)
    {
        StartService(new IPEndPoint(address, port), cancellationToken);
    }
    public void StopService()
    {
        if (!_running) return;
        _running = false;
        _publicTokenSource?.Cancel();
        _serverThread?.Join();
        _linkedTokenSource?.Dispose();

        Parallel.ForEach(_clients,
            (c) => c.Disconnect()
        );
        _clients.Clear();
    }
    private void Run()
    {
        _mqttServer!.StartAsync().Wait(_linkedTokenSource!.Token);
        _linkedTokenSource!.Token.WaitHandle.WaitOne();
        _mqttServer!.StopAsync().Wait();
        OnDisconnect?.Invoke();
    }
    public void Dispose()
    {
        if (_disposed) return;
        StopService();
        _disposed = true;
    }
    public void Send(Packet data, CancellationToken cancellationToken)
    {
        var serializedData = MessageSerializer.Serialize(data);
        Send(serializedData, cancellationToken);
    }
    public void Send(GameMessage data, CancellationToken cancellationToken)
    {
        Send(Packet.Create(data), cancellationToken);
    }
    public void Send(GameState state, CancellationToken cancellationToken)
    {
        Send(Packet.Create(state), cancellationToken);
    }
    public void Send(byte[] data, CancellationToken cancellationToken)
    {
        foreach (var client in _clients)
        {
            client.Send(data, cancellationToken);
        }
    }

}
