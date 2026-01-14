using UnityEngine;

public class WaveMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }
}
