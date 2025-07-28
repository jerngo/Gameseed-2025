using UnityEngine;

public class NPCMovement3D : MonoBehaviour
{
    [Header("Hit Settings")]
    public Transform hitPoint;
    public float hitRadius = 1f;
    public LayerMask ballLayer;
    public Transform[] targetZones;

    public float moveSpeed = 5f;
    public float detectRadius = 10f;
    public Transform modelTransform;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 lastGroundMoveDir = Vector3.zero;

    public bool isGrounded;
    public Transform target; // Bisa bola atau player
    private bool isSmashMode = false;

    public bool isActive = false; // Menandakan NPC aktif

    public float lobForce = 12f;
    public float longForce = 12f;
    public float normalForce = 7f;

    void DoPass()
    {
        Collider[] hitBalls = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        if (hitBalls.Length == 0) return;

        Rigidbody ballRb = hitBalls[0].attachedRigidbody;
        if (ballRb == null) return;

        GameObject targetNPC = FindFirstObjectByType<NPCManager>().GetOtherNPC(this.gameObject);
        if (targetNPC == null) return;

        Vector3 start = ballRb.position;
        Vector3 end = targetNPC.transform.position + Vector3.up * 1.5f;

        float distance = Vector3.Distance(start, end);
        float flightTime = Mathf.Clamp(distance / 3f, 0.6f, 2f);

        Vector3 force = CalculateParabolaVelocity(start, end, flightTime);

        ballRb.linearVelocity = Vector3.zero;
        ballRb.useGravity = true;
        ballRb.AddForce(force, ForceMode.VelocityChange);

        isActive = false;
        FindFirstObjectByType<NPCManager>().OnNPCHit(this);

    }

    Vector3 CalculateParabolaVelocity(Vector3 start, Vector3 end, float time)
    {
        Vector3 distance = end - start;
        Vector3 horizontal = new Vector3(distance.x, 0f, distance.z);
        float verticalDistance = distance.y;
        float horizontalDistance = horizontal.magnitude;

        float vxz = horizontalDistance / time;
        float vy = (verticalDistance + 0.5f * Mathf.Abs(Physics.gravity.y) * time * time) / time;

        Vector3 result = horizontal.normalized * vxz;
        result.y = vy;
        return result;
    }

    public void PrepareSmash()
    {
        isSmashMode = true;
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    void FixedUpdate()
    {
        if (!isActive)
        {
            // Hentikan gerakan total saat tidak aktif, tapi jangan hapus target
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0;
        Vector3 moveDir = direction.normalized;

        if (isGrounded)
        {
            lastGroundMoveDir = moveDir.magnitude > 0.1f ? moveDir : Vector3.zero;
        }
        else
        {
            moveDir = lastGroundMoveDir * 0.2f;
        }

        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, v.y, moveDir.z * moveSpeed);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > 0.1f)
            modelTransform.forward = horizontalVelocity.normalized;

        // Mode smash
        if (isSmashMode && isGrounded && Vector3.Distance(transform.position, target.position) < 1.5f)
        {
            SmashBall();
        }

        if (isGrounded && !isSmashMode && Vector3.Distance(transform.position, target.position) < 1.5f)
        {
            DoPass();
        }

        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        if (movedDistance < 0.05f)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > 1f) // Setelah 1 detik tidak bergerak cukup
            {
                Debug.LogWarning("🚧 NPC stuck, melakukan recovery.");
                transform.position += Vector3.left * 0.2f; // Dorong mundur ringan, bisa diganti arah lain
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    void SmashBall()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 10f, rb.linearVelocity.z); // Lompat

        Rigidbody ballRb = target.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            int zoneIndex = Random.Range(0, targetZones.Length); // NPC pilih zona target
            Vector3 ballPos = target.position;
            Vector3 targetPos = targetZones[zoneIndex].position;
            Vector3 dir = (targetPos - ballPos).normalized;

            // Simulasi input kiri/kanan untuk menentukan jenis pukulan
            float simulatedInput = -1; // -1 = lob, 1 = smash

            float forcePower;
            if (simulatedInput < -0.3f)      // Lob
            {
                forcePower = lobForce;
                dir.y = 1.0f;
                forcePower *= 0.7f;
                Debug.Log("🏐 NPC melakukan LOB!");
            }
            else if (simulatedInput > 0.3f)  // Smash
            {
                forcePower = longForce;
                dir.y = -0.2f;
                forcePower *= 1.7f;
                Debug.Log("💥 NPC melakukan SMASH!");
            }
            else                             // Normal
            {
                forcePower = normalForce;
                dir.y = 0.2f;
                forcePower *= 1.2f;
                Debug.Log("🎯 NPC melakukan pukulan NORMAL!");
            }

            ballRb.linearVelocity = Vector3.zero;
            ballRb.useGravity = true;
            ballRb.AddForce(dir.normalized * forcePower, ForceMode.Impulse);
        }

        isSmashMode = false;
        isActive = false;

        // Setelah smash, aktifkan NPC berikutnya
        FindFirstObjectByType<NPCManager>().OnNPCHit(this);

    }


    void OnCollisionStay(Collision other)
    {
        if (other.contacts.Length > 0)
        {
            ContactPoint contact = other.contacts[0];
            isGrounded = Vector3.Dot(contact.normal, Vector3.up) > 0.5f;
        }
    }

    void OnCollisionExit(Collision other)
    {
        isGrounded = false;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        isSmashMode = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        // Radius deteksi NPC (sudah ada)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        // Tambahan untuk visualize hitPoint
        if (hitPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
        }
    }

}
