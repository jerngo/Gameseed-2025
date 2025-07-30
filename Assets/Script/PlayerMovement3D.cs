using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement3D : MonoBehaviour
{
    [Header("Auto Jump Tracking")]
    public float autoMoveSpeed = 5f;
    public float maxChaseDuration = 1.5f;

    private bool isAutoChasingBall = false;
    private Vector3 targetBallXZPos;
    private float chaseTimer = 0f;

    [Header("Model")]
    public Transform modelTransform;

    [Header("Smash Settings")]
    public float smashHeightThreshold = 7.5f;
    public float smashSpeed = 25f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float dashForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Hit Settings")]
    public Transform hitPoint;
    public float hitRadius = 1f;
    public float hitForce = 5f;
    public float lobForce = 5f;
    public float normalForce = 7f;
    public float longForce = 12f;

    [Header("Target Zones (9 Grid)")]
    public Transform[] ownZones = new Transform[9];
    public Transform[] enemyZones = new Transform[9];

    public LayerMask ballLayer;

    private PlayerControls controls;
    private Rigidbody rb;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool dashPressed;
    private bool hitPressed;

    public Vector3 lastGroundMoveDir = Vector3.zero;

    private bool isDashing = false;
    private Vector3 dashDirection;
    private float dashDuration = 0.4f;
    private float dashTimer = 0f;

    private bool hasRescueHit = false;

    public float passHeight = 1.5f;
    public float passSpeedFactor = 3f;

    public bool isControlled;

    private bool hasHitDuringJump = false;
    private bool isJumping = false;

    private bool pendingPass = false;
    private Transform targetPassBall = null;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;

        controls.Player.Jump.performed += _ => jumpPressed = true;
        controls.Player.Dash.performed += _ => dashPressed = true;
        controls.Player.Hit.performed += _ => hitPressed = true;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start() => rb = GetComponent<Rigidbody>();

    void StartAutoChaseToBall()
    {
        Collider[] balls = Physics.OverlapSphere(transform.position, 30f, ballLayer);
        if (balls.Length > 0)
        {
            Transform ball = balls[0].transform;

            // Ambil posisi XZ bola sebagai target
            targetBallXZPos = new Vector3(ball.position.x, transform.position.y, ball.position.z);
            isAutoChasingBall = true;
            chaseTimer = maxChaseDuration;

            Debug.Log("🔵 Auto kejar bola aktif: " + targetBallXZPos);
        }
    }


    void Update()
    {
        if (!isControlled) return;

        if (IsGrounded())
        {
            if (hitPressed)
            {
                hitPressed = false;

                Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
                if (hits.Length > 0)
                {
                    Debug.Log("Pukul langsung saat di tanah");
                    HitBallToOtherSide();
                }
                else
                {
                    Debug.Log("Lompat karena tidak ada bola");
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                    StartAutoChaseToBall();
                }
            }

            if (dashPressed)
            {
                dashPressed = false;
                TryPassToBall();
            }
        }

        if (isJumping && rb.linearVelocity.y < -0.1f)
        {
            hasHitDuringJump = true;
        }

        if (IsGrounded())
        {
            isJumping = false;
            hasHitDuringJump = false;
        }

        if (IsGrounded() && isAutoChasingBall)
        {
            isAutoChasingBall = false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Ball")) return;

        if (!IsGrounded() && isJumping && !hasHitDuringJump)
        {
            Debug.Log("Pukul bola di udara");
            HitBallToOtherSide();
            hasHitDuringJump = true;
        }
    }

    void HitBallToOtherSide()
    {
        Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        if (hits.Length == 0) return;

        Transform ball = hits[0].transform;
        int zoneIndex = GetZoneIndexFromInput(moveInput);
        if (zoneIndex < 0 || zoneIndex >= enemyZones.Length || enemyZones[zoneIndex] == null) return;

        Vector3 target = enemyZones[zoneIndex].position;
        LaunchBallToTarget(ball, target, hitForce);
    }

    void SmashBall(Transform ball)
    {
        int zoneIndex = GetZoneIndexFromInput(moveInput);
        if (zoneIndex < 0 || zoneIndex >= enemyZones.Length || enemyZones[zoneIndex] == null) return;

        Vector3 target = enemyZones[zoneIndex].position;

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.useGravity = true;

        // Arahkan langsung, menukik cepat
        Vector3 direction = (target - ball.position).normalized;
        direction.y = -0.3f; // bikin menukik, sesuaikan kalau terlalu tajam
        direction.Normalize();

        ballRb.linearVelocity = direction * smashSpeed;

        Debug.DrawLine(ball.position, target, Color.yellow, 2f);
        Debug.Log("🔥 Smash! Ke zona " + zoneIndex);
    }


    void PassBallInOwnArena()
    {
        Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        if (hits.Length == 0) return;

        Transform ball = hits[0].transform;
        int zoneIndex = GetZoneIndexFromInput(moveInput);
        if (zoneIndex < 0 || zoneIndex >= ownZones.Length || ownZones[zoneIndex] == null) return;

        Vector3 target = ownZones[zoneIndex].position;
        LaunchBallToTarget(ball, target, lobForce);
    }

    public float minDashDistance = 1;
    void TryPassToBall()
    {

        Collider[] balls = Physics.OverlapSphere(transform.position, 10f, ballLayer);
        if (balls.Length == 0)
        {
            // Tidak ada bola dekat, dash biasa
            Vector3 dashDir = new Vector3(-moveInput.x, 0, -moveInput.y).normalized;
            if (dashDir.magnitude < 0.1f) dashDir = transform.forward;

            rb.AddForce(dashDir * dashForce, ForceMode.VelocityChange);
            Debug.Log("🟡 Dash biasa (tanpa bola)");
            return;
        }

        Transform ball = balls[0].transform;
        Vector3 toBall = ball.position - transform.position;
        float distXZ = new Vector2(toBall.x, toBall.z).magnitude;
        float verticalOffset = ball.position.y - transform.position.y;

        if (IsGrounded())
        {
            // Jika bola dekat dan cukup rendah, langsung pass
            if (Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer).Length > 0)
            {
                Debug.Log("✅ Pass langsung saat di tanah");
                PassBallInOwnArena();
            }
            // Jika bola tinggi → lompat + siapkan pass
            else if (verticalOffset > 1.2f && distXZ < 1)
            {
                Debug.Log("⬆️ Bola tinggi, lompat & siapkan pass");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                StartAutoChaseToBall();

                pendingPass = true;
                targetPassBall = ball;
            }
            // Jika bola agak jauh, dash ke arahnya
            else if (distXZ < minDashDistance)
            {
                Vector3 dashDir = new Vector3(toBall.x, 0, toBall.z).normalized;
                rb.AddForce(dashDir * dashForce, ForceMode.VelocityChange);
                Debug.Log("🏃 Dash ke arah bola");
            }
            else
            {
                Debug.Log("❓ Tidak dalam kondisi pass/dash yang cocok");
            }
        }
    }


    void LaunchBallToTarget(Transform ball, Vector3 target, float baseArcHeight)
    {
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.useGravity = true;

        float horizontalDistance = Vector3.Distance(new Vector3(ball.position.x, 0, ball.position.z), new Vector3(target.x, 0, target.z));
        float adjustedArcHeight = Mathf.Clamp(baseArcHeight + (horizontalDistance * 0.25f), baseArcHeight, 25f);

        Vector3 velocity = CalculateLaunchVelocity(ball.position, target, adjustedArcHeight);
        rb.linearVelocity = velocity;

        Debug.DrawLine(ball.position, target, Color.red, 2f);
        Debug.Log($"Ball launched to {target} with velocity {velocity}, arcHeight: {adjustedArcHeight}");
    }

    Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float arcHeight)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        Vector3 displacementXZ = new Vector3(end.x - start.x, 0, end.z - start.z);
        float horizontalDistance = displacementXZ.magnitude;
        float deltaY = end.y - start.y;

        float timeToPeak = Mathf.Sqrt(2 * arcHeight / gravity);
        float timeFromPeak = Mathf.Sqrt(2 * Mathf.Max(0.1f, arcHeight - deltaY) / gravity);
        float totalTime = timeToPeak + timeFromPeak;
        if (totalTime <= 0.01f) totalTime = 0.01f;

        Vector3 velocityXZ = displacementXZ / totalTime;
        float velocityY = Mathf.Sqrt(2 * gravity * arcHeight);

        return velocityXZ + (Vector3.up * velocityY);
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void FixedUpdate()
    {
        if (!isControlled) return;

        // Cek apakah sedang melompat
        if (!IsGrounded() && rb.linearVelocity.y > 0.1f && !isJumping)
        {
            isJumping = true;
            Debug.Log("Mulai lompat!");
        }

        // Saat jatuh, anggap sudah tidak bisa pukul lagi
        if (rb.linearVelocity.y < -0.1f && isJumping)
        {
            hasHitDuringJump = true;
        }

        if (IsGrounded())
        {
            isJumping = false;
            hasHitDuringJump = false;
        }

        // Deteksi pukul bola di udara
        // Deteksi pukul bola di udara
        if (!IsGrounded() && isJumping && !hasHitDuringJump)
        {
            print("Cek Bola Diudara");
            Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
            if (hits.Length > 0)
            {
                Transform ball = hits[0].transform;

                // Jika sedang dalam mode pendingPass, jangan smash, tapi pass
                if (pendingPass)
                {
                    Debug.Log("🔄 Pass bola (override smash karena pendingPass)");
                    PassBallInOwnArena();
                    pendingPass = false;
                    targetPassBall = null;
                    hasRescueHit = true;
                }
                else if (ball.position.y >= smashHeightThreshold)
                {
                    Debug.Log("🔥 Smash karena bola tinggi");
                    SmashBall(ball);
                }
                else
                {
                    Debug.Log("🏐 Hit biasa (bola belum cukup tinggi)");
                    HitBallToOtherSide();
                }

                hasHitDuringJump = true;
            }
        }

        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
        else
        {
            rb.linearVelocity = dashDirection * dashForce + new Vector3(0, rb.linearVelocity.y, 0);
            return; // Jangan lanjut gerak normal selama dash
        }

        // Gerakan
        Vector3 moveDir;
        if (IsGrounded())
        {
            moveDir = new Vector3(-moveInput.x, 0, -moveInput.y).normalized;
            lastGroundMoveDir = moveDir.magnitude > 0.1f ? moveDir : Vector3.zero;
        }
        else
        {
            moveDir = lastGroundMoveDir * 0.2f;
        }

       

        // Rotasi model
        if (modelTransform != null)
        {
            if (!IsGrounded())
            {
                modelTransform.forward = Vector3.left;
            }
            else
            {
                Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                if (horizontalVelocity.magnitude > 0.1f)
                {
                    modelTransform.forward = horizontalVelocity.normalized;
                }
            }
        }

        Vector3 v = rb.linearVelocity;

        // Jika sedang auto chase, abaikan input movement manual
        if (isAutoChasingBall && !IsGrounded())
        {
            chaseTimer -= Time.fixedDeltaTime;
            Vector3 direction = (targetBallXZPos - transform.position);
            direction.y = 0f;

            if (direction.magnitude > 0.1f && chaseTimer > 0f)
            {
                rb.linearVelocity = new Vector3(
                    direction.normalized.x * autoMoveSpeed,
                    v.y,
                    direction.normalized.z * autoMoveSpeed
                );
            }
            else
            {
                isAutoChasingBall = false;
            }
        }
        else
        {
            // Hanya apply input move jika tidak auto chase
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, v.y, moveDir.z * moveSpeed);
        }
    }


    int GetZoneIndexFromInput(Vector2 input)
    {
        int col = 1, row = 1;

        if (input.x < -0.3f) col = 0;
        else if (input.x > 0.3f) col = 2;

        if (input.y > 0.3f) row = 0;
        else if (input.y < -0.3f) row = 2;

        return row * 3 + col;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (hitPoint != null) Gizmos.DrawWireSphere(hitPoint.position, hitRadius);

        Gizmos.color = Color.green;
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetBallXZPos, 0.3f);
    }
}