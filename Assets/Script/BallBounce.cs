using UnityEngine;

public class BallBounce : MonoBehaviour
{
    public float bounceForce = 12f; // Sesuaikan kekuatan mantulan
    public string enemyTag = "enemySide";
    public Vector3 playerAreaDirection = Vector3.left; // Anggap area player berada di kiri

    private Rigidbody rb;

    public string arenaSide;

    PlayerSwitchManager playerswitchManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerswitchManager = FindFirstObjectByType<PlayerSwitchManager>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(enemyTag))
        {
            Vector3 bounceDir = (Vector3.up + playerAreaDirection).normalized;
            rb.linearVelocity = bounceDir * bounceForce;
            arenaSide = "Right";
            playerswitchManager.EnableAllControl();
        }
    }
}
