using UnityEngine;
using DG.Tweening;

public class MegaphonePulse : MonoBehaviour
{
    [Header("Scale Settings")]
    public float scaleMultiplier = 1.2f;   // Seberapa besar pembesaran
    public float duration = 0.3f;          // Durasi animasi ke besar/kecil
    public Ease easeType = Ease.InOutSine;

    private Vector3 originalScale;
    private Tween pulseTween;

    void Start()
    {
        originalScale = transform.localScale;

        pulseTween = transform.DOScale(originalScale * scaleMultiplier, duration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnDestroy()
    {
        if (pulseTween != null) pulseTween.Kill();
    }
}
