using Eto.Forms;
using Freeway.Core;
using Freeway.Interfaces;
using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Models.Network;
using Freeway.Singleton;
using System.Net;

namespace Freeway.Services;

public class ClientService
{
    private byte _clientId;
    INetworkClient _networkClient;
    CancellationToken _cancellationToken;
    KeyboardService _keyboardService = new();
    public ClientService(IPEndPoint endPoint, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _clientId = (byte)ConfigurationSingleton.MaxPlayersCount;
        _networkClient = NetworkProvinderSingleton.Instance!.GetClient();
        _networkClient.OnReceive += OnReceive;
        _networkClient.OnDisconnect += OnDisconnect;
        _keyboardService.OnReceive += SendData;
        _networkClient.Connect(endPoint, _cancellationToken);
    }
    private void OnReceive(object? sender, Packet data)
    {
        if (data.GameMessage.Type == MessageType.Connect)
        {
            _clientId = data.GameMessage.PlayerId;
        }
        else if (data.GameMessage.Type == MessageType.State && data.GameState != null)
        {
            OnStateUpdate?.Invoke(null, data.GameState);
        }
    }
    private void SendData(object? sender, GameMessage message)
    {
        if (_clientId == (byte)ConfigurationSingleton.MaxPlayersCount) return;
        _networkClient.Send(message.WithPlayer(_clientId), _cancellationToken);
    }
    public void HandleInputEnvent(object? sender, KeyEventArgs args)
    {
        _keyboardService.HandleInputEnvent(sender, args);
    }

    public event EventHandler<GameState>? OnStateUpdate;
    
    public event Action? OnDisconnect;
}
