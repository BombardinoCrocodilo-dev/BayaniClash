using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GabrielaAttack : Player
{
    private Animator anim;
    private Rigidbody2D rb;
    private HitBox hitBox;
    private float lastAttackTime = 0f; // Track last attack time
    private float Cooldown = 1f; // 1 second cooldown

    private float skill1ManaCost = 20f;
    private float skill2ManaCost = 30f;
    private float skill3ManaCost = 60f;

    [SerializeField] private GameObject hitboxObject;
    [SerializeField] private Transform bulletPoint;
    [SerializeField] private GameObject Bullet;

    [SerializeField] private AudioClip sfxNormalAttack;
    [SerializeField] private AudioClip sfxSkill1;
    [SerializeField] private AudioClip sfxSkill2;
    [SerializeField] private AudioClip sfxSkill3;
    [SerializeField] private AudioClip sfxHorse;
    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        hitBox = hitboxObject.GetComponent<HitBox>();
        anim.applyRootMotion = false;
        if (hitboxObject != null)
            hitboxObject.GetComponent<Collider2D>().enabled = false;
        if (hitBox != null)
        {
            hitBox.owner = this;
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

        if (hitBox != null)
            hitBox.Initialize(this, 10f, 15f);
    }

    public override void Skill1()
    {
        if (!IsOwner) return;
        if (currentMana.Value < skill1ManaCost) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        DeductManaServerRpc(skill1ManaCost);

        anim.applyRootMotion = true;
        if (hitBox != null)
            hitBox.Initialize(this, 20f, 0f);
        anim.SetTrigger("Skill1");
    }

    public void OnBulletTrigger()
    {
        if (!IsOwner) return;
        Skill2ServerRpc();
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

        anim.applyRootMotion = true;
        if (hitBox != null)
            hitBox.Initialize(this, 40f, 0f);
        anim.SetTrigger("Skill3");
    }

    private void OnAnimatorMove()
    {
        if (rb == null || anim == null) return;
        if (anim.applyRootMotion)
        {
            Vector2 delta = anim.deltaPosition;
            rb.MovePosition(rb.position + delta);
        }
    }

    public void EndDash()
    {
        anim.applyRootMotion = false;
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

    [ServerRpc]
    private void Skill2ServerRpc()
    {
        GameObject bulletInstance = Instantiate(Bullet, bulletPoint.position, bulletPoint.rotation);
        bulletInstance.GetComponent<NetworkObject>().Spawn();

        HitBox hb = bulletInstance.GetComponent<HitBox>();
        hb.Initialize(this, 30f, 0f);

        Destroy(bulletInstance, 1f);
    }

    // ServerRpc to deduct mana safely
    [ServerRpc(RequireOwnership = false)]
    private void DeductManaServerRpc(float amount)
    {
        AddMana(-amount); // Uses Player base method to safely deduct mana
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
            case "HorseSound":
                AudioManager.Instance.PlaySFX(sfxHorse);
                break;
        }
    }
}
