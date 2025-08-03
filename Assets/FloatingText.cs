using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI captionText;
    public float floatSpeed = 1f;      // kecepatan naik-turun
    public float floatAmplitude = 10f; // seberapa tinggi gerakannya (dalam pixel UI)

    private Vector3 startPos;

    void Start()
    {
        if (captionText != null)
        {
            startPos = captionText.rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        if (captionText != null)
        {
            float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            captionText.rectTransform.anchoredPosition = startPos + new Vector3(0f, newY, 0f);
        }
    }
}
