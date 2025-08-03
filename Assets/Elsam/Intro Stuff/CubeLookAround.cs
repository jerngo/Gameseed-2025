using UnityEngine;
using DG.Tweening;

public class CubeLookAround : MonoBehaviour
{
    [Header("Settings")]
    public float minAngle = -60f;
    public float maxAngle = 60f;
    public float minDuration = 0.5f;
    public float maxDuration = 1.5f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private bool isLooking = false;
    private Vector3 originalEuler;

    void Start()
    {
        originalEuler = transform.localEulerAngles;
        StartCoroutine(LookAroundRoutine());
    }

    System.Collections.IEnumerator LookAroundRoutine()
    {
        while (true)
        {
            if (!isLooking)
            {
                isLooking = true;

                float randomAngle = Random.Range(minAngle, maxAngle);
                float duration = Random.Range(minDuration, maxDuration);
                float waitTime = Random.Range(minWaitTime, maxWaitTime);

                Vector3 targetEuler = new Vector3(
                    originalEuler.x,
                    originalEuler.y + randomAngle,
                    originalEuler.z
                );

                transform.DOLocalRotate(targetEuler, duration).SetEase(Ease.InOutSine);

                yield return new WaitForSeconds(duration + waitTime);

                isLooking = false;
            }

            yield return null;
        }
    }
}
