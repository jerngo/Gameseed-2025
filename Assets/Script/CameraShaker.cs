using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker shaker; // Static instance yang bisa diakses dari mana saja

    public CinemachineCamera vCam;
    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeRoutine;

    void Awake()
    {
        shaker = this; // Set instance statis
    }

    void Start()
    {
        // Ambil modul noise dari extension Cinemachine terbaru (pastikan sudah di-assign)
        if (vCam != null)
        {
            noise = vCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        if (noise == null)
        {
            Debug.LogError("Noise module not found! Make sure your vCam has CinemachineBasicMultiChannelPerlin as an Extension.");
        }
    }

    public void ShakeCamera(float amplitude, float frequency, float duration)
    {
        if (noise == null) return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake(amplitude, frequency, duration));
    }

    private IEnumerator DoShake(float amplitude, float frequency, float duration)
    {
        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }
}
