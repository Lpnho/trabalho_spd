using Eto.Drawing;

namespace Freeway.Singleton;

internal static class ConfigurationSingleton
{
    public const int DefaultPort = 3333;
    public const int FPS = 60;
    public const string ImageBackgroundPath = "Assets/scene.png";
    public static readonly Size DefaultSize = new(1024, 768); // Proporção 4:3
    public static readonly Size MinimumSize = new(1024, 768);
    public static readonly Size GameMatrixSize = new(1024, 768);
    public const uint MaxPlayersCount = 0b01 << 3;
    public const uint MaxCarCount = 20;
    public const uint NRows = 12;
    public const uint NCols = 18;

    public const int MaxDificultyLevel = 5;
    public const int MinDificultyLevel = 1;

   
}
