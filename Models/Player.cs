using Freeway.Models.Actions;
using System.Drawing;

namespace Freeway.Models;

[Serializable]
internal struct Player
{
    public int Row { get; set; }
    public int Column { get; set; }
    public int Score { get; set; }
    public StateAction State { get; set; }
    public Color Color { get; set; }
}
