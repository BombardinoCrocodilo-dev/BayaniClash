using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    public bool isPVEMode;
    public bool isPracticeMode;

    public int selectedMapIndex;
    public float timer;
    public int hostCharacterIndex;
    public int clientCharacterIndex;

    public void ResetGameData()
    {
        selectedMapIndex = -1;
        timer = 0;
        hostCharacterIndex = -1;
        clientCharacterIndex = -1;
    }
}
