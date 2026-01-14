using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class VoiceLineManager : NetworkBehaviour
{
    private VoiceLineDatabase voiceData;
    private Dictionary<string, CharacterVoiceLines> characterDict = new Dictionary<string, CharacterVoiceLines>();

    public string selectedLanguage = "English"; // default language

    private void Awake()
    {
        selectedLanguage = PlayerPrefs.GetString("VoiceLineLanguage", "English");

        LoadVoiceLinesJSON();
    }

    private void LoadVoiceLinesJSON()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("VoiceLines/VoiceLines");
        if (jsonText == null)
        {
            Debug.LogError("VoiceLines.json not found in Resources/VoiceLines!");
            return;
        }

        voiceData = JsonUtility.FromJson<VoiceLineDatabase>(jsonText.text);
        foreach (var character in voiceData.characters)
        {
            characterDict[character.name] = character;
        }
    }
    public bool TryGetCharacter(string characterName, out CharacterVoiceLines character)
    {
        return characterDict.TryGetValue(characterName, out character);
    }
    public string GetClipName(string characterName, string action)
    {
        if (!characterDict.ContainsKey(characterName))
        {
            Debug.LogWarning($"Character '{characterName}' not found!");
            return null;
        }

        var character = characterDict[characterName];
        var actions = character.GetActionsByLanguage(selectedLanguage);

        string clipName = action.ToLower() switch
        {
            "intro" => StripExtension(actions.intro),
            "kill" => StripExtension(actions.kill),
            "death" => StripExtension(actions.death),
            _ => StripExtension(actions.intro)
        };

        return clipName;
    }

    private string StripExtension(string fileName)
    {
        int index = fileName.LastIndexOf('.');
        if (index > 0) return fileName.Substring(0, index);
        return fileName;
    }

    public void PlayVoiceLine(string characterName, string action)
    {
        string clipName = GetClipName(characterName, action);
        if (clipName == null) return;

        string path = $"VoiceLines/Audio/{characterName}/{selectedLanguage}/{clipName}";
        AudioClip clip = Resources.Load<AudioClip>(path);

        if (clip != null) AudioManager.Instance.PlayVoiceLine(clip);
        else Debug.LogWarning($"Clip not found at path: {path}");
    }
    public void PlayVoiceLineMultiplayer(string characterName, string action)
    {
        if (!IsOwner) return; // only owner triggers
        PlayVoiceLineServerRpc(characterName, action);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayVoiceLineServerRpc(string characterName, string action)
    {
        PlayVoiceLineClientRpc(characterName, action);
    }

    [ClientRpc]
    private void PlayVoiceLineClientRpc(string characterName, string action)
    {
        string clipName = GetClipName(characterName, action);
        if (clipName == null) return;

        string path = $"VoiceLines/Audio/{characterName}/{selectedLanguage}/{clipName}";
        AudioClip clip = Resources.Load<AudioClip>(path);

        if (clip != null) AudioManager.Instance.PlayVoiceLine(clip);
        else Debug.LogWarning($"Clip not found at path: {path}");
    }

    public void SetLanguage(string language)
    {
        if (selectedLanguage == language) return;

        selectedLanguage = language;
        PlayerPrefs.SetString("VoiceLineLanguage", language);
        PlayerPrefs.Save();

        Debug.Log($"Voice line language set to: {selectedLanguage}");
    }
}
