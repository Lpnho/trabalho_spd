using Freeway.Models;
using Freeway.Models.Network;
using System.Net;

namespace Freeway.Core;

internal class MessageSerializer
{
    public static byte[] Serialize(byte action)
    {
        var buffer = new byte[1];
        buffer[0] = (byte)action;
        return buffer;
    }

    public static byte[] Serialize(GameStateMatrix state)
    {
        byte[] payload = state.ToByteArray();
        int payloadSize = payload.Length;

        byte[] data = new byte[
            sizeof(byte) +
            sizeof(int) +
            sizeof(uint) +
            sizeof(uint) +
            payloadSize];

        int offset = 0;

        data[offset++] = (byte)MessageType.State;

        BitConverter.GetBytes(
            IPAddress.HostToNetworkOrder(payloadSize))
            .CopyTo(data, offset);

        offset += sizeof(int);

        BitConverter.GetBytes(
            IPAddress.HostToNetworkOrder((int)state.Row))
            .CopyTo(data, offset);

        offset += sizeof(int);

        BitConverter.GetBytes(
            IPAddress.HostToNetworkOrder((int)state.Col))
            .CopyTo(data, offset);

        offset += sizeof(int);

        Buffer.BlockCopy(
            payload,
            0,
            data,
            offset,
            payloadSize);

        return data;
    }

    public static Packet Deserialize(byte[] data, ref int length)
    {
        if (data == null || data.Length == 0 || length < 1)
            throw new ArgumentException("Pacote inválido.");

        GameMessage message = new(data[0]);
        MessageType action = message.Type;

        if (action != MessageType.State)
            return new Packet { GameMessage = new(data[0]) };

        int offset = 1;

        int payloadSize = IPAddress.NetworkToHostOrder(
            BitConverter.ToInt32(data, offset));

        offset += sizeof(int);

        uint row = (uint)IPAddress.NetworkToHostOrder(
            BitConverter.ToInt32(data, offset));

        offset += sizeof(int);

        uint col = (uint)IPAddress.NetworkToHostOrder(
            BitConverter.ToInt32(data, offset));

        offset += sizeof(int);

        if (data.Length < offset + payloadSize)
            throw new InvalidOperationException(
                "Tamanho do payload inválido.");
        length -= (offset + (int)(col * row));
        return new Packet
        {
            GameMessage = message,
            //GameState = new GameStateMatrix(row, col, data, offset)
        };
    }

    public static byte[] Serialize(GameState gameState)
    {
        return null;
    }
}
