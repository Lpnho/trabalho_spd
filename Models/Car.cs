using Freeway.Models.Actions;
using System.Buffers.Binary;
using System.Drawing;

namespace Freeway.Models;

public struct Car
{
    public int Row { get; set; }
    public int Column { get; set; }
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
        buffer[offSet++] = ColorR;
        buffer[offSet++] = ColorG;
        buffer[offSet] = ColorB;

        return buffer;
    }
    public static Car FromBytes(byte[] data, ref int offSet)
    {
        const int intSize = sizeof(int);
        const int bufferSize = SizeOf;
        if (data.Length - offSet < bufferSize)
            throw new ArgumentException("Buffer inválido");

        int row = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offSet, intSize));
        offSet += intSize;
        int column = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offSet, intSize));
        offSet += intSize;

        byte colorR = data[offSet++];
        byte colorG = data[offSet++];
        byte colorB = data[offSet++];

        return new Car { Row = row, Column = column, ColorR = colorR, ColorG = colorG, ColorB = colorB };
    }
    public const int SizeOf = (2 * sizeof(int)) + 4;

    public Car()
    {
        State = StateAction.Disconnected;
    }
}
