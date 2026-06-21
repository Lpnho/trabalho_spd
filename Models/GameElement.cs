namespace Freeway.Models;


internal readonly struct GameElement
{
    public byte Value { get; }

    public GameElementType Type
        => (GameElementType)((Value >> 6) & 0b11);

    public byte ElementId
        => (byte)((Value >> 3) & 0b111);

    public GameElement(byte value)
    {
        Value = value;
    }

    public GameElement(GameElementType type, byte ElementId)
    {
        Value = (byte)(
            (((byte)type & 0b11) << 6) |
            ((ElementId & 0b111) << 3));
    }
    public static GameElement None = new GameElement(GameElementType.None, 0);

    public static bool operator ==(GameElement left, GameElement right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(GameElement left, GameElement right)
    {
        return left.Value != right.Value;
    }


    public static implicit operator byte(GameElement msg)
        => msg.Value;

    public static implicit operator GameElement(byte value)
        => new(value);

    public override string ToString()
    {
        return $"Type={Type}, Elemebt={ElementId}";
    }
}