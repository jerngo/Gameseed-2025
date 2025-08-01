using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameRuleManager : MonoBehaviour
{
    public Image[] gridPanels = new Image[9];

    public GameObject playerFrontline;
    public GameObject playerBackline;

    public GameObject enemyFrontline;
    public GameObject enemyBackline;

    public Transform playerServerSpawnPos;

    public Transform playerBacklineSpawnPos;
    public Transform playerFrontlineSpawnPos;

    public Transform enemyServerSpawnPos;

    public Transform enemyBacklineSpawnPos;
    public Transform enemyFrontlineSpawnPos;

    public GameObject barrierServe;

    //public enum PlayerType { None, Player1, Player2 }
    private PlayerType[] board = new PlayerType[9];

    public TextMeshProUGUI powerPlayertext;
    public TextMeshProUGUI powerEnemytext;

    public TextMeshProUGUI scorePlayertext;
    public TextMeshProUGUI scoreEnemytext;

    public int playerPower = 0;
    public int enemyPower = 0;

    int playerScore = 0;
    int enemyScore = 0;

    PlayerSwitchManager playerswitchManager;
    NPCManager enemyswitchManager;
    BallBounce ballbounce;

    public bool isServingRound;

    [SerializeField]
    AudioSource suarapluit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerswitchManager = FindFirstObjectByType<PlayerSwitchManager>();
        enemyswitchManager = FindFirstObjectByType<NPCManager>();
        ballbounce = FindFirstObjectByType<BallBounce>();

        
    }

    private void Awake()
    {
        StartCoroutine(InitGame(1f, true));
    }

    IEnumerator InitGame(float duration, bool serveFromPlayer)
    {
        yield return new WaitForSeconds(duration); // tunggu 2 detik

        if (serveFromPlayer)
        {
            PlayerSetServe();
        }
        else {
            PlayerSetServe();
        }

       
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.R)) {
            PlayerSetServe();
                
        }
    }

    void PlayerSetServe() {
        playerswitchManager.hitCount = 0;
        enemyswitchManager.hitCount = 0;

        suarapluit.Play();
        isServingRound = true;

        ballbounce.isAlreadyScored = false;

        playerBackline.GetComponent<PlayerMovement3D>().TeleChartoHere(playerServerSpawnPos);
        playerFrontline.GetComponent<PlayerMovement3D>().TeleChartoDefaultPos();
        enemyBackline.GetComponent<NPCMovement3D>().TeleChartoDefaultPos();
        enemyFrontline.GetComponent<NPCMovement3D>().TeleChartoDefaultPos();

        enemyBackline.GetComponent<NPCMovement3D>().isControlled=false;
        enemyFrontline.GetComponent<NPCMovement3D>().isControlled = false;

        barrierServe.SetActive(true);
        
        playerswitchManager.PlayerServe();
    }

    public void TeleChartoHere(Transform thingtotele, Transform target)
    {
        Vector3 targetPos = target.position;
        // Ambil x & z dari DefaultPosition, y tetap dari posisi sekarang
        Vector3 newPos = new Vector3(targetPos.x, 1.711f, targetPos.z);

        thingtotele.position = newPos;
    }

    void EnemySetServe() { 
        
    }

    public void UsePower(string playertype) {
        if (playertype == "Player")
        {
            playerPower--;
            powerPlayertext.text = $"Power: {playerPower}";
        }
        else if (playertype == "Enemy")
        {
            enemyPower--;
            powerEnemytext.text = $"Power: {enemyPower}";
        }
    }

    public void AddScore(PlayerType player) {

        if (player == PlayerType.Player1) {
            playerScore++;
            scorePlayertext.text = playerScore.ToString();
            StartCoroutine(InitGame(2,true));
        }
        else if (player == PlayerType.Player2)
        {
            enemyScore++;
            scoreEnemytext.text = enemyScore.ToString();
            StartCoroutine(InitGame(2,false));
        }
    }

    public void AddMark(int position, PlayerType player)
    {
        if (position < 0 || position > 8) return;

        AddScore(player);

        board[position] = player;

        // Set warna grid
        if (gridPanels[position] != null)
        {
            Color color = Color.white;

            if (player == PlayerType.Player1)
            {
                color = Color.blue;
            }
            else if (player == PlayerType.Player2) { 
                color = Color.red;
            }

            gridPanels[position].color = color;
        }

        CheckWin(player);
    }


    private void CheckWin(PlayerType player)
    {
        int[,] winPatterns = new int[,]
        {
        {0,1,2}, {3,4,5}, {6,7,8}, // Rows
        {0,3,6}, {1,4,7}, {2,5,8}, // Columns
        {0,4,8}, {2,4,6}           // Diagonals
        };

        for (int i = 0; i < winPatterns.GetLength(0); i++)
        {
            int a = winPatterns[i, 0];
            int b = winPatterns[i, 1];
            int c = winPatterns[i, 2];

            if (board[a] == player && board[b] == player && board[c] == player)
            {
                Debug.Log($"{player} wins!");

                // Tambah power
                if (player == PlayerType.Player1)
                {
                    playerPower++;
                    powerPlayertext.text = $"Power: {playerPower}";
                }
                else if (player == PlayerType.Player2)
                {
                    enemyPower++;
                    powerEnemytext.text = $"Power: {enemyPower}";
                }

                // Reset semua mark & warna milik player pemenang
                for (int j = 0; j < board.Length; j++)
                {
                    if (board[j] == player)
                    {
                        board[j] = PlayerType.None;

                        if (gridPanels[j] != null)
                            gridPanels[j].color = Color.white;
                    }
                }

                return;
            }
        }
    }

    public Vector3 PredictBallLandingPosition()
    {
        Rigidbody ballRb = ballbounce.GetComponent<Rigidbody>();

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
            if (futurePos.y <=  0.65)
            {
                // Ambil posisi XZ saja, pakai tinggi karakter
                return new Vector3(futurePos.x, transform.position.y, futurePos.z);
            }
        }

        // Jika tidak ketemu, fallback ke posisi bola saat ini
        return new Vector3(position.x, transform.position.y, position.z);
    }
}
