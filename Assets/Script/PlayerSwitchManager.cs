using UnityEngine;

public class PlayerSwitchManager : MonoBehaviour
{
    public GameObject playerA;
    public GameObject playerB;

    private GameObject currentPlayer;
    private GameObject otherPlayer;
    private int hitCount = 0;

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

    public void OnPlayerHit(GameObject hitter)
    {
        if (hitter != currentPlayer) return;

        hitCount++;
        SwitchControl();
    }

    public void OnEnemyTouch()
    {
        hitCount = 0;
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
}
