using Unity.Netcode;
using UnityEngine;

public class HitBox : NetworkBehaviour
{
    public Player owner;
    private float damage;
    private float mana;

    public void Initialize(Player owner, float damage, float mana)
    {
        this.owner = owner;
        this.damage = damage;
        this.mana = mana;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        var hurtBox = other.GetComponent<HurtBox>();
        if (hurtBox != null && hurtBox.owner != owner)
        {
            try
            {
                hurtBox.owner.RequestTakeDamageServerRpc(damage);
                if (hurtBox.owner.isBlock == false)
                {
                    hurtBox.owner.PlayHitAnimationClientRpc();
                    hurtBox.owner.LockMovementClientRpc(0.3f);
                }
                owner.RequestAddManaServerRpc(mana);

                Debug.Log($"[HitBox] {owner.name} hit {hurtBox.owner.name}. Target isBlocking = {hurtBox.owner.isBlocking.Value}");
            }
            catch (System.NullReferenceException)
            {

            }
        }
    }
}
