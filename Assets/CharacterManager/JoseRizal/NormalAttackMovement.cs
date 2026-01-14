using UnityEngine;

public class NormalAttackMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }
}
