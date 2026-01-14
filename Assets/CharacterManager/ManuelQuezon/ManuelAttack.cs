using System.Net;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
public class ManuelAttack : Player
{
    private Animator anim;
    private HitBox hitbox;
    private float lastAttackTime = 0f;
    private float Cooldown = 1f;

    private float skill1ManaCost = 20;
    private float skill2ManaCost = 30;
    private float skill3ManaCost = 60;

    [SerializeField] private GameObject hitboxObject;
    [SerializeField] private Transform explosionPoint;
    [SerializeField] private Transform[] fireBallPoint;

    [SerializeField] private GameObject Explosion;
    [SerializeField] private GameObject fireBall;

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
    }
    public override void Skill1()
    {
        if (!IsOwner) return;
        if (currentMana.Value < skill1ManaCost) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        anim.SetTrigger("Skill1");
        DeductManaServerRpc(skill1ManaCost);
    }
    public override void Skill2()
    {
        if (!IsOwner) return;
        if (currentMana.Value < skill2ManaCost) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        anim.SetTrigger("Skill2");
        DeductManaServerRpc(skill2ManaCost);

    }
    public override void Skill3()
    {
        if (!IsOwner) return;
        if (currentMana.Value < skill3ManaCost) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        anim.SetTrigger("Skill3");
        DeductManaServerRpc(skill3ManaCost);
    }
    public void OnExplosionTrigger()
    {
        OnExplosionServerRpc();
    }
    public void OnFireBall1Trigger()
    {
        OnFireBall1ServerRpc();
    }
    public void OnFireBall2Trigger()
    {
        OnFireBall2ServerRpc();
    }
    public void OnFireBall3Trigger()
    {
        OnFireBall3ServerRpc();
    }
    public void OnFireBall4Trigger()
    {
        OnFireBall4ServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnExplosionServerRpc()
    {
        GameObject explosionInstance = Instantiate(Explosion, explosionPoint.position, explosionPoint.rotation);
        explosionInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = explosionInstance.GetComponent<HitBox>();
        hb.Initialize(this, 10f, 15f);
        Destroy(explosionInstance, 0.3f);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnFireBall1ServerRpc()
    {
        GameObject fireBallInstance = Instantiate(fireBall, fireBallPoint[0].position, fireBallPoint[0].rotation);
        fireBallInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = fireBallInstance.GetComponent<HitBox>();
        hb.Initialize(this, 20f, 0f);
        Destroy(fireBallInstance, 1f);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnFireBall2ServerRpc()
    {
        GameObject fireBallInstance = Instantiate(fireBall, fireBallPoint[1].position, fireBallPoint[1].rotation);
        fireBallInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = fireBallInstance.GetComponent<HitBox>();
        hb.Initialize(this, 14f, 0f);
        Destroy(fireBallInstance, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnFireBall3ServerRpc()
    {
        GameObject fireBallInstance = Instantiate(fireBall, fireBallPoint[2].position, fireBallPoint[2].rotation);
        fireBallInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = fireBallInstance.GetComponent<HitBox>();
        hb.Initialize(this, 14f, 0f);
        Destroy(fireBallInstance, 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnFireBall4ServerRpc()
    {
        GameObject fireBallInstance = Instantiate(fireBall, fireBallPoint[3].position, fireBallPoint[3].rotation);
        fireBallInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = fireBallInstance.GetComponent<HitBox>();
        hb.Initialize(this, 14f, 0f);
        Destroy(fireBallInstance, 1f);
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
