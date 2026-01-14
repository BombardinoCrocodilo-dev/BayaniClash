using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectManager : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    [Header("Character Lists")]
    [SerializeField] private GameObject[] hostCharacters;
    [SerializeField] private GameObject[] clientCharacters;
    [SerializeField] private TextMeshProUGUI P1charname;
    [SerializeField] private TextMeshProUGUI P2charname;
    [SerializeField] private string[] charname;
 
    [Header("UI Buttons")]
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private Button lockButton;

    [Header("Character Status Texts")]
    private TMP_Text[] statusTexts;

    [Header("Synced Data")]
    public NetworkVariable<int> ClientSelectedIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private int currentHostIndex = -1;
    private int currentClientIndex = -1;
    private bool hostLocked = false;
    private bool clientLocked = false;

    private void Start()
    {
        // Assign all character buttons
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(index));
        }

        foreach (var c in hostCharacters) c.SetActive(false);
        foreach (var c in clientCharacters) c.SetActive(false);

        lockButton.onClick.AddListener(OnLockedButtonClicked);

        statusTexts = new TMP_Text[characterButtons.Length];
        for (int i = 0; i < characterButtons.Length; i++)
        {
            statusTexts[i] = characterButtons[i].GetComponentInChildren<TMP_Text>();
            if (statusTexts[i] != null)
                statusTexts[i].text = ""; // clear at start
        }

        if (gameData.isPracticeMode)
        {
            return;
        }

        if (StoryModeManager.Instance.isStoryMode)
        {
            LoadStoryModeProgress();
            UnlockAllLockedCharactersIfNeeded();
            UpdateCharacterStatusVisuals();
        }


        if (gameData.isPVEMode)
        {
            HandleOfflineCharacterSelection();
        }
    }
    private void UnlockAllLockedCharactersIfNeeded()
    {
        if (StoryModeSystem.Instance == null) return;
        int lockedCount = StoryModeSystem.Instance.data.lostHeroes.Length + 
                            StoryModeSystem.Instance.data.wonHeroes.Length;
        if (lockedCount >= 10)
        {
            for(int i = 0; i < characterButtons.Length; i++)
            {
                characterButtons[i].interactable = true;
            }
        }
    }
    private void UpdateCharacterStatusVisuals()
    {
        if (StoryModeSystem.Instance == null) return;

        for (int i = 0; i < hostCharacters.Length; i++)
        {
            string heroName = hostCharacters[i].name;
            if (statusTexts[i] == null) continue;

            statusTexts[i].text = "";


            // Check if defeated or lost
            if (Array.Exists(StoryModeSystem.Instance.data.wonHeroes, e => e == heroName))
            {
                statusTexts[i].text = "W";
                statusTexts[i].color = Color.green;
                statusTexts[i].gameObject.SetActive(true);
            }
            else if (Array.Exists(StoryModeSystem.Instance.data.lostHeroes, h => h == heroName))
            {
                statusTexts[i].text = "L";
                statusTexts[i].color = Color.red;
                statusTexts[i].gameObject.SetActive(true);
                
            }
            else
            {
                statusTexts[i].gameObject.SetActive(false);
            }
        }
    }


    private void HandleOfflineCharacterSelection()
    {
        Debug.Log("Offline mode: selecting random AI character...");
        int clientIndex = UnityEngine.Random.Range(0, clientCharacters.Length);
        SetClientCharacter(clientIndex);
        gameData.clientCharacterIndex = clientIndex;
        PlayerPrefs.SetInt("ClientSelectedCharacter", clientIndex);
        PlayerPrefs.Save();
        clientLocked = true;
        UpdatUIClientRpc(currentHostIndex, clientIndex);
    }

    private void OnCharacterButtonClicked(int index)
    {
        if (IsServer)
        {
            if (hostLocked) return;

            SetHostCharacter(index);
            gameData.hostCharacterIndex = index;
            PlayerPrefs.SetInt("HostSelectedCharacter", index);
            PlayerPrefs.Save();

            UpdateHostCharacterClientRpc(index);
        }
        else
        {
            if (clientLocked) return;
            SelectCharacterServerRpc(index);
        }
    }

    public void OnLockedButtonClicked()
    {
        if (IsServer)
        {
            hostLocked = true;
            if (StoryModeManager.Instance != null && StoryModeManager.Instance.isStoryMode)
            {
                string chosenHero = hostCharacters[currentHostIndex].name;
                StoryModeManager.Instance.SetChosenHero(chosenHero);
                Debug.Log("[StoryMode] Hero selected: " + chosenHero);
            }

            CheckAndLoadBattleScene();
        }
        else
        {
            clientLocked = true;
            NotifyServerClientLockedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectCharacterServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        // Update and store
        ClientSelectedIndex.Value = index;
        gameData.clientCharacterIndex = index;
        PlayerPrefs.SetInt("ClientSelectedCharacter", index);
        PlayerPrefs.Save();

        // Apply visuals
        SetClientCharacter(index);
        UpdateClientCharacterClientRpc(index);
    }

    private void SetHostCharacter(int index)
    {
        if (currentHostIndex != -1)
            hostCharacters[currentHostIndex].SetActive(false);

        hostCharacters[index].SetActive(true);
        currentHostIndex = index;
        UpdatUIClientRpc(currentHostIndex, currentClientIndex);
    }

    private void SetClientCharacter(int index)
    {
        // Skip AI auto selection during story mode
        if (StoryModeManager.Instance != null && StoryModeManager.Instance.isStoryMode)
            return;

        if (currentClientIndex != -1)
            clientCharacters[currentClientIndex].SetActive(false);

        clientCharacters[index].SetActive(true);
        currentClientIndex = index;
        gameData.clientCharacterIndex = index;
        UpdatUIClientRpc(currentHostIndex, currentClientIndex);
    }

    [ClientRpc]
    private void UpdateHostCharacterClientRpc(int index)
    {
        if (IsServer) return;
        SetHostCharacter(index);
    }

    [ClientRpc]
    private void UpdateClientCharacterClientRpc(int index)
    {
        if (IsServer) return;
        SetClientCharacter(index);
    }
    [ClientRpc]
    private void UpdatUIClientRpc(int hostIndex, int clientIndex)
    {
        if (hostIndex >= 0 && hostIndex < charname.Length)
            P1charname.SetText(charname[hostIndex]);
        else
            P1charname.SetText("");

        if (clientIndex >= 0 && clientIndex < charname.Length)
            P2charname.SetText(charname[clientIndex]);
        else
            P2charname.SetText("");

    }
    [ServerRpc(RequireOwnership = false)]
    private void NotifyServerClientLockedServerRpc()
    {
        clientLocked = true;
        CheckAndLoadBattleScene();
    }
    private void CheckAndLoadBattleScene()
    {
        if (gameData.isPracticeMode)
        {
            if (hostLocked)
            {
                NetworkManager.SceneManager.LoadScene("BattleScene", LoadSceneMode.Single);
            }
        }
        if (StoryModeManager.Instance != null && StoryModeManager.Instance.isStoryMode)
        {
            if (hostLocked)
            {
                NetworkManager.SceneManager.LoadScene("LoadingBattle", LoadSceneMode.Single);
            }
        }
        if (hostLocked && clientLocked)
        {
            NetworkManager.SceneManager.LoadScene("VSLoading", LoadSceneMode.Single);
        }
    }
    private void LoadStoryModeProgress()
    {
        if (StoryModeManager.Instance.isStoryMode && StoryModeSystem.Instance != null)
        {
            // Load Progress
            StoryModeSystem.Instance.LoadProgress();

            // Update character buttons to reflect locked heroes
            for (int i = 0; i < hostCharacters.Length; i++)
            {
                string heroName = hostCharacters[i].name;
                bool isLocked = StoryModeSystem.Instance.IsHeroLost(heroName) || StoryModeSystem.Instance.IsHeroWon(heroName);
                characterButtons[i].interactable = !isLocked;
            }
        }
    }
}
