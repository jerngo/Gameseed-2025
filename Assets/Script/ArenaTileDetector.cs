using UnityEngine;

public class ArenaTileDetector : MonoBehaviour
{
    public int gridId = 1; // Diisi di Inspector
    public bool isPlayerSide;
    private GameRuleManager gameRuleManager;
    public bool isOutzone;
    void Start()
    {
        gameRuleManager = FindFirstObjectByType<GameRuleManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            BallBounce ball = collision.gameObject.GetComponent<BallBounce>();
            if (ball != null)
            {
                if (!isOutzone)
                {
                    int boardIndex = gridId - 1;
                    if (isPlayerSide)
                    {
                        if (!ball.isAlreadyScored)
                        {
                            gameRuleManager.AddMark(boardIndex, ball.enemySide);
                            ball.isAlreadyScored = true;
                            gameRuleManager.DIsableHitMark();
                        }
                    }
                    else
                    {
                        if (!ball.isAlreadyScored)
                        {
                            gameRuleManager.AddMark(boardIndex, ball.playerSide);
                            ball.isAlreadyScored = true;
                            gameRuleManager.DIsableHitMark();
                        }
                    }
                }
                else {
                    if (!ball.isAlreadyScored)
                    {
                        if (ball.LastSideToHitTheBall == "Player")
                        {
                            gameRuleManager.AddScore(ball.enemySide, false);
                            ball.isAlreadyScored = true;
                            gameRuleManager.DIsableHitMark();
                        }
                        else
                        {
                            gameRuleManager.AddScore(ball.playerSide, false);
                            ball.isAlreadyScored = true;
                            gameRuleManager.DIsableHitMark();
                        }
                    }
                        
                }

            }
        }
    }
}
