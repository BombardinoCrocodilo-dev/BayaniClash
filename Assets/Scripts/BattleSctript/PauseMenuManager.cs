using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;
using UnityEngine.UI;
using Unity.VisualScripting;

public class PauseMenuManager : NetworkBehaviour
{
    private GameData gameData => Data.GameData;
    private ISession activeSession;

    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    private void Start()
    {
        // Make sure all panels start hidden
        if (pausePanel != null) pausePanel.SetActive(false);

    }
    public void SetActiveSession(ISession session)
    {
        activeSession = session;
    }
    public ISession GetActiveSession()
    {
        return activeSession;
    }

    // Called when pause button is clicked (host/server only)
    public void OnPauseButtonPressed()
    {
        if (!IsServer) return;

        // Toggle pause
        isPaused = !isPaused;

        // Freeze host
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        // Notify all clients
        TogglePauseClientRpc(isPaused);

    }

    [ClientRpc]
    private void TogglePauseClientRpc(bool paused)
    {
        if (IsServer) return; // host already handled

        isPaused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void OnResumeButtonPressed()
    {
        if (!IsServer) return;

        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        TogglePauseClientRpc(false); // Resume clients too
    }

    // Called when "Home" button is pressed
    public  void GoToMainMenu()
    {
        if (!IsServer) return;
        AudioManager.Instance.PlayClick();
        Time.timeScale = 1f;
        if (gameData.isPVEMode)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            OnStartGame("MainMenu");
        }
        TimeScaleResumeClientRpc();
        SessionManager.Instance.HostLeaveSession();
    }
    [ClientRpc]
    private void TimeScaleResumeClientRpc()
    {
        if (IsServer) return;
        Time.timeScale = 1f;
    }
    public void PlayAgain()
    {
        if (!IsServer) return;
        AudioManager.Instance.PlayClick();
        gameData.ResetGameData();

        var spawner = FindFirstObjectByType<GameSpawner>();
        if (spawner != null)
            spawner.CleanupOldCharacters();

        PlayAgainClientRpc();
    }
    [ClientRpc]
    private void PlayAgainClientRpc()
    {
        OnStartGame("MapSelection");

    }
    public void OnStartGame(string targetScene)
    {
        // Assign the static target scene in the LoadingManager
        SceneLoadingManager.TargetSceneName = targetScene;

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.SceneManager.LoadScene("LoadingScene",LoadSceneMode.Single);
        }
    }
}
