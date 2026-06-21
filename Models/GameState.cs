namespace Freeway.Models;

[Serializable]
internal class GameState
{
    public Player[] Players { get; }
    public Car[] Cars { get; }

    public GameState(Player[] players, Car[] cars)
    {
        Players = players.ToArray();
        Cars = cars.ToArray();
    }
}