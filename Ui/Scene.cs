using Eto.Drawing;
using Eto.Forms;
using Freeway.Core;
using Freeway.Handlers;
using Freeway.Interfaces;
using Freeway.Interfaces.Network;
using Freeway.Models;
using Freeway.Services;
using Freeway.Singleton;
using Freeway.Workers;

namespace Freeway.Ui;

internal partial class Scene : Panel
{
    private INetworkHandler _networkHandler;
    private INavigationContext _navigationContext;
    private CancellationToken _cancellationToken;

    private KeyboardService _keyboardService = new();

    private Scene(INavigationContext navigationContext,
        INetworkHandler networkHandler,
        CancellationToken cancellationToken = default)
    {
        InitializeComponents();
        _navigationContext = navigationContext;
        _networkHandler = networkHandler;
        _cancellationToken = cancellationToken;
        _navigationContext.KeyDown += _keyboardService.HandleInputEnvent;
        Content = new Bitmap(ConfigurationSingleton.ImageBackgroundPath);
    }
    public static Scene CreateClient(INavigationContext navigationContext,
        INetworkHandler networkHandler,
        CancellationToken cancellationToken = default)
    {
        var result = new Scene(navigationContext, networkHandler, cancellationToken);
        result._keyboardService.OnReceive += (o, arg) => result._networkHandler.Send(arg.Value);
        result._networkHandler.OnReceive += (o, arg) => result.UpdateUI(arg.GameState);
        return result;
    }

    public static Scene CreateServer(INavigationContext navigationContext,
        INetworkHandler networkHandler,
        CancellationToken cancellationToken = default)
    {
        GameStateHandler gameState = new();
        GameWorker gameWorker = new(gameState);
        InputWorker inputWorker = new(gameState);
        CarService carService = new(gameState);

        Scene result = new(navigationContext, networkHandler, cancellationToken);

        result._networkHandler.OnReceive += (o, arg) => inputWorker.AddElement(arg.GameMessage, cancellationToken);
        result._keyboardService.OnReceive += (o, arg) => inputWorker.AddElement(arg, cancellationToken);
        gameWorker.OnStateUpdated += result.Update;

        gameWorker.Start(cancellationToken);
        inputWorker.Start(cancellationToken);
        carService.Start(cancellationToken);

        return result;
    }

    private void UpdateUI(GameState? gameState)
    {

    }

    private void Update(object? sender, GameState gameState)
    {
        _networkHandler.Send(gameState);
        UpdateUI(gameState);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
    }
}
