using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public AudioSource audioSource;
    //public Slider sliderValue;

    // Nilai volume antara 0.0f (diam) sampai 1.0f (maksimal)
    public void SetVolume(float amount)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(amount);
        }
    }

    // Fungsi untuk mengurangi volume sedikit demi sedikit
    public void DecreaseVolume(float amount)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(audioSource.volume - amount);
        }
    }
}
