using Eto.Drawing;

namespace Freeway.Singleton;

public static class ConfigurationSingleton
{
    public const int DefaultPort = 3333;
    public const int FPS = 60;
    public const string ImageBackgroundPath = "Assets/scene.png";
    public const string ImagePlayerPath = "Assets/player.png";
    public const string ImageCarPath = "Assets/car.png";
    public static readonly Size DefaultSize = new(1024, 768); // Proporção 4:3
    public static readonly Size MinimumSize = new(1024, 768);
    public const int MaxPlayersCount = 0b01 << 3;
    public const int MaxCarCount = 20;
    public const int NRows = 12;
    public const int NCols = 18;

    public const int MaxDificultyLevel = 5;
    public const int MinDificultyLevel = 1;

   
}
