using UnityEngine;

public class PlayerSwitchManager : MonoBehaviour
{
    public GameObject playerA;
    public GameObject playerB;

    private GameObject currentPlayer;
    private GameObject otherPlayer;
    public int hitCount = 0;
    public bool isControlAll = false;

    public Transform ball;

    public GameObject GetOtherPlayer()
    {
        return otherPlayer;
    }

    void Start()
    {
        currentPlayer = playerA;
        otherPlayer = playerB;

        EnableController(currentPlayer, true);
        EnableController(otherPlayer, false);
    }

    void EnableController(GameObject player, bool enable)
    {
        var controller = player.GetComponent<PlayerMovement3D>();
        if (controller != null)
        {
            //controller.enabled = enable;
            controller.activeSign.SetActive(enable);
            controller.isControlled = enable;
        }
    }

    void DisableAllControl()
    {
        isControlAll = false;

        EnableController(currentPlayer, false);
        EnableController(otherPlayer, false);
    }

    public void EnableAllControl() {
        isControlAll = true;

        EnableController(currentPlayer, true);
        EnableController(otherPlayer, true);

    }

    void StopMovement(GameObject player)
    {
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var controller = player.GetComponent<PlayerMovement3D>();
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
