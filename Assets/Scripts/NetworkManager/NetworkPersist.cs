using UnityEngine;
using Unity.Netcode;

public class NetworkManagerPersist : MonoBehaviour
{
    [System.Obsolete]
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (FindObjectsOfType<NetworkManager>().Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
