using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ApolinarioAttack : Player
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerController pc;
    private HitBox hitbox;
    private float lastAttackTime = 0f; 
    private float Cooldown = 1f;

    private float skill1ManaCost = 20;
    private float skill2ManaCost = 30;
    private float skill3ManaCost = 60;

    [SerializeField] private Transform windPoint;
    [SerializeField] private Transform tornadoPoint;
    [SerializeField] private Transform dashPoint;

    [SerializeField] private GameObject Wind;
    [SerializeField] private GameObject Tornado;
    [SerializeField] private GameObject TimeFx;
    [SerializeField] private GameObject DashFx;

    [SerializeField] private AudioClip sfxNormalAttack;
    [SerializeField] private AudioClip sfxSkill1;
    [SerializeField] private AudioClip sfxSkill2;
    [SerializeField] private AudioClip sfxSkill3;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        pc = GetComponent<PlayerController>();
        anim.applyRootMotion = false;
    }
    public void PlaySound(string soundName)
    {
        PlaySoundServerRpc(soundName);
    }
    public override void NormalAttack()
    {
        if(!IsOwner) return;
        if (Time.time - lastAttackTime < Cooldown) return;
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");

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
        anim.applyRootMotion = true;
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
    public void OnWindTrigger()
    {
        if(!IsServer) return;
        OnNormalAttackServerRpc();
    }
    public void OnTornadoTrigger()
    {
        if (!IsServer) return;
        OnTornadoServerRpc();
    }
    public void OnDashTrigger()
    {
        if (!IsServer) return;
        OnDashTriggerServerRpc();
    }
    public void OnTimeFxTrigger()
    {
        if (!IsServer) return;
        OnTimeFxServerRpc();
    }
    public void EndDash()
    {
        Debug.Log("APOLINARIO END DASH");
        anim.applyRootMotion = false;
    }
    private void OnAnimatorMove()
    {
        if (anim != null && rb != null && anim.applyRootMotion)
        {
            Vector2 delta = anim.deltaPosition;
            rb.MovePosition(rb.position + delta);
        }
    }
    [ServerRpc]
    private void OnNormalAttackServerRpc()
    {
        GameObject bulletInstance = Instantiate(Wind, windPoint.position, windPoint.rotation);
        bulletInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = bulletInstance.GetComponent<HitBox>();
        hb.Initialize(this, 10f, 15f);
        Destroy(bulletInstance, 0.3f);
    }
    [ServerRpc]
    private void OnTornadoServerRpc()
    {
        GameObject laserInstance = Instantiate(Tornado, tornadoPoint.position, tornadoPoint.rotation);
        laserInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = laserInstance.GetComponent<HitBox>();
        hb.Initialize(this, 20f, 0f);
        Destroy(laserInstance, 1f);
    }
    [ServerRpc]
    private void OnDashTriggerServerRpc()
    {
        GameObject slashInstance = Instantiate(DashFx, dashPoint.position, dashPoint.rotation);
        slashInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = slashInstance.GetComponent<HitBox>();
        hb.Initialize(this, 30f, 0f);
        Destroy(slashInstance, 1f);
    }
    [ServerRpc]
    private void OnTimeFxServerRpc()
    {
        Player enemy = GetEnemyPlayer();
        if (enemy == null) return;

        Vector3 enemyPos = enemy.transform.position + new Vector3(0, 200f, 0); // Adjust height if needed
        GameObject thunderInstance = Instantiate(TimeFx, enemyPos, Quaternion.identity);
        thunderInstance.GetComponent<NetworkObject>().Spawn();
        HitBox hb = thunderInstance.GetComponent<HitBox>();
        hb.Initialize(this, 40f, 0f);
        Destroy(thunderInstance, 1f);
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
