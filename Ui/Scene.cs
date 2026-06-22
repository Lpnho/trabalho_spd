using Eto.Forms;
using Freeway.Interfaces;
using Freeway.Interfaces.Network;
using Freeway.Services;

namespace Freeway.Ui;

public partial class Scene : Panel
{
    private INavigationContext _navigationContext;
    private ClientService _client;
    private GameCanva _canva = new();
    public Scene(INavigationContext navigationContext,
        ClientService client)
    {
        InitializeComponents();
        _client = client;
        _navigationContext = navigationContext;
        _navigationContext.KeyDown += _client.HandleInputEnvent;
        _client.OnStateUpdate += _canva.UpdateGameState;
        _client.OnDisconnect += _navigationContext.NavigatePop;
        Content = _canva;
    }


}
