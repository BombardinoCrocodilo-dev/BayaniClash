using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;     // The player to follow
    public Vector3 offset;       // Optional offset (e.g. new Vector3(0, 1, 0))

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
