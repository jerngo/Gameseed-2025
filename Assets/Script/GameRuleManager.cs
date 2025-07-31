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
    public Transform enemyServerSpawnPos;

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
    BallBounce ballbounce;

    [SerializeField]
    AudioSource suarapluit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerswitchManager = FindFirstObjectByType<PlayerSwitchManager>();
        ballbounce = FindFirstObjectByType<BallBounce>();

        StartCoroutine(InitGame(0.5f, true));
    }

    IEnumerator InitGame(float duration, bool serveFromPlayer)
    {
        yield return new WaitForSeconds(duration); // tunggu 2 detik

        if (serveFromPlayer)
        {
            PlayerSetServe();
            playerFrontline.GetComponent<PlayerMovement3D>().TeleChartoDefaultPos();
        }
        else {
            PlayerSetServe();
            playerFrontline.GetComponent<PlayerMovement3D>().TeleChartoDefaultPos();
        }

        barrierServe.SetActive(true);
       
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.R)) {
            PlayerSetServe();
            barrierServe.SetActive(true);
            playerBackline.GetComponent<PlayerMovement3D>().TeleChartoHere(playerServerSpawnPos);
            playerFrontline.GetComponent<PlayerMovement3D>().TeleChartoDefaultPos();
        }
    }

    void PlayerSetServe() {
        suarapluit.Play();
        playerswitchManager.PlayerServe();
        ballbounce.isAlreadyScored = false;
        playerBackline.GetComponent<PlayerMovement3D>().TeleChartoHere(playerServerSpawnPos);
    }

    void EnemySetServe() { 
        
    }

    public void UsePowerPlayer() {
        playerPower--;
        powerPlayertext.text = $"Power: {playerPower}";
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


}
