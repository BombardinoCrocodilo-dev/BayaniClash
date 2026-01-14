using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class LoadingCutscene : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private double skipSeconds = 1.0; // seconds to skip per touch
    private bool touchProcessed = false;

    private string[] characterNames = new string[]
    {
        "Jose Rizal",
        "LapuLapu",
        "Gabriela Silang",
        "Francisco Dagohoy",
        "Antonio Luna",
        "Melchora Aquino",
        "Apolinario Mabini",
        "Gregorio Del Pilar",
        "Manuel L Quezon",
        "Miguel Malvar"
    };

    void Start()
    {
        PlayCutsceneFromPrefs();
    }
    void Update()
    {
        // Detect touch (or mouse click in editor)
        if ((Input.touchCount > 0 || Input.GetMouseButton(0)) && !touchProcessed)
        {
            SkipForward();
            touchProcessed = true; // prevent multiple skips per frame
        }

        // Reset touchProcessed when no touch
        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            touchProcessed = false;
        }
    }

    private void SkipForward()
    {
        if (videoPlayer.isPlaying)
        {
            double newTime = videoPlayer.time + skipSeconds;
            if (newTime >= videoPlayer.length)
            {
                // If it exceeds video length, end video
                OnVideoFinished(videoPlayer);
            }
            else
            {
                videoPlayer.time = newTime;
            }

            Debug.Log("Skipped forward " + skipSeconds + " seconds. Current time: " + videoPlayer.time);
        }
    }

    private void PlayCutsceneFromPrefs()
    {
        // Load JSON from Resources
        TextAsset jsonFile = Resources.Load<TextAsset>("character_cutscenes");
        if (jsonFile == null)
        {
            Debug.LogError(" JSON not found!");
            return;
        }

        CutsceneData data = JsonUtility.FromJson<CutsceneData>(jsonFile.text);
        if (data == null || data.cutscenes == null)
        {
            Debug.LogError("Failed to parse JSON.");
            return;
        }

        int selectedIndex = PlayerPrefs.GetInt("HostSelectedCharacter", 0);
        string selectedCharacter = characterNames[selectedIndex];

        // Find match
        string videoName = null;
        foreach (var item in data.cutscenes)
        {
            if (item.name == selectedCharacter)
            {
                videoName = item.file;
                break;
            }
        }

        if (string.IsNullOrEmpty(videoName))
        {
            Debug.LogError("No video found for " + selectedCharacter);
            return;
        }

        string videoPath = Path.Combine(Application.streamingAssetsPath, "Videos", videoName);
        Debug.Log(" Playing " + selectedCharacter + " " + videoPath);

        videoPlayer.url = videoPath;
        videoPlayer.Play();

        // load next scene after video ends
        videoPlayer.loopPointReached += OnVideoFinished;
    }
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log(" Cutscene finished — loading BattleScene...");
        NetworkManager.Singleton.SceneManager.LoadScene("BattleScene",LoadSceneMode.Single);
    }
}
