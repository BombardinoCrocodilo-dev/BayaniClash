using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoundHandler : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    [Header("Round Settings")]
    [SerializeField] private int maxRounds = 2;
    [SerializeField] private float roundRestartDelay = 2f;
    [SerializeField] private float endPanelDelay = 2f;

    [Header("Round UI Images")]
    [SerializeField] private Image round1Image;
    [SerializeField] private Image round2Image;
    [SerializeField] private Image round3Image;
    [SerializeField] private Image KOImage;
    [SerializeField] private Image fightImage;
    [SerializeField] private Image endDisplayPanel;
    [SerializeField] private Image youWinImage;
    [SerializeField] private Image youLoseImage;
    [SerializeField] private GameObject P1win1;
    [SerializeField] private GameObject P1win2;
    [SerializeField] private GameObject P2win1;
    [SerializeField] private GameObject P2win2;
    [SerializeField] private GameObject movementButtons;

    [SerializeField] private Image storyFinishImage;

    [Header("Story Mode")]
    [SerializeField] private Image[] stageImages; 
    [SerializeField] private int maxStoryStages = 9;

    private BattleVoiceManager battleVoice;
    private BattleAnnouncer battleAnnouncer;
    public bool buttonLocked;

    public bool storyModeEnd = false;

    private NetworkVariable<bool> roundStarted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool IsRoundStarted => roundStarted.Value;

    private NetworkVariable<int> player1Wins = new NetworkVariable<int>();
    private NetworkVariable<int> player2Wins = new NetworkVariable<int>();

    private int currentRound = 1;
    private int currentStage = 1;
    private bool roundActive = false;
    private bool roundEnded = false;
    private bool stageDisplayed = false;

    private void Start()
    {
        battleVoice = FindFirstObjectByType<BattleVoiceManager>();
        battleAnnouncer = FindFirstObjectByType<BattleAnnouncer>();

        if (gameData.isPracticeMode)
            return;

        if (battleVoice != null && battleAnnouncer != null)
            StartCoroutine(WaitForSpawnAndPlayIntro());
    }

    private IEnumerator WaitForSpawnAndPlayIntro()
    {
        var spawner = FindFirstObjectByType<GameSpawner>();
        while (spawner == null || spawner.SpawnedCharactersCount < 2)
            yield return null;

        // Trigger the networked intro sequence
        if (battleVoice != null)
            battleVoice.StartIntroSequence();
        HideClientControllClientRpc();
        yield return new WaitForSecondsRealtime(11f);
        ShowClientControllClientRpc();
        if (IsServer)
        {
            ShowStageIfStoryMode();
            StartCoroutine(StartRoundRoutine());
        }
    }

    private void ShowStageIfStoryMode()
    {
        if (StoryModeManager.Instance != null && StoryModeManager.Instance.isStoryMode && stageImages.Length >= currentStage && !stageDisplayed)
        {
            stageDisplayed = true;
            ShowStoryStageClientRpc(currentStage - 1);
        }
    }

    private IEnumerator StartRoundRoutine()
    {
        roundEnded = false;
        roundActive = false;

        if (StoryModeManager.Instance.isStoryMode && stageDisplayed)
        {
            yield return new WaitForSecondsRealtime(2f); 
            HideAllRoundImages();
        }
        ShowRoundClientRpc(currentRound);
    }

    [ClientRpc]
    private void ShowStoryStageClientRpc(int stageIndex)
    {
        HideAllRoundImages();

        if (stageImages.Length > stageIndex && stageImages[stageIndex] != null)
            stageImages[stageIndex].gameObject.SetActive(true);
        switch (stageIndex)
        {
            case 0: battleAnnouncer.PlayVoiceServerRpc("Stage1"); break;
            case 1: battleAnnouncer.PlayVoiceServerRpc("Stage2"); break;
            case 2: battleAnnouncer.PlayVoiceServerRpc("Stage3"); break;
            case 3: battleAnnouncer.PlayVoiceServerRpc("Stage4"); break;
            case 4: battleAnnouncer.PlayVoiceServerRpc("Stage5"); break;
            case 5: battleAnnouncer.PlayVoiceServerRpc("Stage6"); break;
            case 6: battleAnnouncer.PlayVoiceServerRpc("Stage7"); break;
            case 7: battleAnnouncer.PlayVoiceServerRpc("Stage8"); break;
            case 8: battleAnnouncer.PlayVoiceServerRpc("Stage9"); break;
            case 9: battleAnnouncer.PlayVoiceServerRpc("FinalStage"); break;
        }
    }

    [ClientRpc]
    private void ShowRoundClientRpc(int roundNumber)
    {
        HideAllRoundImages();

        Image roundToShow = null;

        if (roundNumber == 1) roundToShow = round1Image;
        else if (roundNumber == 2) roundToShow = round2Image;
        else if (roundNumber == 3) roundToShow = round3Image;

        if (battleAnnouncer != null)
        {
            if (roundNumber == 1) battleAnnouncer.PlayVoiceServerRpc("round1");
            else if (roundNumber == 2) battleAnnouncer.PlayVoiceServerRpc("round2");
            else if (roundNumber == 3) battleAnnouncer.PlayVoiceServerRpc("round3");
        }

        if (roundToShow != null)
            roundToShow.gameObject.SetActive(true);

        StartCoroutine(ShowFightSequence(roundToShow));
    }

    [ClientRpc]
    private void ShowKOClientRpc()
    {
        if (KOImage != null)
        {
            KOImage.gameObject.SetActive(true);
            if (battleAnnouncer != null)
                battleAnnouncer.PlayVoiceServerRpc("ko");
            HideClientControllClientRpc();
            StartCoroutine(HideKOAfterDelay(2f));
        }
    }

    private IEnumerator HideKOAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (KOImage != null)
            KOImage.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    private IEnumerator ShowFightSequence(Image roundImage)
    {
        if (IsServer && !gameData.isPracticeMode)
        {
            roundStarted.Value = false;
        }

        HideClientControllClientRpc();

        yield return new WaitForSeconds(2f);

        if (roundImage != null)
            roundImage.gameObject.SetActive(false);

        fightImage.gameObject.SetActive(true);
        if (IsServer && battleAnnouncer != null)
            battleAnnouncer.PlayVoiceServerRpc("fight");
        yield return new WaitForSeconds(1.2f);
        fightImage.gameObject.SetActive(false);

        if (IsServer && !gameData.isPracticeMode)
        {
            roundStarted.Value = true;
        }

        ShowClientControllClientRpc();

        roundActive = true;
        roundEnded = false;
    }
    [ClientRpc]
    private void HideClientControllClientRpc()
    {
        movementButtons.gameObject.SetActive(false);
        buttonLocked = true;
    }
    [ClientRpc]
    private void ShowClientControllClientRpc()
    {
        movementButtons.gameObject.SetActive(true);
        buttonLocked = false;
    }
    private void HideAllRoundImages()
    {
        round1Image.gameObject.SetActive(false);
        round2Image.gameObject.SetActive(false);
        round3Image.gameObject.SetActive(false);
        fightImage.gameObject.SetActive(false);
        endDisplayPanel.gameObject.SetActive(false);
        youWinImage.gameObject.SetActive(false);
        youWinImage.gameObject.SetActive(false);

        if (stageImages != null)
        {
            foreach (var img in stageImages)
            {
                if (img != null)
                    img.gameObject.SetActive(false);
            }
        }
    }

    public void PlayerDefeated(int loserId)
    {
        if (!IsServer || !roundActive || roundEnded)
            return;

        roundEnded = true;
        roundActive = false;
        roundStarted.Value = false;

        if (loserId == 1)
            player2Wins.Value++;
        else
            player1Wins.Value++;

        StartCoroutine(UpdateWinUiDelayed());
        StartCoroutine(PlayWinDeathSequence(loserId));
    }

    private IEnumerator PlayWinDeathSequence(int loserId)
    {
        bool hostLost = loserId == 1;

        if (battleVoice != null)
            battleVoice.StartDeathSequence(hostLost);
 
        yield return new WaitForSecondsRealtime(3f);

        StartCoroutine(ShowKOThenCheckMatch(loserId));
    }

    private IEnumerator ShowKOThenCheckMatch(int loserId)
    {
        ShowKOClientRpc();
        yield return new WaitForSecondsRealtime(2f);
        CheckForMatchEnd();
    }

    public void OnHome()
    {
        if (!IsServer) return;
        player2Wins.Value = 2;
        CheckForMatchEnd();
    }

    private void CheckForMatchEnd()
    {
        if (player1Wins.Value >= maxRounds || player2Wins.Value >= maxRounds)
        {
            bool playerWon = player1Wins.Value >= maxRounds;

            if (StoryModeManager.Instance.isStoryMode)
            {
                if (playerWon)
                    AdvanceStoryStage(true);
                else
                    ShowStoryLoseScreen(false);
                return;
            }
            else
            {
                int winnerId = playerWon ? 1 : 2;
                StartCoroutine(EndMatchRoutine(winnerId));
            }
        }
        else
        {
            StartCoroutine(RestartRound());
        }
    }

    private IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(roundRestartDelay);
        currentRound++;

        // Respawn players
        var spawner = FindFirstObjectByType<GameSpawner>();
        if (spawner != null)
            spawner.RespawnPlayersServerRpc();

        // Reset Timer
        var timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.RequestResetTimerServerRpc();
        }
        StartCoroutine(StartRoundRoutine());
    }
    private IEnumerator UpdateWinUiDelayed()
    {
        yield return null; // wait 1 frame
        UpdateWinUiClientRpc();
    }

    [ClientRpc]
    private void UpdateWinUiClientRpc()
    {
        P1win1.SetActive(player1Wins.Value >= 1);
        P1win2.SetActive(player1Wins.Value >= 2);
        P2win1.SetActive(player2Wins.Value >= 1);
        P2win2.SetActive(player2Wins.Value >= 2);
    }
    [ClientRpc]
    private void ResetWinClientRpc()
    {
        P1win1.SetActive(false);
        P1win2.SetActive(false);
        P2win1.SetActive(false);
        P2win2.SetActive(false);
    }

    private IEnumerator EndMatchRoutine(int winnerId)
    {
        if (gameData.isPVEMode)
        {
            EndMatchClientRpc(winnerId);
            yield break;
        }
        ulong winnerClientId = GetClientIdFromPlayerId(winnerId);
        ulong loserClientId = GetClientIdFromPlayerId(winnerId == 1 ? 2 : 1);

        // RPC to Winner
        EndMatchClientRpc( winnerId,new ClientRpcParams {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { winnerClientId }
                }
            }
        );

        // RPC to Loser
        EndMatchClientRpc(
            winnerId,
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { loserClientId }
                }
            }
        );
        yield return new WaitForSecondsRealtime(endPanelDelay);
    }

    [ClientRpc]
    private void EndMatchClientRpc(int winnerId, ClientRpcParams rpcParams = default)
    {
        if (StoryModeManager.Instance.isStoryMode)
            return;

        if (gameData.isPVEMode)
        {
            bool isWinner = winnerId == 1;
            HideAllRoundImages();
            endDisplayPanel.gameObject.SetActive(true);

            if (isWinner)
            {
                youWinImage.gameObject.SetActive(true);
                battleAnnouncer.PlayVoiceServerRpc("win");
            }
            else
            {
                youLoseImage.gameObject.SetActive(true);
                battleAnnouncer.PlayVoiceServerRpc("lose");
            }
            return;
        }
        else
        {
            HideAllRoundImages();
            endDisplayPanel.gameObject.SetActive(true);
            bool isWinner = (NetworkManager.Singleton.LocalClientId == GetClientIdFromPlayerId(winnerId));

            if (isWinner)
            {
                youWinImage.gameObject.SetActive(true);
                battleAnnouncer.PlayVoiceServerRpc("win");
            }
            else
            {
                youLoseImage.gameObject.SetActive(true);
                battleAnnouncer.PlayVoiceServerRpc("lose");
            }
        }
    }
    private void AdvanceStoryStage(bool playerWon)
    {
        if (!StoryModeManager.Instance.isStoryMode)
            return;

        StoryModeManager.Instance.EndBattle(playerWon);

        if (playerWon && currentStage >= maxStoryStages)
        {
            // Add hero to wonHeroes 
            string hero = StoryModeManager.Instance.chosenHero;
            var win = new System.Collections.Generic.List<string>(StoryModeSystem.Instance.data.wonHeroes);
            var lose = new System.Collections.Generic.List<string>(StoryModeSystem.Instance.data.lostHeroes);
            if (!win.Contains(hero) && !lose.Contains(hero))
                win.Add(hero);

            StoryModeSystem.Instance.data.wonHeroes = win.ToArray();
            StoryModeSystem.Instance.SaveProgress();
        }

        if (currentStage >= maxStoryStages)
        {
            if (storyFinishImage != null)
            {
                HideAllRoundImages();
                storyFinishImage.gameObject.SetActive(true);
                endDisplayPanel.gameObject.SetActive(true);
            }
            storyModeEnd = true;
            return;
        }
        ResetWinClientRpc();
        currentStage++;
        stageDisplayed = false;

        roundEnded = false;
        roundActive = false;
        player1Wins.Value = 0;
        player2Wins.Value = 0;
        currentRound = 1;

        var spawner = FindFirstObjectByType<GameSpawner>();
        if (spawner != null)
            spawner.OnNetworkSpawn();

        if (IsServer)
        {
            ShowStageIfStoryMode();
            StartCoroutine(StartRoundRoutine());
        }
    }
    private void ShowStoryLoseScreen(bool playerWon)
    {
        StoryModeManager.Instance.EndBattle(playerWon);
        HideAllRoundImages();
        endDisplayPanel.gameObject.SetActive(true);
        youLoseImage.gameObject.SetActive(true);
        battleAnnouncer.PlayVoiceServerRpc("lose");
        storyModeEnd = true;
    }

    public void BeginRound()
    {
        if (IsServer)
            StartCoroutine(StartRoundRoutine());
    }
    private ulong GetClientIdFromPlayerId(int playerId)
    {

        if (gameData.isPVEMode)
        {
            return NetworkManager.ServerClientId;
        }

        if (playerId == 1)
        {
            // Host is always Player 1
            return NetworkManager.ServerClientId;
        }
        else
        {
            // Client is always Player 2
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != NetworkManager.ServerClientId)
                    return clientId;
            }
        }

        Debug.LogError("No valid clientId found for playerId: " + playerId);
        return 0;
    }
}
