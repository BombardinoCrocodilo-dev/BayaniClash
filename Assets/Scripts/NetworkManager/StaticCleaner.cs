using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StaticCleaner
{
    public static void ResetAll()
    {
        Debug.Log("[StaticCleaner] Resetting all static instances...");

        // Destroy persistent singletons that use DontDestroyOnLoad
        //GameManager.DestroyInstance();

        // Set static references to null (for non-persistent ones)
        //CharacterSelectManager.Instance = null;
        //SceneLoader.Instance = null;
        //ButtonManager.Instance = null;
        // Add other singletons here as you need
        foreach (var netObj in Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            // Example: destroy only if the object is in the current scene
            if (netObj.gameObject.scene == SceneManager.GetActiveScene())
            {
                UnityEngine.Object.Destroy(netObj.gameObject);
            }
        }
        Debug.Log("[StaticCleaner] Cleanup complete.");
    }
}
