using UnityEngine;
public class ScrollView : MonoBehaviour
{
    public RectTransform viewport;
    public float scaleFactor = 0.5f;
    public float focusRange = 200f;

    private void Update()
    {
        ButtonScroll();
    }

    private void ButtonScroll()
    {
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect == null) continue;

            // Convert button position to Viewport local space
            Vector3 buttonPos = childRect.position;
            Vector3 viewportPos = viewport.InverseTransformPoint(buttonPos);

            // Calculate distance from bottom of viewport
            float bottomY = -viewport.rect.height / 2f;
            float distance = Mathf.Clamp01((viewportPos.y - bottomY) / focusRange);

            // Scale decreases as the button moves up (away from bottom)
            float scale = Mathf.Lerp(1f, scaleFactor, distance);
            child.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
