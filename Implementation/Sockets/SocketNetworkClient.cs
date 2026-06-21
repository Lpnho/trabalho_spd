using Freeway.Core;
using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Network;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Freeway.Implementation.Sockets;
internal class SocketNetworkClient : INetworkClient
{
    public event EventHandler<Packet>? OnReceive;
    private Channel<byte[]> _sentDataChannel = Channel.CreateUnbounded<byte[]>();
    private CancellationTokenSource? _internalTokenSource = new();
    private CancellationTokenSource? _linkedTokenSource = new();
    private Task? _sendTask;
    private Task? _receiveTask;
    private Socket? _socket;
    private bool _running = false;
    private bool _disposed = false;


    public void Connect(IPEndPoint endPoint, CancellationToken cancellationToken)
    {
        if (_running) throw new InvalidOperationException("Cliente já está conectado.");
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(endPoint);
        _running = true;

        _internalTokenSource = new();
        _linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _internalTokenSource.Token);

        _receiveTask = HandleReceiveAsync(_socket, _linkedTokenSource.Token);
        _sendTask = HandleSendAsync(_socket, _linkedTokenSource.Token);

        _receiveTask.ConfigureAwait(false);
        _sendTask.ConfigureAwait(false);
    }
    public void Connect(IPAddress address, int port, CancellationToken token = default)
    {
        Connect(new IPEndPoint(address, port), token);
    }

    public void Connect(Socket socket, CancellationToken cancellationToken)
    {
        if (_running) throw new InvalidOperationException("Cliente já está conectado.");
        if (!socket.Connected) throw new InvalidOperationException("Cliente não está conectado.");
        _socket = socket;
        _running = true;

        _internalTokenSource = new();
        _linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _internalTokenSource.Token);

        _receiveTask = HandleReceiveAsync(_socket, _linkedTokenSource.Token);
        _sendTask = HandleSendAsync(_socket, _linkedTokenSource.Token);

        _receiveTask.ConfigureAwait(false);
        _sendTask.ConfigureAwait(false);

    }
    public void Disconnect()
    {
        if (!_running) throw new InvalidOperationException("Cliente não está conectado.");
        _running = false;

        _internalTokenSource?.Cancel();

        _receiveTask?.Wait();
        _sendTask?.Wait();

        _linkedTokenSource?.Dispose();


        _socket?.Disconnect(false);
        _socket?.Dispose();
    }

    public void Send(byte action)
    {
        _ = Send(MessageSerializer.Serialize(action));
    }

    public void Send(GameState state)
    {
        _ = Send(MessageSerializer.Serialize(state));
    }

    private async Task HandleReceiveAsync(Socket client, CancellationToken cancellationToken = default)
    {
        int decodeBufferIndex = 0;
        byte[] decodeBuffer = new byte[1024];
        byte[] buffer = new byte[1024];
        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            int count = await client.ReceiveAsync(buffer, cancellationToken);
            Buffer.BlockCopy(buffer, 0, decodeBuffer, decodeBufferIndex, count);
            decodeBufferIndex += count;
            while (decodeBufferIndex > 0 && !cancellationToken.IsCancellationRequested)
            {
                Packet pack = MessageSerializer.Deserialize(decodeBuffer, ref decodeBufferIndex);
                OnReceive?.Invoke(null, pack);
            }
        }
    }
    private async Task HandleSendAsync(Socket client, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            byte[] data = await _sentDataChannel.Reader.ReadAsync(cancellationToken);
            await client.SendAsync(data, cancellationToken);
        }
    }
    private async Task Send(byte[] data)
    {
        await _sentDataChannel.Writer.WriteAsync(data);
    }

    public void Dispose()
    {
        if(_disposed) return;
        Disconnect();   
        _disposed = true;
    }
}
