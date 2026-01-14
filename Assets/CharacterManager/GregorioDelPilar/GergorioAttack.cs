using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GergorioAttack : Player
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

    [SerializeField] private Transform shieldPoint;
    [SerializeField] private Transform slashPoint;
    [SerializeField] private Transform skill3SlashPoint;


    [SerializeField] private GameObject Shield;
    [SerializeField] private GameObject Slash;
    [SerializeField] private GameObject skill3Slash;

    [SerializeField] private GameObject hitboxObject;

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
        StartCoroutine(onShield());
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
    public void OnSlashTrigger()
    {
        if (!IsOwner) return; 
        SlashServerRpc();
    }
    public void OnShieldTrigger()
    {
        if (!IsOwner) return;
        OnShieldServerRpc();
    }
    public void OnSkill3SlashTrigger()
    {
        if (!IsOwner) return;
        Skill3SlashServerRpc();
    }
    private IEnumerator onShield()
    {
        gregodioShield = true;
        yield return new WaitForSeconds(5f);
        gregodioShield = false;
    }
    [ServerRpc(RequireOwnership = false)]
    private void SlashServerRpc()
    {
        GameObject slashInstance = Instantiate(Slash, slashPoint.position, slashPoint.rotation);
        slashInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = slashInstance.GetComponent<HitBox>();
        hb.Initialize(this, 20f, 10f);
        Destroy(slashInstance, 1f);
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnShieldServerRpc()
    {
        GameObject shieldInstance = Instantiate(Shield, shieldPoint.position, shieldPoint.rotation);
        shieldInstance.GetComponent<NetworkObject>().Spawn();

        // Tell all clients to follow this player
        FollowPlayer follow = shieldInstance.GetComponent<FollowPlayer>();
        follow.target = shieldPoint;  // or transform if you want it to follow the player center
        Destroy(shieldInstance, 5f);
    }
    [ServerRpc(RequireOwnership = false)]
    private void Skill3SlashServerRpc()
    {
        GameObject skill3SlashInstance = Instantiate(skill3Slash, skill3SlashPoint.position, skill3SlashPoint.rotation);
        skill3SlashInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = skill3SlashInstance.GetComponent<HitBox>();
        hb.Initialize(this, 40f, 10f);
        Destroy(skill3SlashInstance, 1f);
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
