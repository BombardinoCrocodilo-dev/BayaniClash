using UnityEngine;

public class DisableP2Ui : MonoBehaviour
{
    [SerializeField] private GameObject P2UI;
    private GameData gameData => Data.GameData;
    void Start()
    {
        if (!gameData.isPracticeMode) return;
        P2UI.SetActive(false);
    }
}
