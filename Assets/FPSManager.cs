using UnityEngine;

public class FPSManager : MonoBehaviour
{
    void Awake()
    {
        // Hanya ada satu instance
        if (FindObjectsByType<FPSManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // Biar tidak hilang saat pindah scene
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}
