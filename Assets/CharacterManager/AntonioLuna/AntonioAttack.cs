using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AntonioAttack : Player
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


    [SerializeField] private GameObject hitboxObject;
    [SerializeField] private Transform bulletPoint;
    [SerializeField] private Transform laserPoint;
    [SerializeField] private Transform slashPoint;

    [SerializeField] private GameObject Bullet;
    [SerializeField] private GameObject Laser;
    [SerializeField] private GameObject swordSlash;


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
        if (Time.time - lastAttackTime < Cooldown)return;

        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
        if (hitbox != null)
            hitbox.Initialize(this, 10f, 15f);
        anim.SetTrigger("Attack");
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
    public void OnGunTrigger()
    {
        if (!IsOwner) return;
        OnGunTriggerServerRpc();
    }
    public void OnLaserTrigger()
    {
        if (!IsOwner) return;
        OnLaserTriggerServerRpc();
    }
    public void OnSwordSlashTrigger()
    {
        if (!IsOwner) return;
        OnSwordTriggerServerRpc();
    }
    public void EndDash()
    {
        anim.applyRootMotion = false;
    }
    public void DisableHitbox()
    {
        if (hitboxObject != null)
            hitboxObject.GetComponent<Collider2D>().enabled = false;
    }
    public void EnableHitbox()
    {
        if (hitboxObject != null)
            hitboxObject.GetComponent<Collider2D>().enabled = true;
    }
    [ServerRpc]
    private void OnGunTriggerServerRpc()
    {
        GameObject bulletInstance = Instantiate(Bullet, bulletPoint.position, bulletPoint.rotation);
        bulletInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = bulletInstance.GetComponent<HitBox>();
        hb.Initialize(this, 30f, 0f);
        Destroy(bulletInstance, 1f);
    }
    [ServerRpc]
    private void OnLaserTriggerServerRpc()
    {
        GameObject laserInstance = Instantiate(Laser, laserPoint.position, laserPoint.rotation);
        laserInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = laserInstance.GetComponent<HitBox>();
        hb.Initialize(this, 40f, 0f);
        Destroy(laserInstance, 1f);
    }
    [ServerRpc]
    private void OnSwordTriggerServerRpc()
    {
        GameObject slashInstance = Instantiate(swordSlash, slashPoint.position, slashPoint.rotation);
        slashInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = slashInstance.GetComponent<HitBox>();
        hb.Initialize(this, 20f, 0f);
        Destroy(slashInstance, 1f);
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
