using Eto.Forms;
using Freeway.Interfaces;
using Freeway.Services;
using Freeway.Singleton;
using System.Net;

namespace Freeway.Ui;

public partial class ConnectWindow : Panel
{
    private INavigationContext _navigationContext;
    private CancellationToken _cancellationToken;
    public ConnectWindow(INavigationContext navigationContext,
        CancellationToken cancellationToken = default, string? ipServerPort = null)
    {
        InitializeComponents();
        _navigationContext = navigationContext;
        _cancellationToken = cancellationToken;
        _cancelButton.Click += CancelClick;
        _startButton.Click += ConnectClick;

        _ipTextBox.Text = ipServerPort ?? String.Empty;
        _portTextBox.Text = ConfigurationSingleton.DefaultPort.ToString();
    }

    private void CancelClick(object? sender, EventArgs args)
    {
        _navigationContext.NavigatePop();
    }
    private void ConnectClick(object? sender, EventArgs args)
    {
        try
        {
            var endPoint = ValidateInput();
            if (endPoint == null) return;
            var client = new ClientService(endPoint, _cancellationToken);
            CleanAllLog();
            _navigationContext.NavigatePop();
            _navigationContext.NavigateTo(new Scene(_navigationContext, client));
        }
        catch
        {
            CleanAllLog();
            _generalLogLabel.Text = "Erro, Algum erro\ninesperado aconteceu!";
        }

    }
    private IPEndPoint? ValidateInput()
    {
        if (!IPAddress.TryParse(_ipTextBox.Text, out var iPAddress))
        {
            _ipLogLabel.Text = "Erro, endereço inválido!";
        }
        if (!int.TryParse(_portTextBox.Text, out var iPPort))
        {
            _portLogLabel.Text = "Erro, porta inválida!";
            return null;
        }
        return (iPAddress == null) ? null : new IPEndPoint(iPAddress, iPPort);
    }
    private void CleanAllLog()
    {
        _ipLogLabel.Text = String.Empty;
        _portLogLabel.Text = String.Empty;
        _generalLogLabel.Text = String.Empty;
    }
}
