using Atk;
using Freeway.Models;
using Freeway.Models.Network;
using Freeway.Singleton;
using System.Net;

namespace Freeway.Core;

public class MessageSerializer
{
    public static byte[] Serialize(byte action)
    {
        var buffer = new byte[1];
        buffer[0] = (byte)action;
        return buffer;
    }

    public static byte[] Serialize(Packet gameState)
    {
        return gameState.ToBytes();
    }

    public static byte[] Serialize(GameState gameState)
    {
        return gameState.ToBytes();
    }

    public static Packet? Deserialize(byte[] data)
    {
        GameMessage message = new(data[0]);
        MessageType action = message.Type;
        if (action != MessageType.State) return null;
        
        int offSet = 1;
        Player[] players = new Player[ConfigurationSingleton.MaxPlayersCount];
        Car[] car = new Car[ConfigurationSingleton.MaxCarCount];
        for (int i = 0; i < ConfigurationSingleton.MaxPlayersCount; i++)
        {
            players[i] = Player.FromBytes(data, ref offSet);
        }
        for (int i = 0; i < ConfigurationSingleton.MaxCarCount; i++)
        {
            car[i] = Car.FromBytes(data, ref offSet);
        }
        return new Packet
        {
            GameMessage = message,
            GameState = GameState.CreateStateWith(players, car)
        };
    }
}
