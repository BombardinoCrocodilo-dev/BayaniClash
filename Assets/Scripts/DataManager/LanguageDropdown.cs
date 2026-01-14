using TMPro;
using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    private VoiceLineManager voiceLineManager;

    private void Start()
    {
        // Get reference to VoiceLineManager
        voiceLineManager = FindAnyObjectByType<VoiceLineManager>();

        // Setup dropdown options
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new System.Collections.Generic.List<string>
        { "English", "Cebuano", "Ilocano", "Tagalog", "Spanish" });

        // Load saved language from PlayerPrefs
        string savedLanguage = PlayerPrefs.GetString("VoiceLineLanguage", "English");
        int defaultIndex = languageDropdown.options.FindIndex(o => o.text == savedLanguage);
        if (defaultIndex == -1) defaultIndex = 0; // fallback if something is wrong
        languageDropdown.value = defaultIndex;
        languageDropdown.RefreshShownValue();

        // Update VoiceLineManager with saved language if available
        if (voiceLineManager != null)
            voiceLineManager.selectedLanguage = savedLanguage;

        // Listen for changes
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        string selected = languageDropdown.options[index].text;

        // Update VoiceLineManager
        if (voiceLineManager != null)
            voiceLineManager.selectedLanguage = selected;

        // Save to PlayerPrefs immediately
        PlayerPrefs.SetString("VoiceLineLanguage", selected);
        PlayerPrefs.Save();

        Debug.Log($"Language set to: {selected}");
    }
}
