using Freeway.Singleton;

namespace Freeway.Models;

public class GameState
{
    public Player[] Players { get; private set; } = new Player[ConfigurationSingleton.MaxPlayersCount];
    public Car[] Cars { get; private set; } = new Car[ConfigurationSingleton.MaxCarCount];
    private GameState() { }
    public GameState(Player[] players, Car[] cars)
    {
        if (players.Length != ConfigurationSingleton.MaxPlayersCount ||
            cars.Length != ConfigurationSingleton.MaxCarCount)
            throw new ArgumentException($"Quantidade de carros: {cars.Length} " +
                $"ou players:{players.Length} inválida");

        for (int i = 0; i < players.Length; i++)
        {
            Players[i] = players[i];
        }
        for (int i = 0; i < cars.Length; i++)
        {
            Cars[i] = cars[i];
        }
    }

    public static GameState CreateStateWith(Player[] players, Car[] cars)
    {
        if (players.Length != ConfigurationSingleton.MaxPlayersCount ||
            cars.Length != ConfigurationSingleton.MaxCarCount)
            throw new ArgumentException($"Quantidade de carros: {cars.Length} " +
                $"ou players:{players.Length} inválida");
        return new GameState { Cars = cars, Players = players };
    }
    public byte[] ToBytes(int offSetBegin = 0)
    {
        byte[] result = new byte[offSetBegin + SizeOf];
        int offSet = offSetBegin;
        for (int i = 0; i < Players.Length; i++, offSet += Player.SizeOf)
        {
            Buffer.BlockCopy(Players[i].ToBytes(), 0, result, offSet, Player.SizeOf);
        }

        for (int i = 0; i < Cars.Length; i++, offSet += Car.SizeOf)
        {
            Buffer.BlockCopy(Cars[i].ToBytes(), 0, result, offSet, Car.SizeOf);
        }
        return result;
    }
    public const int SizeOf = ConfigurationSingleton.MaxPlayersCount * Player.SizeOf
                                + ConfigurationSingleton.MaxCarCount * Car.SizeOf;
}