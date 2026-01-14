using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class JoseRizalAttacks : Player
{
    private Animator anim;
    private HitBox hitbox;
    private float lastAttackTime = 0f;
    private float Cooldown = 1f;

    private float skill1ManaCost = 20f;
    private float skill2ManaCost = 35f;
    private float skill3ManaCost = 60f;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform healingPoint;
    [SerializeField] private Transform shieldPoint;

    [SerializeField] private GameObject hitboxObject;
    [SerializeField] private GameObject shootEffect;
    [SerializeField] private GameObject healEffect;
    [SerializeField] private GameObject debuffEffect;
    [SerializeField] private GameObject shieldEffect;

    [SerializeField] private AudioClip sfxNormalAttack;
    [SerializeField] private AudioClip sfxSkill1;
    [SerializeField] private AudioClip sfxSkill2;
    [SerializeField] private AudioClip sfxSkill3;

    private void Start()
    {
        anim = GetComponent<Animator>();
        hitbox = hitboxObject.GetComponent<HitBox>();
        if (hitboxObject != null)
        {
            hitboxObject.GetComponent<Collider2D>().enabled = false;
        }
               if(hitbox != null)
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
            hitbox.Initialize(this, 10f, 15f);
        else
            Debug.Log("Hitbox is null");
    }

    public override void Skill1()
    {
        if (!IsOwner) return;
        if (currentMana.Value < skill1ManaCost) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;

        DeductManaServerRpc(skill1ManaCost);
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

    public void OnShootTrigger()
    {
        if (!IsOwner) return;
        Skill1ServerRpc();
    }
    public void Skill2Trigger()
    {
        Skill2ServerRpc();
    }
    public void Skill3Trigger()
    {
        Skill3ServerRpc();
    }

    public void EnableHitbox()
    {
        if (hitboxObject != null)
            hitboxObject.GetComponent<Collider2D>().enabled = true;
    }

    public void DisableHitbox()
    {
        if (hitboxObject != null)
            hitboxObject.GetComponent<Collider2D>().enabled = false;
    }
    [ServerRpc(RequireOwnership = false)]
    private void Skill1ServerRpc()
    {
        GameObject shootInstance = Instantiate(shootEffect, shootPoint.position, shootPoint.rotation);
        shootInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = shootInstance.GetComponent<HitBox>();
        hb.Initialize(this, 20f, 0f);
        Destroy(shootInstance, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Skill2ServerRpc()
    {
        GameObject healInstance = Instantiate(healEffect, healingPoint.position, healingPoint.rotation);
        healInstance.GetComponent<NetworkObject>().Spawn();
        AddHealth(20f);
        Destroy(healInstance, 0.3f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Skill3ServerRpc()
    {
        Player enemy = GetEnemyPlayer();
        if (enemy == null) return;

        Vector3 enemyPos = enemy.transform.position + new Vector3(0, 200f, 0);
        GameObject debuffInstance = Instantiate(debuffEffect, enemyPos, Quaternion.identity);
        debuffInstance.GetComponent<NetworkObject>().Spawn();

        HitBox hb = debuffInstance.GetComponent<HitBox>();
        hb.Initialize(this, 40f, 0f);

        Destroy(debuffInstance, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeductManaServerRpc(float amount)
    {
        AddMana(-amount);
    }
    [ServerRpc(RequireOwnership = false)]
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
