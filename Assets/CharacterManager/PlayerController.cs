using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject playerUIPrefab;

    private float moveSpeed = 130f;
    private Rigidbody2D rb;
    private float moveInput;
    private Animator anim;
    [HideInInspector] public Player player;
    private GameObject playerUIButtonInstance;
    private Transform enemy;
    private RoundHandler roundHandler;

    private bool lastBlockingState = false;
    public bool facingRight = true;
    public bool movementLocked = false;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();
        roundHandler = FindFirstObjectByType<RoundHandler>();

        // get enemy 
        enemy = player.GetEnemyPlayer()?.transform;

        if (IsOwner)
            CreatePlayerButtons();

        // initial facing based on rotation
        facingRight = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 0f)) < 90f;
    }

    private void CreatePlayerButtons()
    {
        playerUIButtonInstance = Instantiate(playerUIPrefab);
        playerUIButtonInstance.name = $"PlayerUI_{OwnerClientId}";
        playerUIButtonInstance.transform.SetParent(this.transform, false);

        foreach (var btn in playerUIButtonInstance.GetComponentsInChildren<Button>())
        {
            switch (btn.name)
            {
                case "AttackButton": btn.onClick.AddListener(OnNormalAttack); break;
                case "Skill1Button": btn.onClick.AddListener(() => OnSkill(1)); break;
                case "Skill2Button": btn.onClick.AddListener(() => OnSkill(2)); break;
                case "Skill3Button": btn.onClick.AddListener(() => OnSkill(3)); break;
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        moveInput = SimpleInput.GetAxis("Horizontal");

        if (movementLocked)
        {
            moveInput = 0f;
            // ensure not blocking while locked
            if (lastBlockingState)
            {
                lastBlockingState = false;
                UpdateBlockingServerRpc(false);
            }
            return;
        }

        // Flip only when player actively moves again (delayed flip behavior)
        if (moveInput != 0)
            UpdateFacingOnMove();

        // compute local blocking (based on enemy pos and move input)
        bool isBlockingNow = ComputeIsMovingBackward();

        // Sync to server when changed
        if (isBlockingNow != lastBlockingState)
        {
            lastBlockingState = isBlockingNow;
            UpdateBlockingServerRpc(isBlockingNow);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (movementLocked)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("Walk", false);
            return;
        }
        if (roundHandler.buttonLocked)
        {
            playerUIButtonInstance.gameObject.SetActive(false);
        }
        else
        {
            playerUIButtonInstance.gameObject.SetActive(true);
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        anim.SetBool("Walk", moveInput != 0);
    }

    private void UpdateFacingOnMove()
    {
        // refresh enemy reference if needed
        if (enemy == null && player != null)
            enemy = player.GetEnemyPlayer()?.transform;

        if (enemy == null) return;

        bool shouldFaceRight = transform.position.x < enemy.position.x;

        if (shouldFaceRight != facingRight)
            Flip(shouldFaceRight);
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }
    private bool ComputeIsMovingBackward()
    {
        if (player == null) return false;

        var enemyPlayer = player.GetEnemyPlayer();
        if (enemyPlayer == null) return false;

        float enemyX = enemyPlayer.transform.position.x;
        float myX = transform.position.x;
        float directionToEnemy = Mathf.Sign(enemyX - myX); // +1 if enemy is right, -1 if enemy is left

        bool isBackward = (directionToEnemy > 0 && moveInput < 0) || (directionToEnemy < 0 && moveInput > 0);

        return isBackward;
    }

    [ServerRpc(RequireOwnership = true)]
    private void UpdateBlockingServerRpc(bool isBlocking, ServerRpcParams rpcParams = default)
    {
        if (player != null)
            player.isBlocking.Value = isBlocking;
    }

    public IEnumerator LockMovement(float duration)
    {
        movementLocked = true;
        yield return new WaitForSeconds(duration);
        movementLocked = false;
    }

    public void OnNormalAttack()
    {
        if (!IsOwner) return;
        RequestNormalAttackServerRpc();
    }

    public void OnSkill(int skillIndex)
    {
        if (!IsOwner) return;
        RequestSkillServerRpc(skillIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestNormalAttackServerRpc(ServerRpcParams rpcParams = default)
    {
        player.NormalAttack();
        NormalAttackClientRpc();
    }

    [ClientRpc]
    void NormalAttackClientRpc()
    {
        if (IsServer) return;
        player.NormalAttack();
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestSkillServerRpc(int skillIndex, ServerRpcParams rpcParams = default)
    {
        ExecuteSkill(index: skillIndex);
        SkillClientRpc(skillIndex);
    }

    [ClientRpc]
    void SkillClientRpc(int skillIndex)
    {
        if (IsServer) return;
        ExecuteSkill(index: skillIndex);
    }

    private void ExecuteSkill(int index)
    {
        switch (index)
        {
            case 1: player.Skill1(); break;
            case 2: player.Skill2(); break;
            case 3: player.Skill3(); break;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && playerUIButtonInstance != null)
            Destroy(playerUIButtonInstance);
    }
}
