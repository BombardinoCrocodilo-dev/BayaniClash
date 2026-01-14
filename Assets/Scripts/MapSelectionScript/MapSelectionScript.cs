using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class MapSelectionScript : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    [Header("Map Description")]
    [SerializeField] private RectTransform LocationIcon;
    [SerializeField] private Vector2[] Location;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private GameObject ConfirmPanel, dropDownPanel;
    //[SerializeField] private Button hostDcButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private string[] mapDescription;
    [SerializeField] private Image Image;
    [SerializeField] private Sprite[] mapSprites;

    private int currentIndex = 0;
    private float timer;

    // This syncs the selected map to clients
    NetworkVariable<int> syncedIndex = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
        );
    void Start()
    {
        if(!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
        {
            UpdateUI(currentIndex);
        }
    }
    public override void OnNetworkSpawn()
    {
        PanelDropDownAnim();
        // Set initial UI
        UpdateUI(currentIndex);

        // Sync map change for clients
        syncedIndex.OnValueChanged += (oldVal, newVal) =>
        {
            if (!IsServer)
            {
                currentIndex = newVal;
                UpdateUI(currentIndex);
            }
        };
    }

    private void UpdateUI(int index)
    {
        if (Location.Length > 0)
        {
            LocationIcon.anchoredPosition = Location[index];
            textMesh.text = mapDescription[index];
            Image.sprite = mapSprites[index];
            
        }
    }
    public void rightArrow()
    {
        AudioManager.Instance.PlayClick();
        if (currentIndex < Location.Length - 1)
        {
            currentIndex++;
            LocationIcon.anchoredPosition = Location[currentIndex];
            textMesh.text = mapDescription[currentIndex];
            Image.sprite = mapSprites[currentIndex];


            if (IsServer)
                syncedIndex.Value = currentIndex;
        }
    }

    public void leftArrow()
    {
        AudioManager.Instance.PlayClick();
        if (currentIndex > 0)
        {
            currentIndex--;
            LocationIcon.anchoredPosition = Location[currentIndex];
            textMesh.text = mapDescription[currentIndex];
            Image.sprite = mapSprites[currentIndex];

 
            if (IsServer)
                syncedIndex.Value = currentIndex;
        }
    }

    public void setTimer100()
    {
        AudioManager.Instance.PlayClick();
        gameData.timer = 100f;
        if (IsServer) LoadCharacterSelectionSceneServerRpc();
    }

    public void setTimer200()
    {
        AudioManager.Instance.PlayClick();
        gameData.timer = 200f;
        if (IsServer) LoadCharacterSelectionSceneServerRpc();
    }

    public void Confirm()
    {
        AudioManager.Instance.PlayClick();
        gameData.selectedMapIndex = currentIndex;
        if (StoryModeManager.Instance != null && StoryModeManager.Instance.isStoryMode || gameData.isPracticeMode)
        {
            if (IsServer) LoadCharacterSelectionSceneServerRpc();
        }
        else if(ConfirmPanel != null)
        {
            bool isActive = ConfirmPanel.activeSelf;
            ConfirmPanel.SetActive(!isActive);
            MapSelected();
        }
    }
    public void PanelDropDownAnim()
    {
        if (!IsServer)
        {
            Animator animator = dropDownPanel.GetComponent<Animator>();
            if (animator != null)
            {
                bool isOpen = animator.GetBool("ToggleDropdown");
                animator.SetBool("ToggleDropdown", !isOpen);
            }
            canvasGroup.interactable = false;
        }
    }
    public void MapSelected()
    {
        if (IsServer)
            syncedIndex.Value = currentIndex;
    }

    public int GetSyncedIndex()
    {
        return syncedIndex.Value;
    }
    [ServerRpc(RequireOwnership = false)]
    private void LoadCharacterSelectionSceneServerRpc()
    {
        OnStartGame("CharacterSelection");
    }
    public void OnStartGame(string targetScene)
    {
        SceneLoadingManager.TargetSceneName = targetScene;

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.SceneManager.LoadScene("LoadingScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
