[System.Serializable]
public class CutsceneItem
{
    public string name;
    public string file;
}

[System.Serializable]
public class CutsceneData
{
    public CutsceneItem[] cutscenes;
}
