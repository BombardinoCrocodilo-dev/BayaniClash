using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class Player : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> currentMana = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> isBlocking = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private float maxHealth = 100f;
    private float maxMana = 100f;

    [HideInInspector] public PlayerController controller;
    private RoundHandler roundHandler;
    public bool isBlock = false;

    private float manaRegenRate = 1f;   
    private float regenInterval = 1f;  
    private Coroutine manaRegenCoroutine;

    public bool gregodioShield;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<PlayerController>();
        if (IsServer)
        {
            roundHandler = FindAnyObjectByType<RoundHandler>();
            manaRegenCoroutine = StartCoroutine(ManaRegenRoutine());
        }

        currentHealth.OnValueChanged += OnHealthChanged;
        currentMana.OnValueChanged += OnManaChanged;

        OnHealthChanged(0, currentHealth.Value);
        OnManaChanged(0, currentMana.Value);
    }
    private IEnumerator ManaRegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);

            if (gameData.isPracticeMode)
            {
                if (currentMana.Value < maxMana)
                {
                    AddMana(100 * regenInterval);
                }
            }

            if (currentMana.Value < maxMana)
            {
                AddMana(manaRegenRate * regenInterval);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (manaRegenCoroutine != null)
            StopCoroutine(manaRegenCoroutine);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestAddManaServerRpc(float mana)
    {
        AddMana(mana);
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;

        if (gregodioShield) return;
        if (isBlocking.Value)
        {
            isBlock = true;
            PlayBlockAnimationClientRpc();
            LockMovementClientRpc(0.3f);
            StartCoroutine(ResetHit());
            return;
        }

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damage, 0, maxHealth);
        LockMovementClientRpc(0.3f);
        StartCoroutine(ResetHit());
    }

    [ClientRpc]
    public void LockMovementClientRpc(float duration)
    {
        if (controller != null && controller.IsOwner)
            controller.StartCoroutine(controller.LockMovement(duration));
    }

    public void TakeMana(float mana)
    {
        if (!IsServer) return;
        currentMana.Value = Mathf.Clamp(currentMana.Value - mana, 0, maxMana);
    }

    public void AddMana(float mana)
    {
        if (!IsServer) return;
        currentMana.Value = Mathf.Clamp(currentMana.Value + mana, 0, maxMana);
    }

    public void AddHealth(float health)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value + health, 0, maxHealth);
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (PlayerStatsManager.Instance == null) return;
        if (gameData.isPracticeMode) return;
        
        float fill = newValue / maxHealth;

        if (gameData.isPVEMode || StoryModeManager.Instance.isStoryMode)
        {
            if (TryGetComponent<PlayerController>(out _))
                PlayerStatsManager.Instance.UpdatePlayer1Health(fill);
            else
                PlayerStatsManager.Instance.UpdatePlayer2Health(fill);

            if (newValue <= 0f)
            {
                if (TryGetComponent<PlayerController>(out _))
                    roundHandler.PlayerDefeated(1);
                else
                    roundHandler.PlayerDefeated(2);
            }
        }
        else
        {
            if (OwnerClientId == 0)
                PlayerStatsManager.Instance.UpdatePlayer1Health(fill);
            else
                PlayerStatsManager.Instance.UpdatePlayer2Health(fill);
        }

        Timer timer = FindAnyObjectByType<Timer>();
        if (newValue <= 0f && roundHandler != null)
        {
            int loserId = OwnerClientId == 0 ? 1 : 2;

            roundHandler.PlayerDefeated(loserId);
            PlayWinLoseAnimationClientRpc(false);

            Player enemy = GetEnemyPlayer();
            if (enemy != null)
                enemy.PlayWinLoseAnimationClientRpc(true);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void TimerEndedServerRpc()
    {
        OnTimerEndMatch();
    }
    private void OnTimerEndMatch()
    {
        if (!IsServer) return;
        float p1Health = PlayerStatsManager.Instance.player1HealthFill.fillAmount;
        float p2Health = PlayerStatsManager.Instance.player2HealthFill.fillAmount;

        int loserId = (p1Health > p2Health) ? 2 : 1;
        int winnerId = (loserId == 1) ? 2 : 1;
        if (gameData.isPVEMode || StoryModeManager.Instance.isStoryMode)
        {
            roundHandler.PlayerDefeated(loserId);

            bool isPlayer1 = TryGetComponent<PlayerController>(out _);
            bool isWin = (winnerId == 1 && isPlayer1) || (winnerId == 2 && !isPlayer1);

            PlayWinLoseAnimationClientRpc(isWin);

            Player enemy = GetEnemyPlayer();
            if (enemy != null)
                enemy.PlayWinLoseAnimationClientRpc(!isWin);

            return;
        }
        roundHandler.PlayerDefeated(loserId);

        bool isPlayer1Client = (OwnerClientId == 0);

        bool playerWins =
            (winnerId == 1 && isPlayer1Client) ||
            (winnerId == 2 && !isPlayer1Client);

        PlayWinLoseAnimationClientRpc(playerWins);

        Player enemyPVP = GetEnemyPlayer();
        if (enemyPVP != null)
            enemyPVP.PlayWinLoseAnimationClientRpc(!playerWins);
    }


    private IEnumerator ResetHit()
    {
        yield return new WaitForSeconds(0.3f);
        isBlock = false;
    }

    private void OnManaChanged(float oldValue, float newValue)
    {
        if (PlayerStatsManager.Instance == null) return;
        float fill = newValue / maxMana;

        if (gameData.isPVEMode || StoryModeManager.Instance.isStoryMode)
        {
            if (TryGetComponent<PlayerController>(out _))
                PlayerStatsManager.Instance.UpdatePlayer1Mana(fill);
            else
                PlayerStatsManager.Instance.UpdatePlayer2Mana(fill);
        }
        else
        {
            if (OwnerClientId == 0)
                PlayerStatsManager.Instance.UpdatePlayer1Mana(fill);
            else
                PlayerStatsManager.Instance.UpdatePlayer2Mana(fill);
        }
    }

    [ClientRpc]
    private void PlayBlockAnimationClientRpc()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Block");
        }
    }

    [ClientRpc]
    public void PlayHitAnimationClientRpc()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("isHit");
        }
    }
    [ClientRpc]
    private void PlayWinLoseAnimationClientRpc(bool isWin)
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            if (isWin)
            {
                anim.ResetTrigger("Win");
                anim.SetTrigger("Win");
            }
            else
            {
                anim.ResetTrigger("Lose");
                anim.SetTrigger("Lose");
            }
        }
    }


    public void ResetForNextRound()
    {
        if (!IsServer) return;
        currentHealth.Value = maxHealth;
        currentMana.Value = 0f;
        isBlocking.Value = false;
    }

    public Player GetEnemyPlayer()
    {
        foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (obj.TryGetComponent(out Player player))
            {
                if (player != this)
                    return player;
            }
        }
        return null;
    }


    public abstract void NormalAttack();
    public abstract void Skill1();
    public abstract void Skill2();
    public abstract void Skill3();
}
