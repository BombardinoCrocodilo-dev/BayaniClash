using UnityEngine;
using Unity.Netcode;

public class ClientSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return; // Only host spawns

        // Make sure at least one client is connected
        if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        ulong clientId = NetworkManager.Singleton.ConnectedClientsList[1].ClientId; // First connected client
        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.GetComponent<NetworkObject>().SpawnWithOwnership(clientId); // Client owns enemy
    }
}
