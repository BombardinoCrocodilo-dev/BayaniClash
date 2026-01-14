using UnityEngine;

[System.Serializable]
public struct Part
{
    public SpriteRenderer renderer;
    public int offset; // 0 = main body, negative = behind, positive = in front
}

public class SpriteSorter : MonoBehaviour
{
    [SerializeField] private Part[] parts;

    void LateUpdate()
    {
        // Base sorting order from Y position
        int baseOrder = Mathf.RoundToInt(-transform.position.y * 100);

        foreach (var part in parts)
        {
            part.renderer.sortingOrder = baseOrder + part.offset;
        }
    }
}
