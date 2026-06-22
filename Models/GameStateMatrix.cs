using System.Text;

namespace Freeway.Models;

public class GameStateMatrix
{
    public int Row { get; private set; }
    public int Col { get; private set; }
    public int Length { get => Row * Col; }

    private GameElement[] _data;

    public GameStateMatrix(int row, int col)
    {
        Row = row;
        Col = col;
        _data = new GameElement[Row * Col];
        for (int i = 0; i < _data.Length; i++)
        {
            _data[i] = GameElement.None;
        }
    }

    public GameElement Get(int row, int col)
    {
        return _data[row * Col + col];
    }
    public void Set(int row, int col, GameElement value)
    {
        _data[row * Col + col] = value;
    }
    public string ToString()
    {
        StringBuilder builder = new();

        for (int i = 0; i < Row; i++)
        {
            for (int j = 0; j < Col; j++)
            {
                builder.AppendFormat("{0,4}", (int)(_data[i * Col + j].Type));
            }
            builder.AppendLine();
        }

        return builder.ToString();
    }
}