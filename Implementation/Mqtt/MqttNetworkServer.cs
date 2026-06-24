using Freeway.Core;
using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Network;
using Freeway.Singleton;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

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
    private Task? _sendTask;
    private CancellationTokenSource? _publicTokenSource = new();
    private CancellationTokenSource? _linkedTokenSource = new();
    private Channel<Packet> _sentDataChannel = System.Threading.Channels.Channel.CreateUnbounded<Packet>();

    public void StartService(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        if (_running) return;
        var factory = new MqttServerFactory();
        var options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(endPoint.Port)
            .Build();

        _mqttServer = factory.CreateMqttServer(options);

        _publicTokenSource = new CancellationTokenSource();
        _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_publicTokenSource.Token, cancellationToken);

        _running = true;
        _serverThread = new Thread(Run);
        _serverThread.Name = "Mqtt Server Thread";
        _serverThread.Start();

        _mqttServer.InterceptingPublishAsync += HandleReceiveAsync;
        _sendTask = HandleSendAsync(_linkedTokenSource.Token);
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
        _sendTask?.Wait();
        _linkedTokenSource?.Dispose();
    }
    private void Run()
    {
        _mqttServer!.StartAsync().Wait(_linkedTokenSource!.Token);
        if (OnConnect != null)
        {
            _mqttServer.ValidatingConnectionAsync += HandleConnectClientAsync;
        }
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


    private Task HandleConnectClientAsync(ValidatingConnectionEventArgs args)
    {
        if (OnConnect == null) return Task.CompletedTask;
        try
        {
            byte[] data = OnConnect.Invoke().ToBytes();
            args.ResponseUserProperties ??= new();
            args.ResponseUserProperties.Add(new MQTTnet.Packets.MqttUserProperty("id", data));
            args.ReasonCode = MqttConnectReasonCode.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        return Task.CompletedTask;
    }

    private async Task HandleReceiveAsync(InterceptingPublishEventArgs args)
    {
        if (args.ApplicationMessage.Topic != ConfigurationSingleton.MqttServerWritterChannel)
            return;

        using var bindCancelToken = CancellationTokenSource.CreateLinkedTokenSource(
            _linkedTokenSource!.Token, args.CancellationToken);

        CancellationToken cancellationToken = bindCancelToken.Token;
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
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
                MqttApplicationMessage msg = new MqttApplicationMessageBuilder()
                .WithTopic(ConfigurationSingleton.MqttServerListenerChannel)
                .WithPayload(data.ToBytes())
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();
                await _mqttServer!
                    .InjectApplicationMessage(new(msg), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public void Send(GameMessage data, CancellationToken cancellationToken)
    {
        Send(Packet.Create(data), cancellationToken);
    }
    public void Send(GameState state, CancellationToken cancellationToken)
    {
        Send(Packet.Create(state), cancellationToken);
    }
    public void Send(Packet data, CancellationToken cancellationToken)
    {
        _ = _sentDataChannel.Writer.WriteAsync(data, cancellationToken);
    }
}
