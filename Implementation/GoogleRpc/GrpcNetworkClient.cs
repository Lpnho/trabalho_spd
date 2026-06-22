using Freeway.Core;

using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Network;
using Grpc.Core;
using Grpc.Net.Client;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Freeway.Implementation.GoogleRpc;

public class GrpcNetworkClient :
    INetworkClient
{
    public event EventHandler<Packet>? OnReceive;
    public event Action? OnDisconnect;
    public event Func<Packet>? OnConnect;

    private Channel<Packet> _sentDataChannel = System.Threading.Channels.Channel.CreateUnbounded<Packet>();
    private CancellationTokenSource? _publicTokenSource = new();
    private CancellationTokenSource? _linkedTokenSource = new();
    private Models.ProtoGameService.ProtoGameServiceClient? _client;

    private Task? _sendTask;
    private Task? _receiveTask;
    private bool _running = false;
    private bool _disposed = false;


    public void Connect(IPAddress address, int port, CancellationToken token = default)
    {
        Connect(new IPEndPoint(address, port), token);
    }
    public void Connect(IPEndPoint endPoint, CancellationToken cancellationToken)
    {
        if (_running) throw new InvalidOperationException("Cliente já está conectado.");
        var channel = GrpcChannel.ForAddress($"http://{endPoint.Address.ToString()}:{endPoint.Port}");
        _client = new Models.ProtoGameService.ProtoGameServiceClient(channel);


        _running = true;

        _publicTokenSource = new();
        _linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _publicTokenSource.Token);
        var stream = _client.Connect();

        _receiveTask = HandleReceiveAsync(stream.ResponseStream, _linkedTokenSource.Token);
        _sendTask = HandleSendAsync(stream.RequestStream, _linkedTokenSource.Token);
    }


    public void Disconnect()
    {
        if (!_running) return;
        _running = false;

        _publicTokenSource?.Cancel();

        _receiveTask?.Wait();
        _sendTask?.Wait();

        _linkedTokenSource?.Dispose();
    }
    private async Task HandleReceiveAsync(IAsyncStreamReader<Models.Packet> client, CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var packet in client.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                OnReceive?.Invoke(null, packet.ToApplicationPacket());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            OnDisconnect?.Invoke();
            Disconnect();
        }
    }
    private async Task HandleSendAsync(IClientStreamWriter<Models.Packet> client, CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var data in _sentDataChannel.Reader.ReadAllAsync(cancellationToken))
            {
                await client.WriteAsync(data.ToGrpcPacket()).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Disconnect();
        }
    }
    public void Dispose()
    {
        if (_disposed) return;
        Disconnect();
        _disposed = true;
    }

    public void Send(GameState state, CancellationToken cancellationToken)
    {
        Send(Packet.Create(state), cancellationToken);
    }
    public void Send(Packet data, CancellationToken cancellationToken)
    {
        _ = _sentDataChannel.Writer.WriteAsync(data, cancellationToken);
    }
    public void Send(GameMessage data, CancellationToken cancellationToken)
    {
        Send(Packet.Create(data), cancellationToken);
    }
}
