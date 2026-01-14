using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Timer : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    public float startTime;

    private NetworkVariable<float> timerCount = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool timerEnded = false; // server-only guard to prevent multiple triggers

    private void Awake()
    {
        startTime = gameData.timer;
        if ((StoryModeManager.Instance != null && StoryModeManager.Instance.isStoryMode) || gameData.isPracticeMode)
        {
            if (timerText != null) timerText.text = "∞";
            return;
        }
    }

    public override void OnNetworkSpawn()
    {
        // Clients should react to network variable changes immediately
        timerCount.OnValueChanged += OnTimerCountChanged;

        if (IsServer)
        {
            timerCount.Value = startTime;
            timerEnded = false;
            Debug.Log("[Timer] OnNetworkSpawn (Server) - timer set to " + timerCount.Value);
        }
        else
        {
            // ensure UI shows the initial value for non-server clients
            UpdateTimerUI(timerCount.Value);
        }
    }

    public override void OnDestroy()
    {
        timerCount.OnValueChanged -= OnTimerCountChanged;
    }

    private void OnTimerCountChanged(float oldVal, float newVal)
    {
        // Update UI on clients when network value changes
        UpdateTimerUI(newVal);
        // optional debug
        Debug.Log($"[Timer] NetworkVar changed: {oldVal} -> {newVal} (IsServer={IsServer})");
    }

    private void Update()
    {
        if (!IsSpawned) return;

        if ((StoryModeManager.Instance != null && StoryModeManager.Instance.isStoryMode) || gameData.isPracticeMode)
        {
            if (timerText != null) timerText.text = "∞";
            return; // skip countdown and UI update
        }

        // Only server should advance the countdown
        if (IsServer && timerCount.Value > 0f && !timerEnded)
        {
            // Note: use unscaled delta if you may be changing timeScale on clients, but usually Time.deltaTime is ok
            timerCount.Value -= Time.deltaTime;

            if (timerCount.Value <= 0f)
            {
                timerCount.Value = 0f;
                timerEnded = true;
                Debug.Log("[Timer] Timer reached 0 on server. Notifying players.");
                NotifyPlayersTimerEnded();
            }
        }

        // client UI updates come via OnValueChanged; we optionally call UpdateTimerUI here for local smoothing
        // UpdateTimerUI(timerCount.Value);
    }

    private void NotifyPlayersTimerEnded()
    {
        // Server should call a ServerRpc on each Player to handle end of time.
        foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (obj.TryGetComponent<Player>(out Player player))
            {
                // Call player's server RPC; this ensures the Player's server-side logic executes
                player.TimerEndedServerRpc();
            }
        }
    }

    private void UpdateTimerUI(float value)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(value / 60f);
        int seconds = Mathf.FloorToInt(value % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Public server RPC - clients or server can call this to request a reset (authoritative)
    [ServerRpc(RequireOwnership = false)]
    public void RequestResetTimerServerRpc()
    {
        ResetTimerInternal();
    }

    // Keep this for possible server direct calls
    public void ResetTimer()
    {
        if (!IsServer) return;
        ResetTimerInternal();
    }

    private void ResetTimerInternal()
    {
        timerCount.Value = startTime;
        timerEnded = false;
        Debug.Log("[Timer] ResetTimerInternal called on server -> timer set to " + timerCount.Value);
        // OnValueChanged will propagate to clients and update UI
    }
}
