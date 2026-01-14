using Unity.Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkCameraTarget : NetworkBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;

    private void Start()
    {
        // Start a coroutine to refresh targets continuously
        StartCoroutine(RefreshTargetsRoutine());
    }

    private IEnumerator RefreshTargetsRoutine()
    {
        while (true)
        {
            // Find all active players in the scene
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            // Only update if we have at least 2 players
            if (players.Length >= 2)
            {
                UpdateTargetGroup(players);
            }

            // Check every 0.5 seconds
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateTargetGroup(GameObject[] players)
    {
        // Clear old targets
        targetGroup.Targets.Clear();

        // Add new player targets
        foreach (GameObject player in players)
        {
            targetGroup.AddMember(player.transform, 1f, 2f); // weight, radius
        }

        Debug.Log("Cinemachine Target Group updated!");
    }

    /// <summary>
    /// Call this manually if you want to force-refresh immediately after spawning players
    /// </summary>
    public void RefreshTargetsNow()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length >= 2)
        {
            UpdateTargetGroup(players);
        }
    }
}
