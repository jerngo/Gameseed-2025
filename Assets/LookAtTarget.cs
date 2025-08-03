using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform ball;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LookAtBallYOnly(ball);
    }

    public void LookAtBallYOnly(Transform target)
    {
        Vector3 targetDirection = target.position - transform.position;

        // Hilangkan komponen vertikal (Y) agar hanya melihat kiri-kanan
        targetDirection.y = 0f;

        // Jika vector-nya valid (tidak nol), lakukan rotasi
        if (targetDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

}
