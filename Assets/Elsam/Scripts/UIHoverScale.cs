using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum HoverAnimationType
    {
        Default,
        Punch,
        Elastic,
        Bounce
    }

    [Header("Animation Settings")]
    public HoverAnimationType animationType = HoverAnimationType.Default;
    public float hoverScale = 1.1f;
    public float duration = 0.2f;

    private Vector3 originalScale;
    private Tween currentTween;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        currentTween?.Kill();

        switch (animationType)
        {
            case HoverAnimationType.Punch:
                transform.localScale = originalScale; // reset scale
                currentTween = transform.DOPunchScale(Vector3.one * 0.1f, duration, 10, 1f);
                break;

            case HoverAnimationType.Elastic:
                currentTween = transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutElastic);
                break;

            case HoverAnimationType.Bounce:
                currentTween = transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutBounce);
                break;

            case HoverAnimationType.Default:
            default:
                currentTween = transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutBack);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }
}
