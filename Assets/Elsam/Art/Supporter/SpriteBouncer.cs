using UnityEngine;
using DG.Tweening;

public class SpriteBouncer : MonoBehaviour
{
    public float moveAmount = 0.5f; // jarak naik turun
    public float duration = 0.5f;   // waktu untuk satu naik atau turun

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        float delay = Random.Range(0f, 1f);
        float dur = Random.Range(0.3f, 0.6f);

        DOVirtual.DelayedCall(delay, () =>
        {
            transform.DOMoveY(startPos.y + moveAmount, dur)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        });
    }
}
