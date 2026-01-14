using Unity.Netcode;
using UnityEngine;

public class NetworkRelay : NetworkBehaviour
{
    public static NetworkRelay Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Spawn the NetworkObject if not already spawned (host only)
        if (IsServer && !NetworkObject.IsSpawned)
        {
            NetworkObject.Spawn();
            Debug.Log("[NetworkRelay] NetworkObject spawned for RPCs");
        }
    }

    // RPC called by host to make all clients leave
    [ClientRpc]
    public void ForceClientsLeaveClientRpc()
    {
        if (IsServer) return; // skip host

        Debug.Log("[NetworkRelay] Client received leave RPC");
        _ = SessionManager.Instance.ForceClientLeave(); // fire-and-forget
    }
}
