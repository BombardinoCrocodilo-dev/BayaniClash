using System;

[Serializable]
public class VoiceLineActions
{
    public string intro;
    public string kill;
    public string death;
}

[Serializable]
public class CharacterVoiceLines
{
    public string name;
    public VoiceLineActions english;
    public VoiceLineActions cebuano;
    public VoiceLineActions ilocano;
    public VoiceLineActions spanish;
    public VoiceLineActions tagalog;

    public VoiceLineActions GetActionsByLanguage(string language)
    {
        switch (language.ToLower())
        {
            case "english": return english;
            case "cebuano": return cebuano;
            case "ilocano": return ilocano;
            case "spanish": return spanish;
            case "tagalog": return tagalog;
            default: return english;
        }
    }
}

[Serializable]
public class VoiceLineDatabase
{
    public CharacterVoiceLines[] characters;
}
