using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBar : MonoBehaviour
{
    private GameData gameData => Data.GameData;
    public void goToMainMenu()
    {
        if(gameData.isPVEMode || gameData.isPracticeMode || StoryModeManager.Instance.isStoryMode)
        {   
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            Debug.Log("Home button pressed!");
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            if(SessionManager.Instance.activeSession  != null)
            {
                SessionManager.Instance.HostLeaveSession();
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
        AudioManager.Instance.PlayClick();
    }

}
