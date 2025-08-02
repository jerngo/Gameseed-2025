using UnityEngine;
using System.Collections;

public class NPCManager : MonoBehaviour
{
    public GameObject playerA;
    public GameObject playerB;

    private GameObject currentPlayer;
    private GameObject otherPlayer;
    public int hitCount = 0;
    public bool isControlAll = false;

    public Transform ball;
    public bool isCooldownAction;

    GameRuleManager gameruleManager;

    public GameObject GetOtherPlayer()
    {
        return otherPlayer;
    }

    void Start()
    {
        gameruleManager = FindFirstObjectByType<GameRuleManager>();

        currentPlayer = playerA;
        otherPlayer = playerB;

        EnableController(playerA, false);
        EnableController(playerB, false);

        //otherPlayer.GetComponent<PlayerMovement3D>().SetServer();
    }

    public void EnemyServe()
    {
        EnableController(playerA, false);
        EnableController(playerB, true);
        playerB.GetComponent<NPCMovement3D>().SetServer();
    }

    private void Update()
    {
        //Debug ulang serve nanti hapus

    }

    void EnableController(GameObject player, bool enable)
    {
        var controller = player.GetComponent<NPCMovement3D>();
        if (controller != null)
        {
            StopMovement(player);
            //controller.enabled = enable;
            controller.activeSign.SetActive(enable);
            controller.isControlled = enable;
            controller.StopMovement();
        }
    }

    void DisableAllControl()
    {
        isControlAll = false;

        EnableController(currentPlayer, false);
        EnableController(otherPlayer, false);
    }

    public void EnableAllControl()
    {
        isControlAll = true;

        EnableController(IsClosestToBallLanding(), true);
        hitCount = 0;
    }

    void StopMovement(GameObject player)
    {
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var controller = player.GetComponent<NPCMovement3D>();
        if (controller != null)
        {
            controller.lastGroundMoveDir = Vector3.zero;
        }
    }

    public void OnEnemyTouch()
    {
        hitCount = 0;
    }

    public bool IsClosestToBall(GameObject player)
    {
        if (ball == null) return false;

        Vector2 playerXZ = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 otherXZ = new Vector2(GetOtherPlayerObject(player).transform.position.x, GetOtherPlayerObject(player).transform.position.z);
        Vector2 ballXZ = new Vector2(ball.position.x, ball.position.z);

        float distPlayer = Vector2.Distance(playerXZ, ballXZ);
        float distOther = Vector2.Distance(otherXZ, ballXZ);

        return distPlayer <= distOther;
    }

    public GameObject IsClosestToBallLanding()
    {
        float distEnemy1 = Vector3.Distance(playerA.transform.position, gameruleManager.PredictBallLandingPosition());
        float distEnemy2 = Vector3.Distance(playerB.transform.position, gameruleManager.PredictBallLandingPosition());

        if (distEnemy1 < distEnemy2)
        {
            return playerA;
        }
        else {
            return playerB;
        }
    }

    private GameObject GetOtherPlayerObject(GameObject current)
    {
        if (current == playerA) return playerB;
        if (current == playerB) return playerA;
        return null;
    }

    void SwitchControl()
    {
        // Hentikan pergerakan player lama dulu
        StopMovement(currentPlayer);

        // Pindahkan kontrol
        EnableController(currentPlayer, false);
        EnableController(otherPlayer, true);

        // Tukar peran
        var temp = currentPlayer;
        currentPlayer = otherPlayer;
        otherPlayer = temp;
    }

    public void ReturnToSingleControl(GameObject newController)
    {
        isControlAll = false;

        if (newController == playerA)
        {

            EnableController(playerA, false);
            EnableController(playerB, true);
            currentPlayer = playerB;
            otherPlayer = playerA;
        }
        else
        {
            EnableController(playerA, true);
            EnableController(playerB, false);
            currentPlayer = playerA;
            otherPlayer = playerB;
        }
    }

}
