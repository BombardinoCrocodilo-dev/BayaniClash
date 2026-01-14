using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance;

    private GameData gameData => Data.GameData;

    [Header("Player 1 UI")]
    public Image player1HealthFill;
    public Image player1ManaFill;

    [Header("Player 2 UI")]
    public Image player2HealthFill;
    public Image player2ManaFill;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdatePlayer1Health(float value) => player1HealthFill.fillAmount = value;
    public void UpdatePlayer1Mana(float value) => player1ManaFill.fillAmount = value;
    public void UpdatePlayer2Health(float value) => player2HealthFill.fillAmount = value;
    public void UpdatePlayer2Mana(float value) => player2ManaFill.fillAmount = value;
}
