using UnityEngine;
using DG.Tweening;

public class IdleFloatMotion : MonoBehaviour
{
    public enum MotionType
    {
        UpDown,
        LeftRight
    }

    public enum WaveMode
    {
        None,           // Gerak DoTween biasa
        Sinusoidal,     // Gelombang waktu dengan offset acak
        RandomDelay,    // DoTween dengan delay acak
        WaveByPosition  // Bergelombang berdasarkan posisi X
    }

    [Header("Settings")]
    public MotionType motionType = MotionType.UpDown;
    public WaveMode waveMode = WaveMode.None;

    [Tooltip("Jarak gerak dalam pixel/unit")]
    public float distance = 10f;

    [Tooltip("Durasi untuk bolak-balik atau 1 siklus gelombang")]
    public float duration = 1f;

    public Ease ease = Ease.InOutSine;

    [Header("Wave By Position Settings")]
    public float waveLength = 50f;     // Jarak antar puncak jika pakai WaveByPosition

    private Vector3 originalPosition;
    private float randomOffset;

    void Start()
    {
        originalPosition = transform.localPosition;
        randomOffset = Random.Range(0f, Mathf.PI * 2f);

        if (waveMode == WaveMode.None)
        {
            Vector3 targetOffset = motionType == MotionType.UpDown ?
                Vector3.up * distance : Vector3.right * distance;

            transform.DOLocalMove(originalPosition + targetOffset, duration)
                     .SetEase(ease)
                     .SetLoops(-1, LoopType.Yoyo);
        }
        else if (waveMode == WaveMode.RandomDelay)
        {
            float initialDelay = Random.Range(0f, duration);
            Vector3 targetOffset = motionType == MotionType.UpDown ?
                Vector3.up * distance : Vector3.right * distance;

            transform.DOLocalMove(originalPosition + targetOffset, duration)
                     .SetEase(ease)
                     .SetLoops(-1, LoopType.Yoyo)
                     .SetDelay(initialDelay);
        }
    }

    void Update()
    {
        if (waveMode == WaveMode.Sinusoidal)
        {
            float wave = Mathf.Sin(Time.time * Mathf.PI * 2 / duration + randomOffset) * distance;
            Vector3 offset = motionType == MotionType.UpDown ? Vector3.up * wave : Vector3.right * wave;
            transform.localPosition = originalPosition + offset;
        }
        else if (waveMode == WaveMode.WaveByPosition)
        {
            float wave = Mathf.Sin((Time.time * Mathf.PI * 2 / duration) + (originalPosition.x / waveLength)) * distance;
            Vector3 offset = motionType == MotionType.UpDown ? Vector3.up * wave : Vector3.right * wave;
            transform.localPosition = originalPosition + offset;
        }
    }
}
