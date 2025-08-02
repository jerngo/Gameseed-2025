using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SupporterWiggle : MonoBehaviour
{
    public float moveAmount = 20f;
    public float duration = 0.5f;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + moveAmount, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
