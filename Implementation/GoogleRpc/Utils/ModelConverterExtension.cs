using Freeway.Core;
using Freeway.Models;
using Freeway.Models.Actions;
using Freeway.Models.Network;
using Google.Protobuf;
using Google.Protobuf.Collections;

namespace Freeway.Implementation.GoogleRpc.Utils;

internal static class ModelConverterExtension
{
    public static Packet ToApplicationPacket(this Models.Packet packet)
    {
        return new Packet
        {
            GameMessage = (GameMessage)packet.GameMessage,
            GameState = packet.GameState?.ToApplicationGameState()
        };
    }
    public static GameState ToApplicationGameState(this Models.GameState gameState)
    {
        return new GameState(
            gameState.Players.ToGrpcPlayerArray(),
            gameState.Cars.ToGrpcCarArray());
    }
    public static Player[] ToGrpcPlayerArray(this RepeatedField<Models.Player> playerArray)
    {
        return playerArray.Select(p => new Player
        {
            Row = p.Row,
            Column = p.Column,
            Score = p.Score,

            State = (StateAction)((byte)p.State),
            ColorR = (byte)p.ColorR,
            ColorG = (byte)p.ColorG,
            ColorB = (byte)p.ColorB,

        }).ToArray();
    }
    public static Car[] ToGrpcCarArray(this RepeatedField<Models.Car> carArray)
    {
        return carArray.Select(c => new Car
        {
            Row = c.Row,
            Column = c.Column,
            State = (StateAction)c.State,
            ColorR = (byte)c.ColorR,
            ColorG = (byte)c.ColorG,
            ColorB = (byte)c.ColorB,
        }).ToArray();
    }

    public static Models.Packet ToGrpcPacket(this Packet packet)
    {
        return new Models.Packet
        {
            GameMessage = (uint)packet.GameMessage,
            GameState = packet.GameState?.ToGrpcGameState()
        };
    }
    public static Models.GameState ToGrpcGameState(this GameState gameState)
    {
        var result = new Models.GameState();
        result.Players.AddRange(gameState.Players.ToGrpcPlayerArray());
        result.Cars.AddRange(gameState.Cars.ToGrpcCarArray());
        return result;
    }
    public static RepeatedField<Models.Player> ToGrpcPlayerArray(this Player[] playerArray)
    {
        RepeatedField<Models.Player> result = new();
        foreach (Player p in playerArray)
        {
            result.Add(new Models.Player
            {
                Row = p.Row,
                Column = p.Column,
                Score = p.Score,

                State = (uint)p.State,
                ColorR = (byte)p.ColorR,
                ColorG = (byte)p.ColorG,
                ColorB = (byte)p.ColorB,

            });
        }
        return result;
    }
    public static RepeatedField<Models.Car> ToGrpcCarArray(this Car[] playerArray)
    {
        RepeatedField<Models.Car> result = new();
        foreach (Car c in playerArray)
        {
            result.Add(new Models.Car
            {
                Row = c.Row,
                Column = c.Column,
                State = (uint)c.State,
                ColorR = (uint)c.ColorR,
                ColorG = (uint)c.ColorG,
                ColorB = (uint)c.ColorB,
            });
        }
        return result;
    }

}
