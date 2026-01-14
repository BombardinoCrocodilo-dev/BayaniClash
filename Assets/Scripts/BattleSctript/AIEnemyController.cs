using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class AIEnemyController : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private Player player;
    private Player targetPlayer;

    [Header("AI Settings")]
    [SerializeField] private float moveSpeed = 100f;
    private float attackRange = 100f; 
    [SerializeField] private float skillRange = 100f; 
    [SerializeField] private float skillCooldown = 3f;
    private float normalAttackCooldown = 2f; 

    private bool facingRight = true;
    private float lastSkillTime = 0f;
    private float lastNormalAttackTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    void Start()
    {
        if (!IsServer) return;

        // Find RoundHandler
        RoundHandler roundHandler = FindFirstObjectByType<RoundHandler>();

        // Find the target player
        foreach (var p in Object.FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (p != player)
            {
                targetPlayer = p;
                break;
            }
        }

        StartCoroutine(AILoop(roundHandler));
    }


    private IEnumerator AILoop(RoundHandler roundHandler)
    {
        while (true)
        {
            if (roundHandler == null || !roundHandler.IsRoundStarted || targetPlayer == null)
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool("Walk", false);
                yield return null;
                continue;
            }

            Vector2 aiPos = transform.position;
            Vector2 playerPos = targetPlayer.transform.position;
            float direction = playerPos.x - aiPos.x;
            float distance = Mathf.Abs(direction);

            UpdateFacing(direction);

           
            if (distance > attackRange)
            {
                // 50% chance to move this frame (AI reacts intermittently)
                if (Random.value > 0.1f)
                {
                    rb.linearVelocity = new Vector2(Mathf.Sign(direction) * moveSpeed, rb.linearVelocity.y);
                    anim.SetBool("Walk", true);
                }
                else
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    anim.SetBool("Walk", false);
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("Walk", false);
                TryAttack(distance);
            }

            // Update blocking
            bool isBackward = ComputeIsMovingBackward();
            if (player.isBlocking.Value != isBackward)
                player.isBlocking.Value = isBackward;

            yield return new WaitForSeconds(0.1f); // Slight delay to avoid frame-perfect chasing
        }
    }
    private void UpdateFacing(float direction)
    {
        bool shouldFaceRight = direction > 0;
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
        }
    }

    private void TryAttack(float distance)
    {
        // Skill attack
        if (Time.time - lastSkillTime >= skillCooldown && distance <= skillRange)
        {
            lastSkillTime = Time.time;
            int skillIndex = Random.Range(1, 4);
            UseSkillServerRpc(skillIndex);
        }
        // Normal attack with cooldown
        else if (distance <= attackRange && Time.time - lastNormalAttackTime >= normalAttackCooldown)
        {
            lastNormalAttackTime = Time.time;
            NormalAttack();
        }
    }

    private void NormalAttack()
    {
        player.NormalAttack();
    }
    [ServerRpc]
    private void UseSkillServerRpc(int index)
    {
        UseSkill(index:  index);
    }

    private void UseSkill(int index)
    {
        switch (index)
        {
            case 1: player.Skill1(); break;
            case 2: player.Skill2(); break;
            case 3: player.Skill3(); break;
        }
    }

    private bool ComputeIsMovingBackward()
    {
        if (targetPlayer == null) return false;

        float directionToPlayer = Mathf.Sign(targetPlayer.transform.position.x - transform.position.x);
        float moveInput = rb.linearVelocity.x;

        // Moving away from player = backward
        return (directionToPlayer > 0 && moveInput < 0) || (directionToPlayer < 0 && moveInput > 0);
    }
}
