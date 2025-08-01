using UnityEngine;
using UnityEngine.UI;

public class UIScrollingBackground : MonoBehaviour
{
    public RawImage image;
    public Vector2 scrollSpeed = new Vector2(0.1f, 0f); // arah horizontal

    void Update()
    {
        image.uvRect = new Rect(image.uvRect.position + scrollSpeed * Time.deltaTime, image.uvRect.size);
    }
}
