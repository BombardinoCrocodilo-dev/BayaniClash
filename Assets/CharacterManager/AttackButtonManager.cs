using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AttackButtonManager : MonoBehaviour
{
    public static AttackButtonManager Instance;

    public Button attackButton;
    public Button Skill1;
    public Button Skill2;
    public Button Skill3;

    private PlayerController controller;

    void Awake()
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

    public void SetController(PlayerController playerController)
    {
        controller = playerController;
    }

    private void Start()
    {
        attackButton.onClick.AddListener(() =>{ if (controller != null) controller.OnNormalAttack(); });
        Skill1.onClick.AddListener(() => { if (controller != null) controller.OnSkill(1); });
        Skill2.onClick.AddListener(() => { if (controller != null) controller.OnSkill(2); });
        Skill3.onClick.AddListener(() => { if (controller != null) controller.OnSkill(3); });
    }
}