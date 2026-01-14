   using System.Collections;
    using Unity.Netcode;
    using UnityEngine;

    public class LapuLapuAttacks : Player
    {
        [SerializeField] private AudioClip sfxNormalAttack;
        [SerializeField] private AudioClip sfxSkill1;
        [SerializeField] private AudioClip sfxSkill2;
        [SerializeField] private AudioClip sfxSkill3;

        [SerializeField] private GameObject hitboxObject;
        private Animator anim;
        private Rigidbody2D rb;
        private HitBox hitbox;
        private float normalAttackDuration = 0.4167f;
        private float lastAttackTime = 0f; // Track last attack time
        private float Cooldown = 1f; // 1 second cooldown

        private float skill1ManaCost = 20;
        private float skill2ManaCost = 30;
        private float skill3ManaCost = 60;

        [SerializeField] private GameObject Thunder;

        private void Start()
        {
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            hitbox = hitboxObject.GetComponent<HitBox>();
            anim.applyRootMotion = false; // default: off
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
            if(!IsOwner) return;
            if (Time.time - lastAttackTime < Cooldown) return;
            lastAttackTime = Time.time;
            anim.SetBool("Attack", true);
            if (hitbox != null)
                hitbox.Initialize(this, 10f, 15f);
            StartCoroutine(ResetAttack());
        }

        public override void Skill1()
        {
            if (!IsOwner) return;
            if (currentMana.Value < skill1ManaCost) return;
            if (Time.time - lastAttackTime < Cooldown) return;
            lastAttackTime = Time.time;
            DeductManaServerRpc(skill1ManaCost);
            anim.applyRootMotion = true;
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
            if (hitbox != null)
            hitbox.Initialize(this, 30f, 0f);
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
        public void OnThunderTrigger()
        {
            if (!IsOwner) return;
            OnThunderTriggerServerRpc();
        }

        private IEnumerator ResetAttack()
        {
            yield return new WaitForSeconds(normalAttackDuration);
            anim.SetBool("Attack", false);
        }

        private void OnAnimatorMove()
        {
            if (anim != null && rb != null && anim.applyRootMotion)
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
        [ServerRpc(RequireOwnership = false)]
        private void OnThunderTriggerServerRpc()
        {
            Player enemy = GetEnemyPlayer();
            if (enemy == null) return;

            Vector3 enemyPos = enemy.transform.position + new Vector3(0, 200f, 0); // Adjust height if needed
            GameObject thunderInstance = Instantiate(Thunder, enemyPos, Quaternion.identity);
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
