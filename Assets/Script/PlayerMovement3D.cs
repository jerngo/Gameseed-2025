using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement3D : MonoBehaviour
{
    [Header("Model")]
    public Transform modelTransform;

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
    public float hitForce = 20f;
    public float lobForce = 12f;
    public float normalForce = 7f;
    public float longForce = 12f;

    [Header("Target Zones (9 Grid)")]
    public Transform[] targetZones = new Transform[9];   // isi di Inspector

    public LayerMask ballLayer;

    private PlayerControls controls;
    private Rigidbody rb;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool dashPressed;
    private bool hitPressed;

    private bool isGrounded;
    public Vector3 lastGroundMoveDir = Vector3.zero;

    private bool isDashing = false;
    private Vector3 dashDirection;
    private float dashDuration = 0.4f;
    private float dashTimer = 0f;

    private bool hasRescueHit = false;

    public float passHeight = 1.5f;
    public float passSpeedFactor = 3f;

    public bool isControlled;
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

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (jumpPressed && isGrounded && !isDashing)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }

        if (dashPressed && !isDashing)
        {
            dashPressed = false;
            isDashing = true;
            dashTimer = dashDuration;

            dashDirection = new Vector3(-moveInput.x, 0, -moveInput.y).normalized;

            if (dashDirection == Vector3.zero)
            {
                dashDirection = -Vector3.right; // default dash ke kanan
            }

            rb.linearVelocity = Vector3.zero;
            rb.AddForce(dashDirection * dashForce + Vector3.up * 2f, ForceMode.Impulse);
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            // Aktifkan rescue hit satu kali menjelang akhir dash
            if (!hasRescueHit && dashTimer <= 0.1f)
            {
                DoRescueHit();
                hasRescueHit = true;
            }

            if (dashTimer <= 0f)
            {
                isDashing = false;
                hasRescueHit = false;
            }

            return;
        }

        // Hit hanya bisa dilakukan saat tidak sedang dash
        if (!hitPressed || isDashing) return;
        hitPressed = false;

        // Detect bola
        Collider[] hitBalls = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        if (hitBalls.Length == 0) return;

        Rigidbody ballRb = hitBalls[0].attachedRigidbody;
        if (ballRb == null) return;

        // ─── GROUND HIT ───
        if (isGrounded)
        {
            GameObject otherPlayer = FindFirstObjectByType<PlayerSwitchManager>().GetOtherPlayer();

            if (otherPlayer != null) { 
                   PassBallToOtherPlayer(ballRb, this.gameObject);
            }


            return;
        }

        // ─── AERIAL HIT (SMASH, LOB, NORMAL) ───
        int zoneIndex = GetZoneIndexFromInput(moveInput);
        if (zoneIndex < 0 || zoneIndex >= targetZones.Length || targetZones[zoneIndex] == null)
            zoneIndex = 4;

        Vector3 ballPos = hitBalls[0].transform.position;
        Vector3 targetPos = targetZones[zoneIndex].position;
        Vector3 dir = (targetPos - ballPos).normalized;

        float forcePower = hitForce;
        if (moveInput.x < -0.3f)      // Lob
        {
            forcePower = lobForce;
            dir.y = 1.0f;
            forcePower *= 0.7f;
        }
        else if (moveInput.x > 0.3f)  // Smash
        {
            forcePower = longForce;
            dir.y = -0.2f;
            forcePower *= 1.7f;
        }
        else                          // Normal
        {
            forcePower = normalForce;
            dir.y = 0.2f;
            forcePower *= 1.2f;
        }

        ballRb.useGravity = true;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce(dir.normalized * forcePower, ForceMode.Impulse);
    }


    void DoRescueHit()
    {
        Collider[] hitBalls = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        if (hitBalls.Length == 0) return;

        Rigidbody ballRb = hitBalls[0].attachedRigidbody;
        if (ballRb == null) return;

        PassBallToOtherPlayer(ballRb, this.gameObject);

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

    void PassBallToOtherPlayer(Rigidbody ballRb, GameObject fromPlayer)
    {
        GameObject otherPlayer = FindFirstObjectByType<PlayerSwitchManager>().GetOtherPlayer();
        if (otherPlayer == null || ballRb == null) return;

        Vector3 start = ballRb.position;
        Vector3 end = otherPlayer.transform.position + Vector3.up * passHeight;

        float distance = Vector3.Distance(start, end);
        float flightTime = Mathf.Clamp(distance / passSpeedFactor, 0.6f, 2f);

        Vector3 force = CalculateParabolaVelocity(start, end, flightTime);

        ballRb.linearVelocity = Vector3.zero;
        ballRb.useGravity = true;
        ballRb.AddForce(force, ForceMode.VelocityChange);

        FindFirstObjectByType<PlayerSwitchManager>().OnPlayerHit(fromPlayer);
    }


    /* ─────────────────────  FixedUpdate  ───────────────────── */
    void FixedUpdate()
    {
        if (isDashing)
            return;

        Vector3 moveDir;

        if (isControlled)
        {
            if (isGrounded)
            {
                moveDir = new Vector3(-moveInput.x, 0, -moveInput.y).normalized;
                lastGroundMoveDir = moveDir.magnitude > 0.1f ? moveDir : Vector3.zero;
            }
            else
            {
                moveDir = lastGroundMoveDir * 0.2f;
            }

            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, v.y, moveDir.z * moveSpeed);
        }
        else
        {
            // Jika tidak dikontrol, hentikan pergerakan horizontal
            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0, v.y, 0);

            // Reset arah gerakan terakhir agar tidak kebawa saat kontrol berpindah kembali
            lastGroundMoveDir = Vector3.zero;
        }

        // ─── ROTATE MODEL ───
        if (modelTransform != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            if (horizontalVelocity.magnitude > 0.1f)
            {
                modelTransform.forward = horizontalVelocity.normalized;
            }
            else if (!isGrounded || isDashing)
            {
                modelTransform.forward = Vector3.left;
            }
        }


    }


    /* ─────────────────────  Util  ───────────────────── */
    public bool IsGrounded() => isGrounded;

    ///  Layout grid:
    ///  [0][1][2]
    ///  [3][4][5]
    ///  [6][7][8]
    int GetZoneIndexFromInput(Vector2 input)
    {
        int col = 1, row = 1;                   // tengah

        if (input.x < -0.3f) col = 0;           // A
        else if (input.x > 0.3f) col = 2;       // D

        if (input.y > 0.3f) row = 0;           // W (atas)
        else if (input.y < -0.3f) row = 2;      // S (bawah)

        return row * 3 + col;                   // indeks 0‑8
    }

    /* ─────────────────── Gizmos helper ─────────────────── */
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (hitPoint != null) Gizmos.DrawWireSphere(hitPoint.position, hitRadius);

        Gizmos.color = Color.green;
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.cyan;
        if (targetZones != null)
            foreach (Transform t in targetZones)
                if (t != null) Gizmos.DrawSphere(t.position, 0.2f);
    }
}
