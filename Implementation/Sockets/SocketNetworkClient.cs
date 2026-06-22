using Freeway.Core;
using Freeway.Implementation.GoogleRpc;
using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Actions;
using Freeway.Models.Network;
using Google.Protobuf;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Freeway.Implementation.Sockets;

public class SocketNetworkClient : INetworkClient
{
    public event EventHandler<Packet>? OnReceive;
    public event Action? OnDisconnect;
    public event Func<Packet>? OnConnect;

    private Channel<byte[]> _sentDataChannel = Channel.CreateUnbounded<byte[]>();
    private CancellationTokenSource? _publicTokenSource = new();
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
        Connect(_socket, cancellationToken);
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

        _publicTokenSource = new();
        _linkedTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _publicTokenSource.Token);

        _receiveTask = HandleReceiveAsync(_socket, _linkedTokenSource.Token);
        _sendTask = HandleSendAsync(_socket, _linkedTokenSource.Token);
    }
    public void Disconnect()
    {
        if (!_running) return;
        _running = false;

        _publicTokenSource?.Cancel();

        _receiveTask?.Wait();
        _sendTask?.Wait();

        _linkedTokenSource?.Dispose();


        _socket?.Disconnect(false);
        _socket?.Dispose();
    }

    private async Task HandleReceiveAsync(Socket client, CancellationToken cancellationToken = default)
    {
        int decodeBufferReadIndex = 0;
        int decodeBufferWriteIndex = 0;

        int decodeBufferCount = 0;

        byte[] decodeBuffer = new byte[4 * 1024];
        byte[] buffer = new byte[4 * 1024];
        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            try
            {
                int lastReadIndex = 0;
                int count = await client.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                do
                {
                    int readBytes = Math.Min(count,
                        decodeBuffer.Length - decodeBufferWriteIndex);

                    count -= readBytes;

                    Buffer.BlockCopy(buffer, lastReadIndex, decodeBuffer, decodeBufferWriteIndex, readBytes);

                    lastReadIndex += readBytes;
                    decodeBufferWriteIndex += readBytes;
                    decodeBufferCount += readBytes;

                    while (!cancellationToken.IsCancellationRequested && decodeBufferCount > 0 && client.Connected)
                    {
                        Packet? pack = MessageSerializer.Deserialize(decodeBuffer,
                            ref decodeBufferReadIndex,
                            ref decodeBufferCount);
                        if (pack == null)
                            break;
                        OnReceive?.Invoke(null, pack);
                    }
                    Buffer.BlockCopy(decodeBuffer, 0, decodeBuffer, decodeBufferReadIndex, decodeBufferCount);
                    decodeBufferReadIndex = 0;
                    decodeBufferWriteIndex = decodeBufferCount;
                } while (lastReadIndex < count && lastReadIndex < buffer.Length && !cancellationToken.IsCancellationRequested);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        OnDisconnect?.Invoke();
        Disconnect();
    }
    private async Task HandleSendAsync(Socket client, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            byte[] data = await _sentDataChannel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            await client.SendAsync(data, cancellationToken).ConfigureAwait(false);
        }
        Disconnect();
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
        //_ = SendAsync(MessageSerializer.Serialize(data), cancellationToken);
        _ = SendAsync(data.ToGrpcPacket().ToByteArray(), cancellationToken);
    }
    public void Send(GameMessage data, CancellationToken cancellationToken)
    {
        Send(Packet.Create(data), cancellationToken);
    }
    public void Send(byte[] data, CancellationToken cancellationToken)
    {
        _ = SendAsync(data, cancellationToken);
    }
    private async Task SendAsync(byte[] data, CancellationToken cancellationToken)
    {
        await _sentDataChannel.Writer.WriteAsync(data, cancellationToken);
    }


}
