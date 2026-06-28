using Freeway.Core;
using Freeway.Implementation.GoogleRpc.Models;
using Freeway.Implementation.GoogleRpc.Utils;
using Freeway.Interfaces.Network;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Net;

using GameState = Freeway.Models.GameState;
using Packet = Freeway.Models.Network.Packet;

namespace Freeway.Implementation.GoogleRpc;

public class GrpcNetworkServer : ProtoGameService.ProtoGameServiceBase, INetworkServer, IDisposable
{
    public event EventHandler<Packet>? OnReceive;
    public event Action? OnDisconnect;
    public event Func<Packet>? OnConnect;
    public event Action<Packet>? OnClientDisconect;

    private bool _running = false;
    private bool _disposed = false;

    private Thread? _serverThread;
    private Server? _server;

    private CancellationTokenSource? _publicTokenSource = new();
    private CancellationTokenSource? _linkedTokenSource = new();

    private readonly ConcurrentDictionary<string, GrpcPersistentConnection> _clients = new();

    public void StartService(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        if (_running) return;

        _server = new Server()
        {
            Ports = { new ServerPort(endPoint.Address.ToString(),
            endPoint.Port, ServerCredentials.Insecure) },
            Services = { ProtoGameService.BindService(this), }
        };

        _publicTokenSource = new CancellationTokenSource();
        _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_publicTokenSource.Token, cancellationToken);

        _serverThread = new Thread(Run);
        _serverThread.Name = "Grpc Server Thread";
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
            (c) => c.Value.CancellationTokenSource.Cancel()
        );
        _clients.Clear();
    }
    private void Run()
    {
        _server!.Start();
        _running = true;
        _linkedTokenSource!.Token.WaitHandle.WaitOne();
        _server.ShutdownAsync().Wait();
    }
    public void Dispose()
    {
        if (_disposed) return;
        StopService();
        _disposed = true;
    }
    public void Send(Packet data, CancellationToken cancellationToken)
    {
        Models.Packet serializedData = data.ToGrpcPacket();
        _ = Send(serializedData, cancellationToken);
    }
    public void Send(GameMessage data, CancellationToken cancellationToken)
    {
        Send(Packet.Create(data), cancellationToken);
    }
    public void Send(GameState state, CancellationToken cancellationToken)
    {
        Send(Packet.Create(state), cancellationToken);
    }
    public async Task Send(Models.Packet packet, CancellationToken ct)
    {
        var tasks = new List<Task>(_clients.Count);

        foreach (var client in _clients.Values)
        {
            if (client.CancellationTokenSource.IsCancellationRequested)
                continue;

            tasks.Add(client.Stream.WriteAsync(packet));
        }

        await Task.WhenAll(tasks);
    }
    public override async Task Connect(IAsyncStreamReader<Models.Packet> requestStream, IServerStreamWriter<Models.Packet> responseStream, ServerCallContext context)
    {
        if (OnConnect == null) throw new InvalidOperationException("Erro callback de conexão não cadastrada!");

        CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _publicTokenSource!.Token,
            context.CancellationToken
            );

        var clientId = context.Peer;

        Packet connectPacket = OnConnect.Invoke();

        if (connectPacket != null)
        {
            await responseStream.WriteAsync(connectPacket.ToGrpcPacket());
        }

        var connection = new GrpcPersistentConnection(responseStream, tokenSource);

        var old = _clients.AddOrUpdate(
            clientId,
            _ => connection,
            (_, existing) =>
            {
                existing.CancellationTokenSource.Cancel();
                return connection;
            }
        );

        try
        {
            await foreach (var request in requestStream.ReadAllAsync(tokenSource.Token))
            {
                OnReceive?.Invoke(this, request.ToApplicationPacket());
            }
        }
        finally
        {
            _clients.TryRemove(clientId, out _);
            OnClientDisconect?.Invoke(connectPacket!);
            OnDisconnect?.Invoke();
        }
    }
}
