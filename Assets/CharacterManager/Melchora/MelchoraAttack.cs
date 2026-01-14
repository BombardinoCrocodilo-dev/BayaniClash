using Unity.Netcode;
using UnityEngine;

public class MelchoraAttack : Player
{
    private Animator anim;
    private HitBox hitbox;
    private float lastAttackTime = 0f;
    private float Cooldown = 1f;

    private float skill1ManaCost = 20f;
    private float skill2ManaCost = 35f;
    private float skill3ManaCost = 60f;

    [SerializeField] private GameObject hitboxObject;
    [SerializeField] private Transform HealingPoint;
    [SerializeField] private GameObject HealingEffect;
    [SerializeField] private Transform WavePoint;
    [SerializeField] private GameObject Wave;
    [SerializeField] private GameObject Explosion;

    [SerializeField] private AudioClip sfxNormalAttack;
    [SerializeField] private AudioClip sfxSkill1;
    [SerializeField] private AudioClip sfxSkill2;
    [SerializeField] private AudioClip sfxSkill3;
    private void Start()
    {
        anim = GetComponent<Animator>();
        hitbox = hitboxObject.GetComponent<HitBox>();
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

    public void OnTriggerWave()
    {
        if (!IsOwner) return;
        Skill1ServerRpc();
    }

    public void onHealTrigger()
    {
        Skill2ServerRpc();
    }

    public void OnExplosionTrigger()
    {
        if (!IsOwner) return;
        Skill3ServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void Skill1ServerRpc()
    {
        GameObject waveInstance = Instantiate(Wave, WavePoint.position, WavePoint.rotation);
        waveInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = waveInstance.GetComponent<HitBox>();
        hb.Initialize(this, 20f, 10f);
        Destroy(waveInstance, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Skill2ServerRpc()
    {
        GameObject healInstance = Instantiate(HealingEffect, HealingPoint.position, HealingPoint.rotation);
        healInstance.GetComponent<NetworkObject>().Spawn();
        AddHealth(20f);
        Destroy(healInstance, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Skill3ServerRpc()
    {
        Player enemy = GetEnemyPlayer();
        if (enemy == null) return;

        Vector3 enemyPos = enemy.transform.position + new Vector3(0, 200f, 0); // Adjust height if needed
        GameObject explosionInstance = Instantiate(Explosion, enemyPos, Quaternion.identity);
        explosionInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = explosionInstance.GetComponent<HitBox>();
        hb.Initialize(this, 40f, 0f);
        Destroy(explosionInstance, 1f);
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
