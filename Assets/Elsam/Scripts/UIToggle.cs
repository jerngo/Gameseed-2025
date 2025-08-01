using UnityEngine;
using DG.Tweening;

public class UIToggle : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject toShow;
    public GameObject toHide;

    [Header("Animation")]
    public float duration = 0.3f;
    public Ease easeIn = Ease.OutBack;
    public Ease easeOut = Ease.InBack;

    [Header("Options")]
    public bool swapShowHide = false;

    public void Toggle()
    {
        GameObject firstHide = swapShowHide ? toShow : toHide;
        GameObject thenShow = swapShowHide ? toHide : toShow;

        if (firstHide != null && firstHide.activeSelf)
        {
            firstHide.transform.DOKill();

            // Pastikan starting scale-nya benar (kalau sebelumnya diubah-ubah)
            if (firstHide.transform.localScale == Vector3.zero)
                firstHide.transform.localScale = Vector3.one;

            firstHide.transform.DOScale(0f, duration).SetEase(easeOut)
                .OnComplete(() =>
                {
                    firstHide.SetActive(false);

                    // Aktifkan dan animasikan yang ingin ditampilkan
                    if (thenShow != null)
                    {
                        thenShow.SetActive(true);
                        thenShow.transform.DOKill();
                        thenShow.transform.localScale = Vector3.zero;
                        thenShow.transform.DOScale(1f, duration).SetEase(easeIn);
                    }
                });
        }
        else
        {
            if (thenShow != null)
            {
                thenShow.SetActive(true);
                thenShow.transform.DOKill();
                thenShow.transform.localScale = Vector3.zero;
                thenShow.transform.DOScale(1f, duration).SetEase(easeIn);
            }
        }
    }

    public void SetSwap(bool value)
    {
        swapShowHide = value;
    }

    public void ToggleWithSwap(bool value)
    {
        swapShowHide = value;
        Toggle();
    }
}
