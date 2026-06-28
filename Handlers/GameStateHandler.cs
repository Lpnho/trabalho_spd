using Freeway.Core;
using Freeway.Interfaces;
using Freeway.Models;
using Freeway.Models.Actions;
using Freeway.Models.Network;
using Freeway.Singleton;
using Gtk;

namespace Freeway.Handlers;

public class GameStateHandler : IStateLockManager, IGameStateUpdater
{
    private Player[] _players = new Player[ConfigurationSingleton.MaxPlayersCount];
    private object[] _playersLock = new object[ConfigurationSingleton.MaxPlayersCount];

    private Car[] _cars = new Car[ConfigurationSingleton.MaxCarCount];
    private object[] _carsLock = new object[ConfigurationSingleton.MaxCarCount];

    private GameStateMatrix _state = new GameStateMatrix(ConfigurationSingleton.NRows,
            ConfigurationSingleton.NCols);

    private object[] _locks;

    private const int StartSafeRow = 0;
    private const int StartSafeCol = 0;
    private object _priorityLock = new();
    private bool _blockPriority = false;

    private int _nRows => _state.Row;
    private int _nCols => _state.Col;

    private ReaderWriterLockSlim _diffReaderWriterLock = new();
    private int _difficultyLevel = 1;
    public int DifficultyLevel
    {
        get
        {
            _diffReaderWriterLock.EnterReadLock();
            int data = _difficultyLevel;
            _diffReaderWriterLock.ExitReadLock();
            return data;
        }
        set
        {
            _diffReaderWriterLock.EnterWriteLock();
            _difficultyLevel = Math.Clamp(value,
               ConfigurationSingleton.MinDificultyLevel,
           ConfigurationSingleton.MaxDificultyLevel);
            _diffReaderWriterLock.ExitWriteLock();
        }
    }
    public GameStateHandler()
    {
        _locks = new object[_nRows];

        for (int i = 0; i < _locks.Length; ++i)
        {
            _locks[i] = new object();
        }
        for (int i = 0; i < ConfigurationSingleton.MaxPlayersCount; ++i)
        {
            _players[i] = new Player();
            _playersLock[i] = new object();
        }
        for (int i = 0; i < ConfigurationSingleton.MaxCarCount; ++i)
        {
            _cars[i] = new Car();
            _carsLock[i] = new object();
        }

    }
   
    public int GetColByPlayerId(int playerId)
    {
        return (playerId * 2 + 1);
    }
    public int GetRowByCarId(int message)
    {
        return ((message % (ConfigurationSingleton.NRows-2)) + 1);
    }
    public int GetIdByCol(int col)
    {
        return (col - 1) / 2;
    }
    public void BlockPriority()
    {
        lock (_priorityLock)
        {
            if (_blockPriority) return;
            _blockPriority = true;
        }
        for (int i = 0; i < _locks.Length; ++i)
        {
            Monitor.Enter(_locks[i]);
        }
    }
    public void UnBlockPriority()
    {
        lock (_priorityLock)
        {
            if (!_blockPriority) return;
            _blockPriority = false;
        }

        for (int i = 0; i < _locks.Length; ++i)
        {
            Monitor.Exit(_locks[i]);
        }
    }
    public void UpdateCarMovement(byte elementId, MovementAction action)
    {
        if (action == MovementAction.Up || action == MovementAction.Down ||
            elementId >= ConfigurationSingleton.MaxCarCount) return;

        GameElement element = GameElement.None;
        lock (_carsLock[elementId])
        {
            int row = _cars[elementId].Row;
            int col = _cars[elementId].Column;
            lock (_locks[row])
            {
                int diff = action == MovementAction.Left ? -1 : 1;
                int index = col + diff;

                if (index < 0) return;
                index = index >= _nCols ? 0 : index;

                element = _state.Get(row, index);
                if (element.Type == GameElementType.Car) return;
                _state.Set(row, col, GameElement.None);
                _state.Set(row, index, new GameElement(GameElementType.Car, elementId));
                _cars[elementId].Row = row;
                _cars[elementId].Column = index;
            }
        }

        if (element.Type == GameElementType.Player)
        {
            byte playerId = element.ElementId;
            lock (_playersLock[playerId])
            {
                ref Player player = ref _players[playerId];
                _state.Set(StartSafeRow, player.Column,
                    new GameElement(GameElementType.Player, playerId));
                player.Row = StartSafeRow;
                player.Score = ((--player.Score) < 0) ? 0 : player.Score;
            }
        }
    }
    public void UpdatePlayerMovement(byte elementId, MovementAction action)
    {
        if (action == MovementAction.Left || action == MovementAction.Right ||
            elementId >= ConfigurationSingleton.MaxPlayersCount) return;

        Monitor.Enter(_playersLock[elementId]);
        ref Player player = ref _players[elementId];

        int row = player.Row;
        int col = player.Column;

        int diff = action == MovementAction.Down ? -1 : 1;
        int targetRow = row + diff;


        int firstRow = Math.Min(row, targetRow);
        int secondRow = Math.Max(row, targetRow);

        bool enteredFirst = false;
        bool enteredSecond = false;
        try
        {
            if (firstRow >= 0)
            {
                Monitor.Enter(_locks[firstRow]);
                enteredFirst = true;
            }
            if (secondRow < _nRows)
            {
                Monitor.Enter(_locks[secondRow]);
                enteredSecond = true;
            }
            if (targetRow < 0) return;


            _state.Set(row, col, GameElement.None);

            var newElement = new GameElement(GameElementType.Player, elementId);

            if (targetRow >= _nRows)
            {
                _state.Set(StartSafeRow, col, newElement);
                player.Row = StartSafeRow;
                ++player.Score;
                return;
            }

            if (_state.Get(targetRow, col) == GameElement.None)
            {
                _state.Set(targetRow, col, newElement);
                player.Row = targetRow;
                return;
            }

            _state.Set(StartSafeRow, col, newElement);
            player.Row = StartSafeRow;
            player.Score = ((--player.Score) < 0) ? 0 : player.Score;
        }
        finally
        {
            if (enteredSecond)
            {
                Monitor.Exit(_locks[secondRow]);
            }
            if (enteredFirst)
            {
                Monitor.Exit(_locks[firstRow]);
            }
            Monitor.Exit(_playersLock[elementId]);
        }
    }
    public GameState BlockPriorityGetState()
    {
        BlockPriority();

        //Console.Clear();
        //Console.WriteLine(_state.ToString());

        return new GameState(_players, _cars);
    }
    public byte ConnectPlayer()
    {
        for (int i = 0; i < ConfigurationSingleton.MaxPlayersCount; ++i)
        {
            lock (_playersLock[i])
            {
                if (_players[i].State == StateAction.Disconnected)
                {
                    ref Player player = ref _players[i];
                    player.State = StateAction.Connected;
                    player.Row = StartSafeRow;
                    player.Column = GetColByPlayerId(i);
                    player.Score = 0;
                    _state.Set(player.Row, player.Column, new GameElement(GameElementType.Player, (byte)i));
                    return (byte)i;
                }
            }
        }
        return (byte)ConfigurationSingleton.MaxPlayersCount;
    }
    public byte ConnectCar()
    {
        for (int i = 0; i < ConfigurationSingleton.MaxCarCount; ++i)
        {
            lock (_carsLock[i])
            {
                if (_cars[i].State == StateAction.Disconnected)
                {
                    ref Car car = ref _cars[i];
                    car.State = StateAction.Connected;
                    car.Row =  GetRowByCarId(i);
                    car.Column = StartSafeCol;
                    _state.Set(car.Row, car.Column,
                        new GameElement(GameElementType.Car, (byte)i));
                    return (byte)i;
                }
            }
        }
        return (byte)ConfigurationSingleton.MaxCarCount;
    }

    public void DisconnectPlayer(byte playerId)
    {
        if (playerId > ConfigurationSingleton.MaxPlayersCount) return;
        lock (_playersLock[playerId])
        {
            ref Player player = ref _players[playerId];
            if (player.Row != StartSafeRow)
            {
                lock (_locks[player.Row])
                {
                    _state.Set(player.Row, player.Column, GameElement.None);
                }
            }

            player.State = StateAction.Disconnected;
            player.Row = StartSafeRow;
            player.Column = GetColByPlayerId(playerId);
            player.Score = 0;
        }
    }
    public void DisconnectCar(byte carId)
    {
        if (carId > ConfigurationSingleton.MaxCarCount) return;
        lock (_carsLock[carId])
        {
            ref Car car = ref _cars[carId];
            int row = GetRowByCarId(carId);

            if (car.Column != StartSafeCol)
            {
                lock (_locks[row])
                {
                    _state.Set(car.Row, car.Column,
                        GameElement.None);
                }
            }
            car.Row = row;
            car.Column = StartSafeCol;
            car.State = StateAction.Disconnected;
        }
    }
}
