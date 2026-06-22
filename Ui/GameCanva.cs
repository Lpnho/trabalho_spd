using Eto.Drawing;
using Eto.Forms;
using Freeway.Core;
using Freeway.Models;
using Freeway.Singleton;
using System.Text;

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

        float cellWidth = (float)Width / ConfigurationSingleton.NCols;
        float cellHeight = (float)Height / ConfigurationSingleton.NRows;

        for (int i = 0; i < _gameState.Players.Length; i++)
        {
            if (_gameState.Players[i].State == Models.Actions.StateAction.Connected)
            {
                float col = _gameState.Players[i].Column * cellWidth;
                float row = _gameState.Players[i].Row * cellHeight;

                row = Height - row - cellHeight;

                g.DrawImage(_player, col, row, cellWidth, cellHeight);
            }
        }

        for (int i = 0; i < _gameState.Cars.Length; i++)
        {
            float col = _gameState.Cars[i].Column * cellWidth;
            float row = _gameState.Cars[i].Row * cellHeight;

            row = Height - row - cellHeight;

            g.DrawImage(_car, col, row, cellWidth, cellHeight);
        }
    }

    public void UpdateGameState(object? sender, GameState gameState)
    {

        _oldGameState = _gameState;
        _gameState = gameState;

        Application.Instance.AsyncInvoke(() =>
        {
            Invalidate();
        });
        //GameStateMatrix mat = new(ConfigurationSingleton.NRows, ConfigurationSingleton.NCols);

        //for (int i = 0; i < gameState.Cars.Length; i++)
        //{
        //    mat.Set(gameState.Cars[i].Row, gameState.Cars[i].Column, new GameElement(GameElementType.Car, (byte)i));
        //}
        //for (int i = 0; i < gameState.Players.Length; i++)
        //{
        //    mat.Set(gameState.Players[i].Row, gameState.Players[i].Column, new GameElement(GameElementType.Player, (byte)i));
        //}
        //Console.Clear();
        //Console.WriteLine(mat.ToString());
    }
}
