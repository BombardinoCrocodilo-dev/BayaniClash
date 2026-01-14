using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class VSLoader : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image hostImage;
    [SerializeField] private Image clientImage;

    [Header("Character Images")]
    [SerializeField] private Sprite[] characterSprites;

    private int hostIndex = -1;
    private int clientIndex = -1;

    private void Start()
    {
        if (IsServer)
        {
            hostIndex = PlayerPrefs.GetInt("HostSelectedCharacter", 0);
            clientIndex = PlayerPrefs.GetInt("ClientSelectedCharacter", 0);

            // Immediately update host UI
            UpdateImages(hostIndex, clientIndex);

            // Sync to all clients
            UpdateClientImagesClientRpc(hostIndex, clientIndex);

            StartCoroutine(LoadBattleSceneAfterDelay());
        }
    }

    [ClientRpc]
    private void UpdateClientImagesClientRpc(int hostIdx, int clientIdx)
    {
        hostIndex = hostIdx;
        clientIndex = clientIdx;
        UpdateImages(hostIndex, clientIndex);
    }

    private void UpdateImages(int hostIdx, int clientIdx)
    {
        if (characterSprites == null || characterSprites.Length == 0)
        {
            Debug.LogError("Character sprites not assigned!");
            return;
        }

        if (hostIdx >= 0 && hostIdx < characterSprites.Length)
            hostImage.sprite = characterSprites[hostIdx];
        if (clientIdx >= 0 && clientIdx < characterSprites.Length)
            clientImage.sprite = characterSprites[clientIdx];
    }

    private IEnumerator LoadBattleSceneAfterDelay()
    {
        yield return new WaitForSeconds(5f); // show loading screen

        if (IsServer)
        {
            // Use Netcode scene manager to load for all clients
            NetworkManager.SceneManager.LoadScene("BattleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
