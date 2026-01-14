using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroPanel : MonoBehaviour
{
    private static IntroPanel Instance;

    [Header("UI References")]
    public GameObject introPanel;
    public Toggle dontShowAgainToggle;
    public TextMeshProUGUI text;
    public Button continueButton;

    private const string PREF_KEY = "ShowIntroPanel";
    private static bool hasShownThisSession = false;

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
        }
    }
    private void Start()
    {
        // Check if player disabled it permanently
        int showPanel = PlayerPrefs.GetInt(PREF_KEY, 1);

        // Only show if:
        // 1. Player allows it (showPanel == 1)
        // 2. It has not shown yet this app launch (!hasShownThisSession)
        if (showPanel == 1 && !hasShownThisSession)
        {
            introPanel.SetActive(true);
            hasShownThisSession = true;   // Mark that it has shown ONCE
        }
        else
        {
            introPanel.SetActive(false);
            dontShowAgainToggle.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
        }

        continueButton.onClick.AddListener(OnContinue);
    }

    private void OnContinue()
    {
        if (dontShowAgainToggle.isOn)
        {
            PlayerPrefs.SetInt(PREF_KEY, 0); // Never show again
            PlayerPrefs.Save();
        }

        introPanel.SetActive(false);
    }
    public void OpenIntroPanel()
    {
        introPanel.SetActive(true);
    }
}
