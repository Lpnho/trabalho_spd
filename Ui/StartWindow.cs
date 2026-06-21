using Eto.Forms;
using Freeway.Interfaces;
using Freeway.Singleton;
using Gtk;
using System.Net;

namespace Freeway.Ui;
internal partial class StartWindow : Panel
{
    private INavigationContext _navigationContext;
    private CancellationToken _cancelationToken;
    public StartWindow(INavigationContext navigationContext, CancellationToken cancellationToken = default)
    {
        InitializeComponents();
        _navigationContext = navigationContext;
        _cancelationToken = cancellationToken;
        _startClientButton.Click += StartGameClick;
        _startServerButton.Click += StartServerClick;
    }
    private void StartGameClick(object? sender, EventArgs args)
    {
        _navigationContext.NavigateTo(new ConnectWindow(_navigationContext, _cancelationToken));
    }
    private void StartServerClick(object? sender, EventArgs args)
    {
        var server = NetworkProvinderSingleton.Instance!.GetServer();
        server.StartService(new IPEndPoint(IPAddress.Loopback, ConfigurationSingleton.DefaultPort), _cancelationToken);
        _navigationContext.NavigateTo(Scene.CreateServer(_navigationContext, server, _cancelationToken));
    
    }
}
