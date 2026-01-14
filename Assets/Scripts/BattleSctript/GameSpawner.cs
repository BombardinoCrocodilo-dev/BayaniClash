using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameSpawner : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    [Header("All Character Prefabs (Same Order as in CharacterSelectManager)")]
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private GameObject[] AIcharacterPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform HostSpawnPoint;
    [SerializeField] private Transform ClientSpawnPoint;

    [Header("P1 UI")]
    [SerializeField] private TextMeshProUGUI P1characterName;
    [SerializeField] private Image P1icon;

    [Header("P2 UI")]
    [SerializeField] private TextMeshProUGUI P2characterName;
    [SerializeField] private Image P2icon;

    [SerializeField] private string[] characterName;
    [SerializeField] private Sprite[] characterIcon;

    private int hostSelectedIndex = 0;
    private int clientSelectedIndex = 0;

    private List<NetworkObject> spawnedCharacters = new List<NetworkObject>();
    public int SpawnedCharactersCount => spawnedCharacters.Count;

   
    public override void OnNetworkSpawn()
    {
        CleanupOldCharacters();
        SpawnCurrentStage();
    }

    public void SpawnCurrentStage()
    {
        if (!IsServer) return;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        foreach (var netObj in spawnedCharacters)
        {
            if (netObj != null && netObj.IsSpawned)
                netObj.Despawn(true);
        }
        spawnedCharacters.Clear();

        yield return new WaitForSeconds(0.1f);
        hostSelectedIndex = gameData.hostCharacterIndex;

        clientSelectedIndex = gameData.clientCharacterIndex;

        if (gameData.isPracticeMode && !StoryModeManager.Instance.isStoryMode)
        {
            Debug.Log("Practice mode active — spawning only player character.");
            SpawnCharacterForClient(NetworkManager.ServerClientId, hostSelectedIndex, HostSpawnPoint);

            // Only show P1 info
            UpdateUIClientRpc(hostSelectedIndex, -1);
            yield break;
        }
        if (StoryModeManager.Instance != null && StoryModeManager.Instance.isStoryMode)
        {
            GameObject enemyPrefab = StoryModeManager.Instance.currentEnemyPrefab;
            clientSelectedIndex = GetCharacterIndexByPrefab(enemyPrefab, AIcharacterPrefabs);
        }

        SpawnPlayers();
        UpdateUIClientRpc(hostSelectedIndex, clientSelectedIndex);
    }

    private void SpawnPlayers()
    {
        SpawnCharacterForClient(NetworkManager.ServerClientId, hostSelectedIndex, HostSpawnPoint);

        if (gameData.isPVEMode || StoryModeManager.Instance.isStoryMode)
        {
            GameObject prefabToSpawn = AIcharacterPrefabs[clientSelectedIndex];
            GameObject aiInstance = Instantiate(prefabToSpawn, ClientSpawnPoint.position, ClientSpawnPoint.rotation);
            var netObj = aiInstance.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
                spawnedCharacters.Add(netObj);
            }
        }
        else 
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != NetworkManager.ServerClientId)
                    SpawnCharacterForClient(clientId, clientSelectedIndex, ClientSpawnPoint);
            }
        }
    }

    private int GetCharacterIndexByPrefab(GameObject prefab, GameObject[] array)
    {
        for (int i = 0; i < array.Length; i++)
            if (array[i] == prefab) return i;
        return 0;
    }

    private void SpawnCharacterForClient(ulong clientId, int characterIndex, Transform spawnPoint)
    {
        GameObject prefabToSpawn = characterPrefabs[characterIndex];
        GameObject instance = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        var netObj = instance.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.SpawnAsPlayerObject(clientId);
            spawnedCharacters.Add(netObj);
        }
    }
    public void CleanupOldCharacters()
    {
        foreach (var netObj in spawnedCharacters)
        {
            if (netObj != null)
            {
                if (netObj.IsSpawned)
                    netObj.Despawn(true); // despawn on the network

                Destroy(netObj.gameObject); // destroy the leftover GameObject
            }
        }
        spawnedCharacters.Clear();

        hostSelectedIndex = gameData.hostCharacterIndex;

        clientSelectedIndex = gameData.clientCharacterIndex;
    }

    [ClientRpc]
    private void UpdateUIClientRpc(int hostIndex, int clientIndex)
    {
        P1characterName.SetText(characterName[hostIndex]);
        P1icon.sprite = characterIcon[hostIndex];
        P2characterName.SetText(characterName[clientIndex]);
        P2icon.sprite = characterIcon[clientIndex];
    }

    [ServerRpc(RequireOwnership = false)]
    public void RespawnPlayersServerRpc()
    {
        SpawnCurrentStage();
    }
    public Transform GetSpawnedCharacter(int index)
    {
        if (index >= 0 && index < spawnedCharacters.Count)
            return spawnedCharacters[index].transform;
        return null;
    }

    public string GetHostName()
    {
        if (hostSelectedIndex >= 0 && hostSelectedIndex < characterName.Length)
            return characterName[hostSelectedIndex];
        return "Unknown";
    }

    public string GetClientName()
    {
        if (clientSelectedIndex >= 0 && clientSelectedIndex < characterName.Length)
            return characterName[clientSelectedIndex];
        return "Unknown";
    }

    public int GetHostIndex() => hostSelectedIndex;
    public int GetClientIndex() => clientSelectedIndex;
}
