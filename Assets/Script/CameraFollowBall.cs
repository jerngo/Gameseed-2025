using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowWithLimit : MonoBehaviour
{
    public CinemachineCamera virtualCam;
    public Transform ballTransform;
    public Transform cameraTarget; // Ini adalah Follow target untuk Cinemachine

    [Header("Limit Kamera")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = 1f;
    public float maxY = 5f;

    [Header("FOV Tetap")]
    public float fixedFOV = 40f;

    void Start()
    {
        // Set FOV satu kali
        if (virtualCam != null)
            virtualCam.Lens.FieldOfView = fixedFOV;
    }

    void LateUpdate()
    {
        if (ballTransform == null || cameraTarget == null) return;

        Vector3 targetPos = ballTransform.position;

        // Hanya perbolehkan gerakan dalam batas X dan Y
        float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

        // Posisi Z dari cameraTarget tidak diubah agar kamera tetap konsisten
        cameraTarget.position = new Vector3(clampedX, clampedY, cameraTarget.position.z);
    }
}
