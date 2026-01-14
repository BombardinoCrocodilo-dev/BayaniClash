using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SelectionTimer : NetworkBehaviour
{
    public TextMeshProUGUI timerText;
    private NetworkVariable<float> timerValue = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );
    public float setTimer = 50f;
    void Start()
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
        {
            timerText.text = setTimer.ToString("0");
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            timerValue.Value = setTimer;
        }
        UpdateTimeUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
        {
            if (setTimer > 0)
            {
                setTimer -= Time.deltaTime;
                timerText.text = setTimer.ToString("0");
            }
            return;
        }
        if (!IsSpawned) return;

        if (IsServer && timerValue.Value > 0)
        {
            timerValue.Value -= Time.deltaTime;
        }
        UpdateTimeUI();

    }
    private void UpdateTimeUI()
    {
        timerText.text = timerValue.Value.ToString("0");
    }
}
