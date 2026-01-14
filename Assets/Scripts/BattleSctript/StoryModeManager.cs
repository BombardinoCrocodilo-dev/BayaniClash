using System.IO;
using UnityEngine;

public class StoryModeManager : MonoBehaviour
{
    public static StoryModeManager Instance;
    
    public bool isStoryMode = false;
    public string chosenHero;
    public GameObject currentEnemyPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ActivateStoryMode()
    {
        isStoryMode = true;

        if (StoryModeSystem.Instance != null)
        {
            currentEnemyPrefab = StoryModeSystem.Instance.GetEnemyForStage(1); // Stage 1
        }
    }

    public void SetChosenHero(string hero)
    {
        chosenHero = hero;
        if (isStoryMode && StoryModeSystem.Instance != null)
        {
            currentEnemyPrefab = StoryModeSystem.Instance.GetEnemyForStage(
                StoryModeSystem.Instance.data.currentStage
            );
        }
    }

    public void EndBattle(bool playerWon)
    {
        if (isStoryMode && StoryModeSystem.Instance != null)
        {
            StoryModeSystem.Instance.OnBattleEnd(playerWon);

            currentEnemyPrefab = StoryModeSystem.Instance.GetEnemyForStage(
                StoryModeSystem.Instance.data.currentStage
            );
        }
    }
}
