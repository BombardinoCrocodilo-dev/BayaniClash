using Unity.Netcode;
using UnityEngine;

[RequireComponent (typeof(NetworkObject))]
public class OnPlayerConnected : NetworkBehaviour
{
    public GameObject clientWaitPanel;
    public CanvasGroup canvasGroup;
    public override void OnNetworkSpawn()
    {
        NetworkManager.OnClientConnectedCallback += OnCLientConnected;
    }
    public void OnCLientConnected(ulong clientId)
    {
        
    }
}
