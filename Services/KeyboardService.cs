using Eto.Forms;
using Freeway.Core;
using Freeway.Models;
using Freeway.Models.Actions;

namespace Freeway.Services;

public class KeyboardService
{
    public void HandleInputEnvent(object? sender, KeyEventArgs args)
    {
        if (OnReceive == null) return;
        HandleControl(args);
        HandleMovement(args);
    }
    private void HandleControl(KeyEventArgs args)
    {
        ControlAction? controlAction = (args.Key) switch
        {
            Keys.Escape => ControlAction.Stop,
            _ => null,
        };

        if (controlAction != null)
        {
            OnReceive?.Invoke(null, new GameMessage(MessageType.Control, (byte)controlAction));
        }
    }
    private void HandleMovement(KeyEventArgs args)
    {
        MovementAction? moveAction = (args.Key) switch
        {
            Keys.W => MovementAction.Up,
            Keys.Up => MovementAction.Up,
            Keys.S => MovementAction.Down,
            Keys.Down => MovementAction.Down,
            _ => null,
        };

        if (moveAction != null)
        {
            OnReceive?.Invoke(null, new GameMessage(MessageType.Movement, (byte)moveAction));
        }
    }

    public event EventHandler<GameMessage>? OnReceive;
}