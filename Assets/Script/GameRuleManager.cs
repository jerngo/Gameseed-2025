using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

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

    public AudioSource bolaMasuk;
    public AudioSource powerUp;

    public AudioSource crowdCheer;
    public AudioSource crowdDissapoint;

    public RawImage playerpower1;
    public RawImage playerpower2;
    public RawImage playerpower3;

    public RawImage enemypower1;
    public RawImage enemypower2;
    public RawImage enemypower3;

    public EaseOutScript loadingScript;
    public int winScore=21;

    public GameSettings gameSettings;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerswitchManager = FindFirstObjectByType<PlayerSwitchManager>();
        enemyswitchManager = FindFirstObjectByType<NPCManager>();
        ballbounce = FindFirstObjectByType<BallBounce>();

        StartCoroutine(InitGame(2f, true));
    }

    private void Awake()
    {
        
    }

    IEnumerator InitGame(float duration, bool serveFromPlayer)
    {
        yield return new WaitForSeconds(duration); // tunggu 2 detik

        if (serveFromPlayer)
        {
            PlayerSetServe();
        }
        else {
            EnemySetServe();
        }

       
    }

    void UpdateStars(int score, RawImage img1, RawImage img2, RawImage img3)
    {
        Color activeColor = new Color32(254, 196, 33, 255);
        Color inactiveColor = Color.white;

        img1.color = (score >= 1) ? activeColor : inactiveColor;
        img2.color = (score >= 2) ? activeColor : inactiveColor;
        img3.color = (score >= 3) ? activeColor : inactiveColor;
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
        ballbounce.ToggleTrail(false);
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
        playerswitchManager.hitCount = 0;
        enemyswitchManager.hitCount = 0;

        ballbounce.ToggleTrail(false);
        suarapluit.Play();
        isServingRound = true;

        ballbounce.isAlreadyScored = false;

        playerBackline.GetComponent<PlayerMovement3D>().TeleChartoDefaultPos();
        playerFrontline.GetComponent<PlayerMovement3D>().TeleChartoDefaultPos();
        enemyBackline.GetComponent<NPCMovement3D>().TeleChartoHere(enemyServerSpawnPos);
        enemyFrontline.GetComponent<NPCMovement3D>().TeleChartoDefaultPos();

        playerBackline.GetComponent<PlayerMovement3D>().isControlled = true;
        playerFrontline.GetComponent<PlayerMovement3D>().isControlled = true;

        barrierServe.SetActive(true);

        enemyswitchManager.EnemyServe();
    }

    public void UsePower(string playertype) {
        if (playertype == "Player")
        {
            playerPower--;
            UpdateStars(playerPower, playerpower1, playerpower2, playerpower3);
            powerPlayertext.text = $"Power: {playerPower}"; 
        }
        else if (playertype == "Enemy")
        {
            enemyPower--;
            UpdateStars(enemyPower, enemypower1, enemypower2, enemypower3);
            powerEnemytext.text = $"Power: {enemyPower}";
        }
    }

    public void AddScore(PlayerType player, bool isIn = true) {
        bolaMasuk.Play();
        if (isIn)
        {
            crowdCheer.Play();
        }
        else {
            crowdDissapoint.Play();
        }

        if (player == PlayerType.Player1) {
            playerScore++;
            scorePlayertext.text = playerScore.ToString();
            if (playerScore >= winScore)
            {
                gameSettings.Winner1 = gameSettings.PlayerCharacter1;
                gameSettings.Winner2 = gameSettings.PlayerCharacter2;
                gameSettings.Loser1 = gameSettings.EnemyCharacter1;
                gameSettings.Loser2 = gameSettings.EnemyCharacter2;

                gameSettings.winnerName = "Player";
                ShowGameEndCaption();
                suarapluit.Play();
                
                loadingScript.EaseIn(3);
            }
            else { 
                StartCoroutine(InitGame(2,true));
            }
        }
        else if (player == PlayerType.Player2)
        {
            enemyScore++;
            scoreEnemytext.text = enemyScore.ToString();
            if (enemyScore >= winScore)
            {
                gameSettings.Loser1 = gameSettings.PlayerCharacter1;
                gameSettings.Loser2 = gameSettings.PlayerCharacter2;
                gameSettings.Winner1 = gameSettings.EnemyCharacter1;
                gameSettings.Winner2 = gameSettings.EnemyCharacter2;

                gameSettings.winnerName = "Enemy";
                ShowGameEndCaption();
                suarapluit.Play();
                loadingScript.EaseIn(3);
            }
            else { 
                StartCoroutine(InitGame(2,false));
            }
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
                color = new Color32(100, 143, 163, 255);
            }
            else if (player == PlayerType.Player2) { 
                color = new Color32(160, 39, 1, 255); ;
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
                    powerUp.Play();
                    if (playerPower < 3) { 
                        playerPower++;
                        UpdateStars(playerPower, playerpower1, playerpower2, playerpower3);
                        powerPlayertext.text = $"Power: {playerPower}";
                    
                    }
                }
                else if (player == PlayerType.Player2)
                {
                    if (enemyPower < 3) { 
                        powerUp.Play();
                        enemyPower++;
                        UpdateStars(enemyPower, enemypower1, enemypower2, enemypower3);
                        powerEnemytext.text = $"Power: {enemyPower}";
                    }
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

    public GameObject gameCaption;
    public float durationZoom;
    public float textSize = 5.2426f;
    public Ease easeIn = Ease.OutBack;
    void ShowGameEndCaption() {
        gameCaption.SetActive(true);
        gameCaption.transform.DOKill();
        gameCaption.transform.localScale = Vector3.zero;
        gameCaption.transform.DOScale(textSize, durationZoom).SetEase(easeIn).OnComplete(() =>
        {
            
        });
    }

}
