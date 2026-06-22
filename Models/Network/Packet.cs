using Freeway.Core;

namespace Freeway.Models.Network;

public class Packet
{
    public Packet() { }
    public Packet(GameMessage gameMessage)
    {
        GameMessage = gameMessage;
    }
    public static Packet Create(GameState state)
    {
        return new Packet { GameMessage = GameMessage.Create(MessageType.State), GameState = state };
    }

    public static Packet Create(GameMessage gameMessage)
    {
        return new Packet(gameMessage);
    }
    public GameMessage GameMessage { get; set; }
    public GameState? GameState { get; set; }
    public byte[] ToBytes()
    {
        byte[] result;
        if (GameState == null)
        {
            result = new byte[1];
            result[0] = (byte)GameMessage;
            return result;
        }
        byte[] state = GameState.ToBytes(1);
        state[0] = (byte)GameMessage;
        return state;
    }
}