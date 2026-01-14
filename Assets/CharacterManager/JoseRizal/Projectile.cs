using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    private float lifeTime = 5f;

    private float timer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        // destroy after some time
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject); // safer than SetActive(false) for projectiles
        }
    }
}
