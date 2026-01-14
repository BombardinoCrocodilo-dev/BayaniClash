using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class BattleVoiceManager : NetworkBehaviour
{
    [SerializeField]public CinemachineCamera mainCamera;

    private string[] characterNames = new string[]
    {
        "JoseRizal",
        "LapuLapu",
        "GabrielaSilang",
        "FranciscoDagohoy",
        "AntonioLuna",
        "MelchoraAquino",
        "ApolinarioMabini",
        "GregorioDelPilar",
        "ManuelQuezon",
        "MiguelMalvar"
    };

    public GameSpawner spawner;
    public CinemachineTargetGroup targetGroup;

    private Transform hostCharacter;
    private Transform clientCharacter;

    public void SetupFromSpawner()
    {
        if (spawner == null || spawner.SpawnedCharactersCount < 2)
        {
            return;
        }

        hostCharacter = spawner.GetSpawnedCharacter(0);
        clientCharacter = spawner.GetSpawnedCharacter(1);
    }

    public void StartIntroSequence()
    {
        if (IsServer)
            StartCoroutine(PlayIntroSequenceNetworked());
    }

    public void StartDeathSequence(bool hostLost)
    {
        if (IsServer)
            StartCoroutine(PlayDeathAndKillNetworked(hostLost));
    }


    private IEnumerator PlayIntroSequenceNetworked()
    {
        SetupFromSpawner();

        if (hostCharacter == null || clientCharacter == null) yield break;

        // Host intro
        ZoomCameraToClientRpc(hostCharacter.GetComponent<NetworkObject>().NetworkObjectId);
        PlayVoiceLineTargetedClientRpc(spawner.GetHostIndex(), "intro", default);
        yield return new WaitForSeconds(5f);

        // Client intro
        ZoomCameraToClientRpc(clientCharacter.GetComponent<NetworkObject>().NetworkObjectId);
        PlayVoiceLineTargetedClientRpc(spawner.GetClientIndex(), "intro", default);
        yield return new WaitForSeconds(5f);

        // Zoom out
        ResetCameraZoomClientRpc();
    }

    private IEnumerator PlayDeathAndKillNetworked(bool hostLost)
    {
        SetupFromSpawner();

        if (hostCharacter == null || clientCharacter == null) yield break;

        Transform deadChar = hostLost ? hostCharacter : clientCharacter;
        Transform winnerChar = hostLost ? clientCharacter : hostCharacter;

        int deadIndex = hostLost ? spawner.GetHostIndex() : spawner.GetClientIndex();
        int winnerIndex = hostLost ? spawner.GetClientIndex() : spawner.GetHostIndex();

        ulong deadClientId = deadChar.GetComponent<NetworkObject>().OwnerClientId;
        ulong winnerClientId = winnerChar.GetComponent<NetworkObject>().OwnerClientId;

        // Zoom camera for everyone
        ZoomCameraToClientRpc(deadChar.GetComponent<NetworkObject>().NetworkObjectId);

        // Play DEATH VOICE — ONLY FOR THE OWNER OF THE DEAD CHARACTER
        PlayVoiceLineTargetedClientRpc(
            deadIndex,
            "death",
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { deadClientId }
                }
            }
        );

        yield return new WaitForSeconds(1.5f);

        ZoomCameraToClientRpc(winnerChar.GetComponent<NetworkObject>().NetworkObjectId);

        // Play KILL VOICE — ONLY FOR THE OWNER OF THE WINNER CHARACTER
        PlayVoiceLineTargetedClientRpc(
            winnerIndex,
            "kill",
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { winnerClientId }
                }
            }
        );

        yield return new WaitForSeconds(1.5f);

        ResetCameraZoomClientRpc();
    }



    [ClientRpc]
    private void ZoomCameraToClientRpc(ulong targetNetworkId)
    {
        if (mainCamera == null) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkId, out NetworkObject targetObj))
        {
            mainCamera.Follow = targetObj.transform;
            mainCamera.LookAt = targetObj.transform;
        }
    }

    [ClientRpc]
    private void ResetCameraZoomClientRpc()
    {
        if (mainCamera != null)
        {
            mainCamera.Follow = targetGroup.transform;
            mainCamera.LookAt = targetGroup.transform;
        }
    }

    [ClientRpc]
    private void PlayVoiceLineTargetedClientRpc(int index, string action, ClientRpcParams rpcParams = default)
    {
        if (index < 0 || index >= characterNames.Length) return;

        string characterName = characterNames[index];
        var voiceLineManager = FindAnyObjectByType<VoiceLineManager>();

        if (voiceLineManager != null)
        {
            voiceLineManager.PlayVoiceLine(characterName, action);
        }
    }

}
