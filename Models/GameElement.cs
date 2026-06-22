namespace Freeway.Models;

public readonly struct GameElement
{
    public byte Value { get; }

    public GameElementType Type
        => (GameElementType)((Value >> 6) & 0b11);

    public byte ElementId
        => (byte)(Value & 0b0011_1111);

    public GameElement(byte value)
    {
        Value = value;
    }

    public GameElement(GameElementType type, byte elementId)
    {
        Value = (byte)(
            ((byte)type << 6) |
            (elementId & 0b0011_1111)
        );
    }

    public static GameElement None = new(GameElementType.None, 0);

    public static bool operator ==(GameElement left, GameElement right)
        => left.Value == right.Value;

    public static bool operator !=(GameElement left, GameElement right)
        => left.Value != right.Value;

    public static implicit operator byte(GameElement msg)
        => msg.Value;

    public static implicit operator GameElement(byte value)
        => new(value);

    public override string ToString()
        => $"Type={Type}, ElementId={ElementId}";
}