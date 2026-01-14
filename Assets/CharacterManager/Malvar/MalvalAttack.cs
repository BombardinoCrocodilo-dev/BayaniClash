using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

public class MalvalAttack : Player
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerController pc;
    private HitBox hitbox;
    private float lastAttackTime = 0f; // Track last attack time
    private float Cooldown = 1f; // 1 second cooldown

    private float skill1ManaCost = 20;
    private float skill2ManaCost = 30;
    private float skill3ManaCost = 60;

    private bool onTriggerBuff;

    [SerializeField] private Transform buffPoint;

    [SerializeField] private GameObject hitboxObject;
    [SerializeField] private GameObject Buff;
    [SerializeField] private GameObject Slash;

    [SerializeField] private AudioClip sfxNormalAttack;
    [SerializeField] private AudioClip sfxSkill1;
    [SerializeField] private AudioClip sfxSkill2;
    [SerializeField] private AudioClip sfxSkill3;
    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        hitbox = hitboxObject.GetComponent<HitBox>();
        pc = GetComponent<PlayerController>();
        if (hitbox != null)
        {
            hitbox.owner = this;
        }
    }
    public void PlaySound(string soundName)
    {
        PlaySoundServerRpc(soundName);
    }

    public override void NormalAttack()
    {
        if (!IsOwner) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
        if (hitbox != null)
        {
            if(onTriggerBuff)
            {
                hitbox.Initialize(this, 20f, 15f);
            }
            else
            {
                hitbox.Initialize(this, 10f, 15f);
            }
        }
    }
    public override void Skill1()
    {
        if (!IsOwner) return;
        if (currentMana.Value < skill1ManaCost) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        DeductManaServerRpc(skill1ManaCost);
        if (hitbox != null)
            hitbox.Initialize(this, 20f, 0f);
        anim.SetTrigger("Skill1");
    }
    public override void Skill2()
    {
        if (!IsOwner) return;
        if (currentMana.Value < skill2ManaCost) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        DeductManaServerRpc(skill2ManaCost);
        anim.SetTrigger("Skill2");
        StartCoroutine(onBuff());
    }
    public override void Skill3()
    {
        if (!IsOwner) return;
        if (currentMana.Value < skill3ManaCost) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        DeductManaServerRpc(skill3ManaCost);
        anim.SetTrigger("Skill3");
    }
    private IEnumerator onBuff()
    {
        onTriggerBuff = true;
        yield return new WaitForSeconds(5f);
        onTriggerBuff = false;
    }
    public void onBuffTrigger()
    {
        if (!IsOwner) return;
        OnBuffServerRpc();
    }
    public void onSlashTrigger()
    {
        OnSlashTriggerServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnBuffServerRpc()
    {
        GameObject buffInstance = Instantiate(Buff, buffPoint.position, buffPoint.rotation);
        buffInstance.GetComponent<NetworkObject>().Spawn();

        // Tell all clients to follow this player
        FollowPlayer follow = buffInstance.GetComponent<FollowPlayer>();
        follow.target = buffPoint;  // or transform if you want it to follow the player center

        Destroy(buffInstance, 5f);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnSlashTriggerServerRpc()
    {
        Player enemy = GetEnemyPlayer();
        if (enemy == null) return;

        Vector3 enemyPos = enemy.transform.position + new Vector3(0, 120f, 0); // Adjust height if needed
        GameObject thunderInstance = Instantiate(Slash, enemyPos, Quaternion.identity);
        thunderInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = thunderInstance.GetComponent<HitBox>();
        hb.Initialize(this, 40f, 0f);
        Destroy(thunderInstance, 1.4f);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DeductManaServerRpc(float amount)
    {
        AddMana(-amount);
    }
    [ServerRpc]
    private void PlaySoundServerRpc(string soundName)
    {
        PlaySoundClientRpc(soundName);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(string soundName)
    {
        switch (soundName)
        {
            case "Normal":
                AudioManager.Instance.PlaySFX(sfxNormalAttack);
                break;
            case "Skill1":
                AudioManager.Instance.PlaySFX(sfxSkill1);
                break;
            case "Skill2":
                AudioManager.Instance.PlaySFX(sfxSkill2);
                break;
            case "Skill3":
                AudioManager.Instance.PlaySFX(sfxSkill3);
                break;
        }
    }
}
