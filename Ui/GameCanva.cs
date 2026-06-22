using Eto.Drawing;
using Eto.Forms;
using Freeway.Core;
using Freeway.Models;
using Freeway.Singleton;

namespace Freeway.Ui;

public class GameCanva : Drawable
{
    private Bitmap _background = new Bitmap(ConfigurationSingleton.ImageBackgroundPath);
    private Bitmap _player = new(ConfigurationSingleton.ImagePlayerPath);
    private Bitmap _car = new(ConfigurationSingleton.ImageCarPath);

    private GameState? _gameState;
    private GameState? _oldGameState;

    private bool StateHasChanged => _gameState?.GetHashCode() != _oldGameState?.GetHashCode();
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;

        g.DrawImage(_background, 0, 0, Width, Height);
        if (_gameState == null) return;

        for (int i = 0; i < _gameState.Players.Length; i++)
        {
            if (_gameState.Players[i].State == Models.Actions.StateAction.Connected)
            {
                int col = (_gameState.Players[i].Column /
                    ConfigurationSingleton.NCols) * Width;
                int row = Height - (_gameState.Players[i].Row /
                    ConfigurationSingleton.NRows) * Height;
                g.DrawImage(_player, col, row, Width, Height);
            }
        }

        for (int i = 0; i < _gameState.Cars.Length; i++)
        {
            int col = (_gameState.Cars[i].Column /
                ConfigurationSingleton.NCols) * Width;

            int row = Height - (_gameState.Cars[i].Row /
                ConfigurationSingleton.NRows) * Height;
            g.DrawImage(_car, col, row, Width, Height);
        }
    }
    public void UpdateGameState(object? sender, GameState gameState)
    {
        _oldGameState = _gameState;
        _gameState = gameState;
    }
}
