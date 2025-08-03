using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    public GameSettings gameSettings;

    public PlayerMovement3D player1Anim;
    public GameObject player1ModelHolder;

    public PlayerMovement3D player2Anim;
    public GameObject player2ModelHolder;

    public NPCMovement3D enemy1Anim;
    public GameObject enemy1ModelHolder;

    public NPCMovement3D enemy2Anim;
    public GameObject enemy2ModelHolder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // Player 1
        GameObject player1Instance = Instantiate(gameSettings.PlayerCharacter1, player1ModelHolder.transform);
        player1Instance.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        player1Instance.transform.localRotation = Quaternion.identity;
        player1Instance.transform.localScale = Vector3.one;
        player1Anim.anim = player1Instance.GetComponent<Animator>();

        // Player 2
        GameObject player2Instance = Instantiate(gameSettings.PlayerCharacter2, player2ModelHolder.transform);
        player2Instance.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        player2Instance.transform.localRotation = Quaternion.identity;
        player2Instance.transform.localScale = Vector3.one;
        player2Anim.anim = player2Instance.GetComponent<Animator>();

        // Enemy 1
        GameObject enemy1Instance = Instantiate(gameSettings.EnemyCharacter1, enemy1ModelHolder.transform);
        enemy1Instance.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        enemy1Instance.transform.localRotation = Quaternion.identity;
        enemy1Instance.transform.localScale = Vector3.one;
        enemy1Anim.anim = enemy1Instance.GetComponent<Animator>();

        // Enemy 2
        GameObject enemy2Instance = Instantiate(gameSettings.EnemyCharacter2, enemy2ModelHolder.transform);
        enemy2Instance.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        enemy2Instance.transform.localRotation = Quaternion.identity;
        enemy2Instance.transform.localScale = Vector3.one;
        enemy2Anim.anim = enemy2Instance.GetComponent<Animator>();
    }

}
