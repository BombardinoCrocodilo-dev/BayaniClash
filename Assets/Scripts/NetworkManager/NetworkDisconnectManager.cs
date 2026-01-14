using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


[RequireComponent(typeof(NetworkObject))]
public class NetworkDisconnectManager : NetworkBehaviour
{
    private NetworkVariable<bool> syncDcButton = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
    );

    public static bool dcButtonClicked;

    public Button hostButton, clientButton;

    void Start()
    {
        syncDcButton.OnValueChanged += (oldVal, newVal) =>
        {
            if (!IsServer)
            {
                dcButtonClicked = newVal;
            }
        };
        if (IsServer)
        {
            hostButton.onClick.AddListener(OnHostClick);
        }
    }
    public void OnDisconnectButton()
    {
        if (NetworkManager.Singleton == null) return;

        if (IsServer)
        {
            StartCoroutine(DisconnectAndReturnToMenu());
        }
        else
        {
            RequestShutdownServerRpc();
            StartCoroutine(DisconnectAndReturnToMenu());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestShutdownServerRpc()
    {
        StartCoroutine(DisconnectAndReturnToMenu());
    }
    private void OnHostClick()
    {
        InvokButtonClientRpc();
    }

    private IEnumerator DisconnectAndReturnToMenu()
    {
        dcButtonClicked = true;
        if (IsServer) syncDcButton.Value = dcButtonClicked;
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        yield return null;
        SceneManager.LoadScene("MainMenu");
    }
    [ClientRpc]
    private void InvokButtonClientRpc()
    {
        if (IsServer) return;
        clientButton.onClick.Invoke();
    }
}
