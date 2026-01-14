using UnityEngine;
using Unity.Netcode;

public class PracticeModeSpawner : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    [Header("Settings")]
    public bool isPracticeMode; // Just declare it here
    public GameObject characterPrefab;
    public Transform spawnPoint; // Assign your empty GameObject here

    private void Start()
    {
        // Assign the value from ButtonManager at runtime
        isPracticeMode = gameData.isPracticeMode;

        if (isPracticeMode)
        {
            SpawnPracticeCharacter();
        }
    }

    private void SpawnPracticeCharacter()
    {
        if (characterPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("CharacterPrefab or SpawnPoint is not assigned!");
            return;
        }

        // Instantiate prefab at the spawn point
        GameObject practiceChar = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);

        var netObj = practiceChar.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(); // no owner, server-controlled
        }
    }
}
