using UnityEngine;

public class BallBounce : MonoBehaviour
{
    public float bounceForce = 12f; // Sesuaikan kekuatan mantulan
    public string enemyTag = "enemySide";
    public Vector3 playerAreaDirection = Vector3.left; // Anggap area player berada di kiri

    private Rigidbody rb;

    public string arenaSide;

    PlayerSwitchManager playerswitchManager;
    GameRuleManager gameruleManager;

    public bool isServingBall = false;

    public bool isAlreadyScored = false;

    public PlayerType playerSide = PlayerType.Player1;
    public PlayerType enemySide = PlayerType.Player2;

    public string LastSideToHitTheBall = "";

    public AudioSource ballhitground;

    public TrailRenderer trail;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerswitchManager = FindFirstObjectByType<PlayerSwitchManager>();
        gameruleManager = FindFirstObjectByType<GameRuleManager>();
        trail.enabled = false;

    }

    void OnCollisionEnter(Collision collision)
    {
        //ballhitground.Play();
    }

    public void ToggleTrail(bool toggle) {
        trail.enabled = toggle;
    }

}
