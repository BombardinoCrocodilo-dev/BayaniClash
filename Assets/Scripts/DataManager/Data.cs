using UnityEngine;
public static class Data
{
    private static GameData _gameData;

    public static GameData GameData
    {
        get
        {
            if (_gameData == null)
                _gameData = Resources.Load<GameData>("GameData");
            return _gameData;
        }
    }
}
