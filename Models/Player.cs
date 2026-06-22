using Freeway.Models.Actions;
using System.Buffers.Binary;
using System.Drawing;
using System.Net;

namespace Freeway.Models;

public struct Player
{
    public Player()
    {
        State = StateAction.Disconnected;
    }

    public int Row { get; set; }
    public int Column { get; set; }
    public int Score { get; set; }
    public StateAction State { get; set; }
    public byte ColorR { get; set; }
    public byte ColorG { get; set; }
    public byte ColorB { get; set; }

    public byte[] ToBytes()
    {
        const int intSize = sizeof(int);
        const int bufferSize = SizeOf;
        byte[] buffer = new byte[bufferSize];

        int offSet = 0;


        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offSet, intSize), Row);
        offSet += intSize;
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offSet, intSize), Column);
        offSet += intSize;
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(offSet, intSize), Score);
        offSet += intSize;

        buffer[offSet++] = (byte)State;
        buffer[offSet++] = ColorR;
        buffer[offSet++] = ColorG;
        buffer[offSet] = ColorB;

        return buffer;
    }

    public const int SizeOf = (3 * sizeof(int)) + 4;
    public static Player FromBytes(byte[] data, ref int offSet)
    {
        const int intSize = sizeof(int);
        const int bufferSize = SizeOf;

        if (data.Length - offSet < bufferSize)
            throw new ArgumentException("Buffer inválido");

        int row = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offSet, intSize));
        offSet += intSize;
        int column = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offSet, intSize));
        offSet += intSize;
        int score = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offSet, intSize));
        offSet += intSize;

        byte state = data[offSet++];
        byte colorR = data[offSet++];
        byte colorG = data[offSet++];
        byte colorB = data[offSet++];

        return new Player
        {
            Row = row,
            Column = column,
            Score = score,
            State = (StateAction)state,
            ColorR = colorR,
            ColorG = colorG,
            ColorB = colorB
        };
    }
}
