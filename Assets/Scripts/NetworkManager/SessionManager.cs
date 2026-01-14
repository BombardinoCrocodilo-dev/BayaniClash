using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : NetworkBehaviour
{
    public static SessionManager Instance { get; private set; }

    //[SerializeField] GameObject OnJoinErrorPanel;

    public ISession activeSession;

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
            return;
        }
    }
    public void SetActiveSession(ISession session)
    {
        activeSession = session;
        Debug.Log($"[SessionManager] Active session set: {activeSession.Id}");
    }

    public ISession GetActiveSession() => activeSession;
    public async void HostLeaveSession()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log("[SessionManager] Host: telling clients to leave session...");

        // Tell all clients to leave
        ClientGoHomeClientRpc();

        // Wait a short time to give clients a chance to process
        await Task.Delay(500);

        // Host cleanup
        await CleanUpSessionAsync();
    }


    // -------------------- CLIENT ACTION --------------------
    public async Task ForceClientLeave()
    {
        if (activeSession != null)
        {
            Debug.Log("[SessionManager] Client: leaving UMS session...");
            await activeSession.LeaveAsync(); // first leave
            Debug.Log("[SessionManager] Client: left UMS session");
        }

        // Shutdown NGO if needed
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // Now load the main menu
        SceneManager.LoadScene("MainMenu");
        Debug.Log("[SessionManager] Client: loaded MainMenu");
    }


    // -------------------- HOST & SHARED CLEANUP --------------------
    public async Task CleanUpSessionAsync()
    {
        Debug.Log("[SessionManager] Cleaning up session...");

        // Leave UMS session
        if (activeSession != null)
        {
            await activeSession.LeaveAsync();
            Debug.Log("[SessionManager] Host left UMS session");
        }

        // Shutdown NGO
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[SessionManager] NGO shutdown on host");
        }

        await Task.Delay(100);

        // Load main menu
        SceneManager.LoadScene("MainMenu");
        Debug.Log("[SessionManager] Host loaded MainMenu");
    }
    [ClientRpc]
    private void ClientGoHomeClientRpc()
    {
        if (IsServer) return;
        _ = ForceClientLeave();
    }
}
