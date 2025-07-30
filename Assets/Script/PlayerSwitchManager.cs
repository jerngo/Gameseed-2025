using UnityEngine;

public class PlayerSwitchManager : MonoBehaviour
{
    public GameObject playerA;
    public GameObject playerB;

    private GameObject currentPlayer;
    private GameObject otherPlayer;
    public int hitCount = 0;
    public bool isControlAll = false;



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
            controller.enabled = enable;
            controller.isControlled = enable;
        }
    }

    void EnableAllControl() {
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

    public GameObject GetNearestPlayerToBall(Transform ball)
    {
        float distA = Vector3.Distance(playerA.transform.position, ball.position);
        float distB = Vector3.Distance(playerB.transform.position, ball.position);

        return (distA <= distB) ? playerA : playerB;
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
            EnableController(playerA, true);
            EnableController(playerB, false);
            currentPlayer = playerA;
            otherPlayer = playerB;
        }
        else
        {
            EnableController(playerA, false);
            EnableController(playerB, true);
            currentPlayer = playerB;
            otherPlayer = playerA;
        }
    }

}
