using UnityEngine;
using Unity.Netcode;

public class HostSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return; // Only host spawns
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        GameObject player = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
    }
}
