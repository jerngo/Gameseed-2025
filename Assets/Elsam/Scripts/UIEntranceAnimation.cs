using UnityEngine;
using DG.Tweening;

public class UIEntranceAnimation : MonoBehaviour
{
    public enum EntranceType
    {
        ScaleIn,
        FadeIn,
        SlideFromLeft,
        SlideFromRight,
        SlideFromTop,
        SlideFromBottom
    }

    [Header("Entrance Settings")]
    public EntranceType entranceType = EntranceType.ScaleIn;
    public float duration = 0.5f;
    public float offset = 300f;
    public Ease ease = Ease.OutBack;

    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private void Awake()
    {
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;

        // Optional: add CanvasGroup for fading
        if (entranceType == EntranceType.FadeIn)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        PlayEntrance();
    }

    public void PlayEntrance()
    {
        transform.DOKill(); // cancel previous animations
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;

        switch (entranceType)
        {
            case EntranceType.ScaleIn:
                transform.localScale = Vector3.zero;
                transform.DOScale(originalScale, duration).SetEase(ease);
                break;

            case EntranceType.FadeIn:
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, duration).SetEase(ease);
                break;

            case EntranceType.SlideFromLeft:
                transform.localPosition = originalPosition + Vector3.left * offset;
                transform.DOLocalMove(originalPosition, duration).SetEase(ease);
                break;

            case EntranceType.SlideFromRight:
                transform.localPosition = originalPosition + Vector3.right * offset;
                transform.DOLocalMove(originalPosition, duration).SetEase(ease);
                break;

            case EntranceType.SlideFromTop:
                transform.localPosition = originalPosition + Vector3.up * offset;
                transform.DOLocalMove(originalPosition, duration).SetEase(ease);
                break;

            case EntranceType.SlideFromBottom:
                transform.localPosition = originalPosition + Vector3.down * offset;
                transform.DOLocalMove(originalPosition, duration).SetEase(ease);
                break;
        }
    }
}
