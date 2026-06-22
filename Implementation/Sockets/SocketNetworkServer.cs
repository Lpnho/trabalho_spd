using Freeway.Core;
using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Network;
using System.Net;
using System.Net.Sockets;

namespace Freeway.Implementation.Sockets;

public class SocketNetworkServer : INetworkServer
{
    public event EventHandler<Packet>? OnReceive;
    public event Action? OnDisconnect;
    public event Func<Packet>? OnConnect;

    private bool _running = false;
    private bool _disposed = false;

    private Socket? _socket;
    private Task? _serverTask;

    private CancellationTokenSource? _publicTokenSource = new();
    private CancellationTokenSource? _linkedTokenSource = new();

    private List<SocketNetworkClient> _clients = new();

    public void StartService(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        if (_running) return;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(endPoint);
        _socket.Listen();
        _running = true;
        _publicTokenSource = new CancellationTokenSource();
        _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_publicTokenSource.Token, cancellationToken);

        _serverTask = Run(cancellationToken);
        _serverTask.ConfigureAwait(false);

    }
    public void StartService(IPAddress address, int port, CancellationToken cancellationToken = default)
    {
        StartService(new IPEndPoint(address, port), cancellationToken);
    }
    public void StopService()
    {
        if (!_running) throw new InvalidOperationException("Server não está conectado.");
        _running = false;
        _publicTokenSource?.Cancel();
        _serverTask?.Wait();
        _linkedTokenSource?.Dispose();

        Parallel.ForEach(_clients,
            (c) => c.Disconnect()
        );
        _socket?.Close();
        _socket?.Dispose();
        _clients.Clear();
    }
    private async Task Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var socket = await _socket!.AcceptAsync(cancellationToken).ConfigureAwait(false);
            var client = new SocketNetworkClient();
            client.OnReceive += OnReceive;
            client.OnConnect += OnConnect;
            //client.OnDisconnect += OnDisconnect;
            _clients.Add(client);
            _ = Task.Run(() =>
            {
                client.Connect(socket, cancellationToken);
                if (OnConnect != null)
                {
                    client.Send(OnConnect.Invoke(), cancellationToken);
                }
            }, cancellationToken).ConfigureAwait(false);
        }
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
