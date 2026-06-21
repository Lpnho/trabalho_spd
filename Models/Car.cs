using Freeway.Models.Actions;
using System.Drawing;

namespace Freeway.Models;

[Serializable]
internal struct Car
{
    public int Row { get; set; }
    public int Column { get; set; }
    public Color Color { get; set; }
}
