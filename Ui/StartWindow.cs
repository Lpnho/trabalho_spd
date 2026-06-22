using Eto.Forms;
using Freeway.Interfaces;
using Freeway.Singleton;
using Freeway.Workers;
using Gtk;
using System.Net;

namespace Freeway.Ui;

public partial class StartWindow : Panel
{
    private INavigationContext _navigationContext;
    private CancellationToken _cancelationToken;
    private ServerWorker? _worker;
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
        _worker?.Stop();
        _navigationContext.NavigateTo(new ConnectWindow(_navigationContext, _cancelationToken));
    }
    private void StartServerClick(object? sender, EventArgs args)
    {
        _worker?.Stop();
        _worker = new ServerWorker();
        _worker.Start(_cancelationToken);
        _navigationContext.NavigateTo(new ConnectWindow(_navigationContext, _cancelationToken, IPAddress.Loopback.ToString()));
    }
}
