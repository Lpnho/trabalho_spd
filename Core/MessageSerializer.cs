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

    public static Packet? Deserialize(byte[] data, ref int offSet, ref int readCount)
    {
        if (data == null || data.Length == 0 || readCount < 1)
            throw new ArgumentException("Pacote inválido.");

        GameMessage message = new(data[0]);
        MessageType action = message.Type;

        if (action == MessageType.Connect || action == MessageType.Control)
        {
            readCount -= 1;
            offSet += 1;
            return new Packet { GameMessage = new(data[0]) };
        }

        if (action != MessageType.State ||
            readCount < (ConfigurationSingleton.MaxPlayersCount * Player.SizeOf +
            ConfigurationSingleton.MaxCarCount * Car.SizeOf + 1))
        {
            return null;
        }

        offSet++;
        readCount--;

        Player[] players = new Player[ConfigurationSingleton.MaxPlayersCount];
        Car[] car = new Car[ConfigurationSingleton.MaxCarCount];
        for (int i = 0; i < ConfigurationSingleton.MaxPlayersCount; i++)
        {
            players[i] = Player.FromBytes(data, ref offSet);
            readCount -= Player.SizeOf;
        }
        for (int i = 0; i < ConfigurationSingleton.MaxCarCount; i++)
        {
            car[i] = Car.FromBytes(data, ref offSet);
            readCount -= Car.SizeOf;
        }
        return new Packet
        {
            GameMessage = message,
            GameState = GameState.CreateStateWith(players, car)
        };
    }
}
