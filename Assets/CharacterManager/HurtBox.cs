using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public Player owner; // assign in prefab or in Awake()

    private void Awake()
    {
        // If not assigned manually, try to find Player in parent
        if (owner == null)
            owner = GetComponentInParent<Player>();
    }
}
