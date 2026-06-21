using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Network;
using System.Net;
using System.Net.Sockets;

namespace Freeway.Implementation.Sockets;
internal class SocketNetworkServer : INetworkServer
{
    public event EventHandler<Packet>? OnReceive;
    private bool _running = false;
    private bool _disposed = false;

    private Socket? _socket;
    private Task? _serverTask;

    private CancellationTokenSource? _internalTokenSource = new();
    private CancellationTokenSource? _linkedTokenSource = new();

    private List<SocketNetworkClient> _clients = new();


    public void StartService(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        if (_running) return;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(endPoint);
        _socket.Listen();
        _running = true;

        _internalTokenSource = new CancellationTokenSource();
        _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_internalTokenSource.Token, cancellationToken);

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
        _internalTokenSource?.Cancel();
        _serverTask?.Wait();
        _linkedTokenSource?.Dispose();

        Parallel.ForEach(_clients,
            (c) => c.Disconnect()
        );
        _socket?.Close();
        _socket?.Dispose();
        _clients.Clear();
    }

    public void Send(byte action)
    {
        foreach (var client in _clients)
        {
            client.Send(action);
        }
    }

    public void Send(GameState state)
    {
        foreach (var client in _clients)
        {
            client.Send(state);
        }
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var socket = await _socket!.AcceptAsync(cancellationToken).ConfigureAwait(false);
            var client = new SocketNetworkClient();
            client.OnReceive += OnReceive;
            client.Connect(socket, cancellationToken);
            _clients.Add(client);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        StopService();
        _disposed = true;
    }
}
