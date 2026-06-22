namespace Freeway.Models;

public class GameStateMatrix
{
    public uint Row { get; private set; }
    public uint Col { get; private set; }
    public uint Length { get => Row * Col; }

    private GameElement[] _data;

    public GameStateMatrix(uint row, uint col)
    {
        Row = row;
        Col = col;
        _data = new GameElement[Row * Col];
    }

    public GameStateMatrix(uint row, uint col, byte[] data, int offSetBegin = 0) :
        this(row, col)
    {
        if (data.Length < row * col)
        {
            throw new ArgumentException("Dados Insuficientes para a criação do estado");
        }
        Update(data, offSetBegin);
    }

    public void Update(byte[] data, int offSetBegin = 0)
    {
        if ((data.Length - offSetBegin) < data.Length)
            throw new ArgumentException("Dados Insuficientes para a criação do estado");
        Buffer.BlockCopy(data, offSetBegin, _data, 0, data.Length);
    }


    public GameElement Get(int row, int col)
    {
        return _data[row * Col + col];
    }
    public void Set(int row, int col, GameElement value)
    {
        _data[row * Col + col] = value;
    }
    public byte[] ToByteArray()
    {
        return _data.Select(e => (byte)e).ToArray();
    }

    public GameStateMatrix Clone()
    {
        GameStateMatrix result = new GameStateMatrix(Row, Col);
        Buffer.BlockCopy(_data, 0, result._data, 0, (int)result.Length);
        return result;
    }
}
