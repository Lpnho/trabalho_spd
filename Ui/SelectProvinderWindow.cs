using Eto.Forms;
using Freeway.Implementation.GoogleRpc;
using Freeway.Implementation.Mqtt;
using Freeway.Implementation.Sockets;
using Freeway.Interfaces;
using Freeway.Singleton;
using Freeway.Workers;

namespace Freeway.Ui;

public partial class SelectProvinderWindow : Panel
{
    private INavigationContext _navigationContext;
    private CancellationToken _cancelationToken;
    private ServerWorker? _worker;
    public SelectProvinderWindow(INavigationContext navigationContext, CancellationToken cancellationToken = default)
    {
        InitializeComponents();
        _navigationContext = navigationContext;
        _cancelationToken = cancellationToken;
        _mqttClientButton.Click += MqttStartGameClick;
        _grpcServerButton.Click += GrpcStartGameClick;
        _socketServerButton.Click += SocketStartGameClick;
    }
    private void GrpcStartGameClick(object? sender, EventArgs args)
    {
        new NetworkProvinderSingleton(new GrpcNetworkProvinder());
        _navigationContext.NavigateTo(new StartWindow(_navigationContext, _cancelationToken));
    }
    private void MqttStartGameClick(object? sender, EventArgs args)
    {
        new NetworkProvinderSingleton(new MqttNetworkProvinder());
        _navigationContext.NavigateTo(new StartWindow(_navigationContext, _cancelationToken));
    }
    private void SocketStartGameClick(object? sender, EventArgs args)
    {
        new NetworkProvinderSingleton(new SocketNetworkProvinder());
        _navigationContext.NavigateTo(new StartWindow(_navigationContext, _cancelationToken));
    }
}
