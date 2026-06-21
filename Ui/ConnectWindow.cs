using Eto.Forms;
using Freeway.Interfaces;
using Freeway.Singleton;
using System.Net;

namespace Freeway.Ui;

internal partial class ConnectWindow : Panel
{
    private INavigationContext _navigationContext;
    private CancellationToken _cancellationToken;
    public ConnectWindow(INavigationContext navigationContext,
        CancellationToken cancellationToken = default)
    {
        InitializeComponents();
        _navigationContext = navigationContext;
        _cancellationToken = cancellationToken;
        _cancelButton.Click += CancelClick;
        _startButton.Click += ConnectClick;
        SetDefaultValues();
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
            var result = NetworkProvinderSingleton.Instance!.GetClient();
            result.Connect(endPoint, _cancellationToken);
            CleanAllLog();
            _navigationContext.NavigatePop();
            _navigationContext.NavigateTo(Scene.CreateClient(_navigationContext, result, _cancellationToken));
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
    private void SetDefaultValues()
    {
        _portTextBox.Text = ConfigurationSingleton.DefaultPort.ToString();
    }
}
