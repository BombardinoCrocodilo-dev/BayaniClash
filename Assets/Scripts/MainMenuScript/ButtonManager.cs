using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ButtonManager : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    [Header("UI Panels")]
    [SerializeField]private GameObject pvpPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject serverDcPanel;
    [SerializeField] private GameObject createSessionWidget;
    [SerializeField] private GameObject joinSessionWidget;
    [SerializeField] private GameObject sessionPanel;
    [SerializeField] private GameObject showCodeSessionWidget;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject creatButton;

    private void Start()
    {
        CleanData();
    }
    private void CleanData()
    {
        gameData.ResetGameData();
        StoryModeManager.Instance.isStoryMode = false;
        gameData.isPracticeMode = false;
        gameData.isPVEMode = false;
        pvpPanel.SetActive(false);
        settingsPanel.SetActive(false);

        StoryModeSystem.Instance.data.currentStage = 1;
        StoryModeSystem.Instance.data.defeatedEnemies = new string[0];

        var spawner = FindFirstObjectByType<GameSpawner>();
        if (spawner != null)
            spawner.CleanupOldCharacters();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if(SessionManager.Instance.activeSession != null)
        {
            SessionManager.Instance.activeSession.LeaveAsync();
        }
    }
    public void OnStartGame(string targetScene)
    {
        SceneLoadingManager.TargetSceneName = targetScene;

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.SceneManager.LoadScene("LoadingScene",LoadSceneMode.Single);
        }
    }
    public void OnStart()
    {
        AudioManager.Instance.PlayClick();
        OnStartGame("MapSelection");
    }
    public void StoryMode()
    {
        AudioManager.Instance.PlayClick();
        if (StoryModeManager.Instance == null) return;

        StoryModeManager.Instance.ActivateStoryMode();

        if (!NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.StartHost();

        OnStartGame("MapSelection");
    }

    public void PvE()
    {
        gameData.isPVEMode = true;
        AudioManager.Instance.PlayClick();
        NetworkManager.Singleton.StartHost();
        OnStartGame("MapSelection");
    }
    public void Practice()
    {
        gameData.isPracticeMode = true;
        AudioManager.Instance.PlayClick();
        NetworkManager.Singleton.StartHost();
        OnStartGame("MapSelection");
    }
    public void Settings()
    {
        AudioManager.Instance.ConnectSliders();
    }
    public void CharacterDescScene()
    {
        AudioManager.Instance.PlayClick();
        SceneManager.LoadScene("CharacterDescription");
    }
    public void ExitGame()
    {
        AudioManager.Instance.PlayClick();
        Application.Quit();
    }

}
