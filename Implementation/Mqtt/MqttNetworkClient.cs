using Freeway.Core;
using Freeway.Implementation.GoogleRpc;
using Freeway.Implementation.GoogleRpc.Utils;
using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Actions;
using Freeway.Models.Network;
using Freeway.Singleton;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using MQTTnet;
using MQTTnet.Formatter;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Freeway.Implementation.Mqtt;

public class MqttNetworkClient : INetworkClient
{
    public event EventHandler<Packet>? OnReceive;
    public event Action? OnDisconnect;
    public event Func<Packet>? OnConnect;

    private Channel<Packet> _sentDataChannel = System.Threading.Channels.Channel.CreateUnbounded<Packet>();
    private CancellationTokenSource? _publicTokenSource = new();
    private CancellationTokenSource? _linkedTokenSource = new();
    private IMqttClient? _mqttClient;

    private bool _running = false;
    private bool _disposed = false;
    private Task? _sendTask;

    public void Connect(IPAddress address, int port, CancellationToken token = default)
    {
        Connect(new IPEndPoint(address, port), token);
    }
    public void Connect(IPEndPoint endPoint, CancellationToken cancellationToken)
    {
        if (_running) throw new InvalidOperationException("Cliente já está conectado.");

        var mqttFactory = new MqttClientFactory();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(endPoint.Address.ToString(), endPoint.Port)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();

        _mqttClient = mqttFactory.CreateMqttClient();
        _running = true;
        _publicTokenSource = new();
        _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _publicTokenSource.Token);

        

        var response = _mqttClient.ConnectAsync(mqttClientOptions, _linkedTokenSource.Token).Result;
        byte data = response.UserProperties.
            First(prop => prop.Name.Equals("id")).ValueBuffer.ToArray().First();
        OnReceive?.Invoke(null, Packet.Create((GameMessage)data));

        _mqttClient.SubscribeAsync(ConfigurationSingleton.MqttServerListenerChannel);
        _mqttClient.SubscribeAsync(ConfigurationSingleton.MqttServerWritterChannel);

        _mqttClient.ApplicationMessageReceivedAsync += HandleReceiveAsync;
        _mqttClient.DisconnectedAsync += (e) => Task.Run(() => OnDisconnect!.Invoke());

        _sendTask = HandleSendAsync(_linkedTokenSource.Token);
    }

    public void Disconnect()
    {
        if (!_running) return;
        _running = false;

        _publicTokenSource?.Cancel();
        _sendTask?.Wait();
        _linkedTokenSource?.Dispose();
    }

    private async Task HandleReceiveAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        CancellationToken cancellationToken = _linkedTokenSource!.Token;
        try
        {
            const int headerSize = 1;
            byte[] buffer = new byte[4 * 1024];
            using MemoryStream stream = new(args.ApplicationMessage.Payload.ToArray());

            int bufferOffSet = 0;
            await stream.ReadExactlyAsync(buffer, bufferOffSet, headerSize, cancellationToken).ConfigureAwait(false);
            GameMessage header = (GameMessage)buffer[0];

            if (header.Type == MessageType.State)
            {
                bufferOffSet += headerSize;
                await stream.ReadExactlyAsync(buffer, bufferOffSet, GameState.SizeOf, cancellationToken).ConfigureAwait(false);
                Packet? pack = MessageSerializer.Deserialize(buffer);
                if (pack != null)
                {
                    OnReceive?.Invoke(null, pack);
                }
            }
            else
            {
                OnReceive?.Invoke(null, Packet.Create(header));
            }
        }
        catch (Exception)
        {
            //Console.WriteLine(ex);
        }
        finally
        {
            OnDisconnect?.Invoke();
        }
    }
    private async Task HandleSendAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var data in _sentDataChannel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                await _mqttClient!.PublishBinaryAsync(ConfigurationSingleton.MqttServerWritterChannel,
                    data.ToBytes()).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            //Console.WriteLine(ex);
        }
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
    public void Dispose()
    {
        if (_disposed) return;
        Disconnect();
        _disposed = true;
    }
}
