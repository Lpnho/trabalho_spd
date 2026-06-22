using Freeway.Models;
using Freeway.Models.Actions;

namespace Freeway.Core;

public readonly struct GameMessage
{
    public byte Value { get; }

    public MessageType Type
        => (MessageType)((Value >> 6) & 0b11);

    public byte PlayerId
        => (byte)((Value >> 3) & 0b111);

    public byte Action
        => (byte)(Value & 0b111);

    public MovementAction MovementAction
        => (MovementAction)Action;

    public ControlAction ControlAction
        => (ControlAction)Action;

    public StateAction StateAction
        => (StateAction)Action;

    public GameMessage(byte value)
    {
        Value = value;
    }

    public GameMessage(
        MessageType type,
        byte playerId,
        byte action)
    {
        Value = (byte)(
            (((byte)type & 0b11) << 6) |
            ((playerId & 0b111) << 3) |
            (action & 0b111));
    }

    public GameMessage(
        MessageType type,
        byte action)
    {
        Value = (byte)(
            (((byte)type & 0b11) << 6) |
            (action & 0b111));
    }

    public GameMessage WithPlayer(byte playerId)
    {
        return new GameMessage(
            (byte)(
                (Value & 0b11000111) |
                ((playerId & 0b111) << 3)));
    }

    public static GameMessage Create(
        MessageType type,
        byte playerId,
        byte action)
    {
        return new(type, playerId, action);
    }

    public static GameMessage CreateConnectResponse(
        byte playerId)
    {
        return new(MessageType.Connect, playerId, 0);
    }
    public static implicit operator byte(GameMessage msg)
        => msg.Value;

    public static implicit operator GameMessage(byte value)
        => new(value);

    public override string ToString()
    {
        return $"Type={Type}, Player={PlayerId}, Action={Action}";
    }
}