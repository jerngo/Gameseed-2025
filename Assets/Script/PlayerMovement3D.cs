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

    public float serveSpeed=7;
    public float serveSpikeHeightMin = -0.2f;
    public float serveSpikeHeightMax = 0f;

    [Header("Target Zones (9 Grid)")]
    public Transform[] ownZones = new Transform[9];
    public Transform[] enemyZones = new Transform[9];

    public LayerMask ballLayer;

    private PlayerControls controls;
    [SerializeField]
    private Rigidbody rb;

    private Vector2 moveInput;
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

    public string ArenaSide;

    BallBounce ballManager;
    PlayerSwitchManager playerSwitchManager;
    NPCManager npcSwitchManager;
    GameRuleManager gamerulemanager;

    public Transform DefaultPosition;
    public GameObject activeSign;

    public AudioSource hitSound;
    public AudioSource smashSound;
    public AudioSource jumpSound;
    public AudioSource slowmoSound;
    public AudioSource serveSOund;

    public Animator anim;
    void Awake()
    {
        playerSwitchManager = FindFirstObjectByType<PlayerSwitchManager>();
        npcSwitchManager = FindFirstObjectByType<NPCManager>();
        controls = new PlayerControls();
        ballManager = FindFirstObjectByType<BallBounce>();
        gamerulemanager = FindFirstObjectByType<GameRuleManager>();

        controls.Player.Move.performed += ctx => {
            if (isControlled) moveInput = ctx.ReadValue<Vector2>();
        };
        controls.Player.Move.canceled += _ => {
            if (isControlled) moveInput = Vector2.zero;
        };
        controls.Player.Dash.performed += _ => {
            if (CanReceiveInput()) dashPressed = true;
        };
        controls.Player.Hit.performed += _ => {
            if (CanReceiveInput()) hitPressed = true;
        };
        controls.Player.PowerShoot.performed += _ => {
            if (CanReceiveInput() && IsPowerReady()) TryPowerShoot();
        };
        controls.Player.Serve.performed += _ => {
            if (isServing) StartServing();
        };

    }

    public void StopMovement() {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        lastGroundMoveDir = Vector3.zero;
        if (anim != null)
        {
            anim.SetFloat("Speed", 0);
        }
    }

    public void TeleChartoDefaultPos() {
        Vector3 targetPos = DefaultPosition.position;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        isAutoChasingBall = false;
        // Ambil x & z dari DefaultPosition, y tetap dari posisi sekarang
        Vector3 newPos = new Vector3(targetPos.x, 1.711f, targetPos.z);

        transform.position = newPos;
        modelTransform.forward = Vector3.left;
        rb.isKinematic = false;
    }

    public void TeleChartoHere(Transform target)
    {
        Vector3 targetPos = target.position;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        isAutoChasingBall = false;
        // Ambil x & z dari DefaultPosition, y tetap dari posisi sekarang
        Vector3 newPos = new Vector3(targetPos.x, 1.711f, targetPos.z);

        transform.position = newPos;
        modelTransform.forward = Vector3.left;
        rb.isKinematic = false;
    }

    private bool CanReceiveInput()
    {
        if (!isControlled) return false;

        if (isServing) return false;

        // Jika sedang mode free for all, hanya karakter terdekat dengan bola yang terima input
        if (playerSwitchManager != null && playerSwitchManager.isControlAll)
        {
            return playerSwitchManager.IsClosestToBall(gameObject);
        }

        return true;
    }

    bool IsPowerReady() {
        return gamerulemanager.playerPower > 0;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start() => rb = GetComponent<Rigidbody>();

    void StartAutoChaseToBall()
    {
        if (ballManager.arenaSide == ArenaSide) {
            Collider[] balls = Physics.OverlapSphere(transform.position, 30f, ballLayer);
            if (balls.Length > 0)
            {
                jumpSound.Play();

                Transform ball = balls[0].transform;

                // Ambil posisi XZ bola sebagai target
                targetBallXZPos = PredictBallLandingPosition(ball.GetComponent<Rigidbody>());
                isAutoChasingBall = true;
                chaseTimer = maxChaseDuration;

                Debug.Log("🔵 Auto kejar bola aktif: " + targetBallXZPos);
            }
        }
       
    }

    Vector3 PredictBallLandingPosition(Rigidbody ballRb)
    {
        Vector3 velocity = ballRb.linearVelocity;
        Vector3 position = ballRb.position;

        float time = 0f;
        float timeStep = 0.05f;
        float maxTime = 3f;
        Vector3 gravity = Physics.gravity;

        for (; time < maxTime; time += timeStep)
        {
            // Prediksi posisi di masa depan
            Vector3 futurePos = position + velocity * time + 0.5f * gravity * time * time;

            // Jika bola mencapai atau melewati permukaan tanah
            if (futurePos.y <= groundCheck.position.y + 0.1f)
            {
                // Ambil posisi XZ saja, pakai tinggi karakter
                return new Vector3(futurePos.x, transform.position.y, futurePos.z);
            }
        }

        // Jika tidak ketemu, fallback ke posisi bola saat ini
        return new Vector3(position.x, transform.position.y, position.z);
    }


    void CharacterAction() {
        if (!isControlled) return;
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

            if (playerSwitchManager.hitCount < 2)
            {
                TryPassToBall();

            }
            else
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

        }
    }

    void Update()
    {
        bool isIdle = rb.linearVelocity.magnitude < 0.1f;
        if (isIdle)
        {
            if (anim != null)
            {
                anim.SetFloat("Speed", 0);
            }
        }

        if (IsGrounded())
        {
            if (anim != null)
            {
                anim.SetBool("IsGrounded", true);
            }
        }

        else
        {
            if (anim != null)
            {
                anim.SetBool("IsGrounded", false);
            }
            float verticalVelocity = rb.linearVelocity.y;

            // Cek arah vertikal
            bool isFalling = verticalVelocity < -0.1f;

            if (anim != null)
            {
                anim.SetBool("IsFalling", isFalling);
            }
        }

        if (isInSlowMotion)
        {
            slowMotionTimer -= Time.unscaledDeltaTime;
            if (slowMotionTimer <= 0f)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
                isInSlowMotion = false;
                Debug.Log("⏱️ Slow Motion selesai");
            }
        }

        if (isControlled) {
            if (IsGrounded())
            {
                if (!playerSwitchManager.isControlAll)
                {
                    CharacterAction();
                }
                else {
                    if (playerSwitchManager.IsClosestToBall(gameObject))
                    {
                        Debug.Log(gameObject.name+" yang paling dekat = " + playerSwitchManager.IsClosestToBall(gameObject));
                        CharacterAction();
                    }
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
                //anim.SetBool("IsGrounded", true);
            }

            if (IsGrounded() && isAutoChasingBall)
            {
                //isAutoChasingBall = false;
                //anim.SetBool("IsGrounded", false);
            }
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
        ballManager.arenaSide = "";
        ballManager.LastSideToHitTheBall = ArenaSide;
        playerSwitchManager.hitCount = 0;
        playerSwitchManager.ReturnToSingleControl(this.gameObject);

        npcSwitchManager.EnableAllControl();

        Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        if (hits.Length == 0) return;

        Transform ball = hits[0].transform;
        int zoneIndex = GetZoneIndexFromInput(moveInput);
        if (zoneIndex < 0 || zoneIndex >= enemyZones.Length || enemyZones[zoneIndex] == null) return;

        Vector3 target = enemyZones[zoneIndex].position;

        moveInput = Vector2.zero;

        LaunchBallToTarget(ball, target, hitForce);
    }

    void SmashBall(Transform ball)
    {
        smashSound.Play();
        if (anim != null) { 
            anim.SetTrigger("Spike");
            anim.Play("Armature_Spike");
        }

        ballManager.arenaSide = "";
        playerSwitchManager.hitCount = 0;
        ballManager.LastSideToHitTheBall = ArenaSide;
        playerSwitchManager.ReturnToSingleControl(this.gameObject);

        npcSwitchManager.EnableAllControl();

        int zoneIndex = GetZoneIndexFromInput(moveInput);
        if (zoneIndex < 0 || zoneIndex >= enemyZones.Length || enemyZones[zoneIndex] == null) return;

        Vector3 target = enemyZones[zoneIndex].position;

        moveInput = Vector2.zero;

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.useGravity = true;

        // Hitung jarak horizontal (XZ) antara bola dan target
        Vector2 ballXZ = new Vector2(ball.position.x, ball.position.z);
        Vector2 targetXZ = new Vector2(target.x, target.z);
        float distanceXZ = Vector2.Distance(ballXZ, targetXZ);

        // Semakin dekat -> Y lebih negatif (menukik), Semakin jauh -> Y lebih datar
        float minY = -0.8f; // tukikan tajam (jarak dekat)
        float maxY = -0.2f; // tukikan datar (jarak jauh)
        float maxDistance = 10;

        if (ballManager.isServingBall)
        {
            minY = serveSpikeHeightMin;
            maxY = serveSpikeHeightMax;
        }

        float t = Mathf.Clamp01(distanceXZ / maxDistance); // pastikan t antara 0–1
        float dynamicY = Mathf.Lerp(maxY, minY, 1 - t); // dibalik untuk efek terbalik

       

        // Arahkan dan ubah arah Y jadi dinamis
        Vector3 direction = (target - ball.position).normalized;
        direction.y = dynamicY;
        direction.Normalize();

        if (!ballManager.isServingBall)
        {
            ballRb.linearVelocity = direction * smashSpeed;
        }
        else {
            ballRb.linearVelocity = direction * serveSpeed;
        }

        ballManager.isServingBall = false;

        CameraShaker.shaker.ShakeCamera(2f, 3f, 0.3f);
        Debug.DrawLine(ball.position, target, Color.yellow, 2f);
        Debug.Log("🔥 Smash! Ke zona " + zoneIndex + ", jarak = " + distanceXZ.ToString("F2") + ", Y = " + dynamicY.ToString("F2"));
    }




    void PassBallInOwnArena()
    {
        ballManager.arenaSide = ArenaSide;
        ballManager.LastSideToHitTheBall = ArenaSide;
        playerSwitchManager.hitCount++;

        npcSwitchManager.hitCount = 0;
        npcSwitchManager.EnableAllControl();

        Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        if (hits.Length == 0) return;

        Transform ball = hits[0].transform;
        int zoneIndex = GetZoneIndexFromInput(moveInput);
        if (zoneIndex < 0 || zoneIndex >= ownZones.Length || ownZones[zoneIndex] == null) return;

        moveInput = Vector2.zero;

        Vector3 target = ownZones[zoneIndex].position;
        LaunchBallToTarget(ball, target, lobForce);

        playerSwitchManager.ReturnToSingleControl(this.gameObject);
    }

    public float minDashDistance = 1;
    bool isDashingToBall = false;
    Transform dashBallTarget;
    float dashTime = 0f;
    public float maxDashTime = 0.4f;

    void TryPassToBall()
    {

        Collider[] balls = Physics.OverlapSphere(transform.position, 10f, ballLayer);
        if (balls.Length == 0)
        {
            // Aktifkan mode dash ke bola
            Debug.Log("🏃 Dash otomatis ke bola");
            isDashingToBall = true;
            dashBallTarget = this.gameObject.transform;
            dashTime = maxDashTime;
            if (anim != null) { 
                anim.SetTrigger("Dash");
                anim.Play("Armature_Dash");
            }

            jumpSound.Play();
            return;
        }

        Transform ball = balls[0].transform;
        Vector3 toBall = ball.position - transform.position;
        float distXZ = new Vector2(toBall.x, toBall.z).magnitude;
        float verticalOffset = ball.position.y - transform.position.y;

        if (IsGrounded())
        {
            if (Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer).Length > 0)
            {
                Debug.Log("✅ Pass langsung saat di tanah");
                PassBallInOwnArena();
            }
            else if (verticalOffset > 1.2f && distXZ < 1)
            {
                Debug.Log("⬆️ Bola tinggi, lompat & siapkan pass");
                jumpSound.Play();
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                StartAutoChaseToBall();

                pendingPass = true;
                targetPassBall = ball;
            }
            else
            {
                // Aktifkan mode dash ke bola
                Debug.Log("🏃 Dash otomatis ke bola");
                isDashingToBall = true;
                dashBallTarget = ball;
                dashTime = maxDashTime;
                if (anim != null) { 
                    anim.SetTrigger("Dash");
                    anim.Play("Armature_Dash");
                }

                jumpSound.Play();
            }
        }
    }


    void LaunchBallToTarget(Transform ball, Vector3 target, float baseArcHeight)
    {
        hitSound.Play();
        if (anim != null) { 
            anim.SetTrigger("Pass");
            anim.Play("Armature_Pass");
        }

        ballManager.isServingBall = false;

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.useGravity = true;

        float horizontalDistance = Vector3.Distance(new Vector3(ball.position.x, 0, ball.position.z), new Vector3(target.x, 0, target.z));
        float adjustedArcHeight = Mathf.Clamp(baseArcHeight + (horizontalDistance * 0.25f), baseArcHeight, 25f);

        Vector3 velocity = CalculateLaunchVelocity(ball.position, target, adjustedArcHeight);
        rb.linearVelocity = velocity;

        moveInput = Vector2.zero;

        // Tambahkan rotasi sesuai arah terbang bola
        Vector3 flightDirection = velocity.normalized;
        Vector3 spinAxis = Vector3.Cross(flightDirection, Vector3.up).normalized;
        float spinStrength = 15f;
        rb.angularVelocity = spinAxis * spinStrength;

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
        if (IsGrounded())
        {
            if (anim != null)
            {
                anim.SetBool("IsGrounded", true);
            }
        }

        else
        {
            if (anim != null)
            {
                anim.SetBool("IsGrounded", false);
            }
            float verticalVelocity = rb.linearVelocity.y;

            // Cek arah vertikal
            bool isFalling = verticalVelocity < -0.1f;

            if (anim != null)
            {
                anim.SetBool("IsFalling", isFalling);
            }
        }

        if (!isControlled)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

            isDashing = false;
            isDashingToBall = false;
            isAutoChasingBall = false;

            if (!gamerulemanager.isServingRound) { 
                MoveToDefaultPas();
            }
            return;
        }

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
                    if (isPowerShootPending)
                    {
                        Debug.Log("💥 PowerShoot aktif!");
                        PowerShootBall(ball);
                        isPowerShootPending = false;
                        powerShootTimer = powerShootSlowTime;
                    }
                    else
                    {
                        Debug.Log("🔥 Smash karena bola tinggi");
                        SmashBall(ball);
                    }
                }
                else
                {
                    if (isPowerShootPending)
                    {
                        Debug.Log("💥 PowerShoot aktif!");
                        PowerShootBall(ball);
                        isPowerShootPending = false;
                        powerShootTimer = powerShootSlowTime;
                    }
                    else { 
                        Debug.Log("🏐 Hit biasa (bola belum cukup tinggi)");
                        HitBallToOtherSide();
                    }
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

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                if (anim != null) { 
                    anim.SetBool("IsDashing", false);
                }
            }
            else
            {
                if (anim != null) { 
                    anim.SetBool("IsDashing", true);
                }
                rb.linearVelocity = dashDirection * dashForce + new Vector3(0, rb.linearVelocity.y, 0);

                // Cek apakah saat dash mengenai bola → langsung pass
                Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
                if (hits.Length > 0)
                {
                    Debug.Log("✅ Passing bola saat dash");
                    PassBallInOwnArena();
                    isDashing = false; // opsional: batasi satu kali
                }

                return; // Jangan lanjut gerak normal selama dash
            }
        }

        if (isDashingToBall)
        {
            dashTime -= Time.fixedDeltaTime;

            if (dashBallTarget == null || dashTime <= 0f)
            {
                isDashingToBall = false;
                return;
            }

            Vector3 dashDir = new Vector3(modelTransform.forward.x, 0, modelTransform.forward.z).normalized;


            rb.linearVelocity = dashDir * dashForce + new Vector3(0, rb.linearVelocity.y, 0);

            // ⬇️ Tambahan ini
            if (modelTransform != null && dashDir.sqrMagnitude > 0.01f)
            {
                modelTransform.forward = dashDir;
            }

            // Cek overlap bola untuk pass otomatis
            if (Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer).Length > 0)
            {
                Debug.Log("✅ Pass otomatis saat dash ke bola");
                PassBallInOwnArena();
                //isDashingToBall = false;
            }

            return; // Selama dash aktif, abaikan kontrol biasa
        }

        

        // Gerakan
        Vector3 moveDir= new Vector3(0, 0, 0);
        if (IsGrounded())
        {
            moveDir = new Vector3(-moveInput.x, 0, -moveInput.y).normalized;
            lastGroundMoveDir = moveDir.magnitude > 0.1f ? moveDir : Vector3.zero;

        }
        else
        {

            //moveDir = lastGroundMoveDir * 0.2f;
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

        if (anim != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float speed = horizontalVelocity.magnitude;
            anim.SetFloat("Speed", speed);
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

    //Serving
    public bool isServing = false;
    private int serveStage = 0; // 0 = belum lempar, 1 = sudah lempar ke atas, siap smash
    public Transform ballHolder;
    public float servingHeight=10;
    public void SetServer() {
        serveStage = 0;
        isServing = true;
        
        // Ambil bola
        //Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        //if (hits.Length == 0) return;

        //playerSwitchManager.ball = hits[0].transform;
        Rigidbody ballRb = playerSwitchManager.ball.GetComponent<Rigidbody>();
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.useGravity = false;
        ballRb.isKinematic = true;

        // Tempelkan bola ke tangan
        playerSwitchManager.ball.SetParent(ballHolder, true);
        playerSwitchManager.ball.localPosition = Vector3.zero;
        playerSwitchManager.ball.localRotation = Quaternion.identity;
    }

    void StartServing()
    {
        if (!playerSwitchManager.ball) return;

        if (serveStage == 0)
        {
            if (anim != null) { 
                anim.SetTrigger("Serve");
                anim.Play("Armature_Serve");
            }
            serveSOund.Play();
            // Lepaskan dari tangan dan lempar ke atas
            ballManager.ToggleTrail(false);
            playerSwitchManager.ball.SetParent(null);
            Rigidbody ballRb = playerSwitchManager.ball.GetComponent<Rigidbody>();
            ballRb.isKinematic = false;
            ballRb.useGravity = true;
            ballRb.linearVelocity = Vector3.up * servingHeight; // lempar ke atas
            ballManager.LastSideToHitTheBall = "Player";
            serveStage = 1;
            gamerulemanager.barrierServe.SetActive(false);
            gamerulemanager.isServingRound = false;
        }
        else
        {
            jumpSound.Play();
            ballManager.ToggleTrail(true);
            // Pukul ke arah lawan
            hitPressed = true;
            ballManager.isServingBall = true;
            isServing = false;
            serveStage = 0;
        }
    }

    void PassBallInPlace()
    {
        ballManager.arenaSide = ArenaSide;

        Collider[] hits = Physics.OverlapSphere(hitPoint.position, hitRadius, ballLayer);
        if (hits.Length == 0) return;

        Transform ball = hits[0].transform;

        Vector3 targetPosition = hitPoint.position;

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.useGravity = false;

        ball.position = targetPosition;
    }

    private bool isPowerShootPending = false;
    private float powerShootSlowTime = 0.5f;
    private float powerShootTimer = 0f;
    public float powerShootSpeed = 35f;

    private float slowMotionDuration = 3f;
    private float slowMotionTimer = 0f;
    public float slowMotionScale = 0.3f; // 0.3 = 30% kecepatan normal
    private bool isInSlowMotion = false;
    void TryPowerShoot()
    {
        if (!IsGrounded()) return;

        slowmoSound.Play();
        gamerulemanager.UsePower(ArenaSide);

        Debug.Log("🚀 Mulai PowerShoot + Slow Motion");
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        StartAutoChaseToBall();

        isPowerShootPending = true;

        // Aktifkan slow motion global
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // sesuaikan physics update
        slowMotionTimer = slowMotionDuration;
        isInSlowMotion = true;
    }


    void PowerShootBall(Transform ball)
    {
        smashSound.Play();
        if (anim != null) {
            anim.Play("Armature_Spike");
            anim.SetTrigger("Spike");
        }

        slowMotionTimer = 0f;
        ballManager.arenaSide = "";
        ballManager.LastSideToHitTheBall = ArenaSide;
        playerSwitchManager.hitCount = 0;
        playerSwitchManager.ReturnToSingleControl(this.gameObject);

        npcSwitchManager.EnableAllControl();

        int zoneIndex = GetZoneIndexFromInput(moveInput);
        if (zoneIndex < 0 || zoneIndex >= enemyZones.Length || enemyZones[zoneIndex] == null) return;

        Vector3 target = enemyZones[zoneIndex].position;
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.useGravity = true;

        moveInput = Vector2.zero;

        Vector3 direction = (target - ball.position).normalized;
        direction.y = Mathf.Clamp(direction.y, -0.3f, 0.2f); // bisa disesuaikan

        ballRb.linearVelocity = direction * powerShootSpeed;

        CameraShaker.shaker.ShakeCamera(2f, 3f, 0.3f);
        Debug.DrawLine(ball.position, target, Color.cyan, 2f);
        Debug.Log("💥 PowerShoot ke zona " + zoneIndex + ", arah = " + direction);
    }

    void MoveToDefaultPas() {
        // Bergerak ke DefaultPosition jika belum sampai
        Vector3 toDefault = DefaultPosition.position - transform.position;
        Vector3 toDefaultXZ = new Vector3(toDefault.x, 0, toDefault.z);

        if (toDefaultXZ.magnitude > 0.1f)
        {
            Vector3 moveDir = toDefaultXZ.normalized;
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

            // Hadapkan ke arah gerak
            if (moveDir.magnitude > 0.1f && modelTransform != null)
            {
                modelTransform.forward = moveDir;
            }
        }
        else
        {
            // Sudah sampai posisi, berhenti dan lihat ke kiri
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            if (modelTransform != null)
            {
                modelTransform.forward = Vector3.left;
                if (anim != null)
                {
                    anim.SetFloat("Speed", 0);
                }
            }
        }
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