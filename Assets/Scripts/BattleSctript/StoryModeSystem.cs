using System.IO;
using UnityEngine;

public class StoryModeSystem : MonoBehaviour
{
    public static StoryModeSystem Instance;

    [Header("Story Settings")]
    public GameObject[] stageEnemies;
    public StoryModeData data = new StoryModeData();


    private string savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Persistent path
        savePath = Path.Combine(Application.persistentDataPath, "storymode.json");

        // Load or create new save
        LoadProgress();
    }

    public void OnBattleEnd(bool playerWon)
    {
        string hero = StoryModeManager.Instance != null ? StoryModeManager.Instance.chosenHero : "";
        GameObject enemyPrefab = GetEnemyForStage(data.currentStage);
        string enemyName = enemyPrefab != null ? enemyPrefab.name : "Unknown";

        var roundhanlder = FindAnyObjectByType<RoundHandler>();

        if (playerWon)
        {
            var defeated = new System.Collections.Generic.List<string>(data.defeatedEnemies);
            if (!defeated.Contains(enemyName))
                defeated.Add(enemyName);
            data.defeatedEnemies = defeated.ToArray();
            data.currentStage++;
        }
        else
        {
            var lost = new System.Collections.Generic.List<string>(data.lostHeroes);
            var win = new System.Collections.Generic.List<string>(data.wonHeroes);
            if (!lost.Contains(hero)&&!win.Contains(hero))
                lost.Add(hero);

            data.lostHeroes = lost.ToArray();
        }

        SaveProgress();
    }

    public GameObject GetEnemyForStage(int stage)
    {
        int idx = stage - 1;
        if (idx >= 0 && idx < stageEnemies.Length)
            return stageEnemies[idx];
        return null;
    }


    public bool IsHeroWon(string heroName)
    {
        return System.Array.Exists(data.wonHeroes, h => h == heroName);
    }

    public bool IsHeroLost(string heroName)
    {
        return System.Array.Exists(data.lostHeroes, h => h == heroName);
    }


    public void SaveProgress()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);

            // Ensure folder exists
            string folder = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(savePath, json);
            Debug.Log("[StoryMode] Saved to " + savePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[StoryMode] Save failed: " + e.Message);
        }
    }

    public void LoadProgress()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                data = JsonUtility.FromJson<StoryModeData>(json) ?? new StoryModeData();
                Debug.Log("[StoryMode] Loaded progress - Stage: " + data.currentStage);
                Debug.Log("[StoryMode] Locked Heroes: " + string.Join(", ", data.lostHeroes));
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[StoryMode] Load failed: " + e.Message);
            }
        }

        // No save found, create new
        data = new StoryModeData { currentStage = 1, defeatedEnemies = new string[0], lostHeroes = new string[0] };
        SaveProgress();
        Debug.Log("[StoryMode] New save created.");
    }

    public void ResetProgress()
    {
        data = new StoryModeData { currentStage = 1, defeatedEnemies = new string[0], lostHeroes = new string[0] };
        SaveProgress();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveProgress();
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }
}
