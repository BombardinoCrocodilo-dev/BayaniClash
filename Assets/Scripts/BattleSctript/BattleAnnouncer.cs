using Unity.Netcode;
using UnityEngine;

public class BattleAnnouncer : NetworkBehaviour
{
    [Header("Announcer Clips")]
    [SerializeField] private AudioClip round1Clip;
    [SerializeField] private AudioClip round2Clip;
    [SerializeField] private AudioClip round3Clip;
    [SerializeField] private AudioClip fightClip;
    [SerializeField] private AudioClip koClip;
    [SerializeField] private AudioClip youWinClip;
    [SerializeField] private AudioClip youLoseClip;
    [SerializeField] private AudioClip[] StageClip; 


    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Server tells all clients to play voice
    [ServerRpc(RequireOwnership = false)]
    public void PlayVoiceServerRpc(string clipName)
    {
        PlayVoiceClientRpc(clipName);
    }

    [ClientRpc]
    private void PlayVoiceClientRpc(string clipName)
    {
        AudioClip clip = null;

        switch (clipName)
        {
            case "round1": clip = round1Clip; break;
            case "round2": clip = round2Clip; break;
            case "round3": clip = round3Clip; break;
            case "fight": clip = fightClip; break;
            case "ko": clip = koClip; break;
            case "win": clip = youWinClip; break;
            case "lose": clip = youLoseClip; break;
            case "Stage1": clip = StageClip[0]; break;
            case "Stage2": clip = StageClip[1]; break;
            case "Stage3": clip = StageClip[2]; break;
            case "Stage4": clip = StageClip[3]; break;
            case "Stage5": clip = StageClip[4]; break;
            case "Stage6": clip = StageClip[5]; break;
            case "Stage7": clip = StageClip[6]; break;
            case "Stage8": clip = StageClip[7]; break;
            case "Stage9": clip = StageClip[8]; break;
            case "FinalStage": clip = StageClip[9]; break;
        }

        if (clip != null)
            AudioManager.Instance.PlayVoiceLine(clip);
    }
}
