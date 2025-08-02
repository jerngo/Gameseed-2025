using UnityEngine;
using DG.Tweening;

public class UIEntranceAnimation : MonoBehaviour
{
    public enum EntranceType
    {
        ScaleIn,
        FadeIn
    }

    [Header("Entrance Settings")]
    public EntranceType entranceType = EntranceType.ScaleIn;
    public float duration = 0.5f;
    public Ease ease = Ease.OutBack;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;

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
        transform.DOKill(); // Stop ongoing tweens

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
        }
    }
}
