using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public GameSettings gameSettings;
    public RuntimeAnimatorController animatorContHome;
    public RuntimeAnimatorController animatorContAway;


    public GameObject player1ModelHolder;

    public GameObject player2ModelHolder;

    public GameObject enemy1ModelHolder;

    public GameObject enemy2ModelHolder;

    private void Awake()
    {
        // Player 1
        GameObject player1 = Instantiate(gameSettings.PlayerCharacter1, player1ModelHolder.transform);
        player1.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        player1.transform.localRotation = Quaternion.identity;
        player1.transform.localScale = Vector3.one;
        player1.GetComponent<Animator>().runtimeAnimatorController = animatorContHome;

        // Player 2
        GameObject player2 = Instantiate(gameSettings.PlayerCharacter2, player2ModelHolder.transform);
        player2.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        player2.transform.localRotation = Quaternion.identity;
        player2.transform.localScale = Vector3.one;
        Animator animPlayer2 = player2.GetComponent<Animator>();
        animPlayer2.runtimeAnimatorController = animatorContHome;
        animPlayer2.Play("Waving 2");

        // Enemy 1
        GameObject enemy1 = Instantiate(gameSettings.EnemyCharacter1, enemy1ModelHolder.transform);
        enemy1.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        enemy1.transform.localRotation = Quaternion.identity;
        enemy1.transform.localScale = Vector3.one;
        Animator animEnemy1 = enemy1.GetComponent<Animator>();
        animEnemy1.runtimeAnimatorController = animatorContHome;
        animEnemy1.Play("Waving");

        // Enemy 2
        GameObject enemy2 = Instantiate(gameSettings.EnemyCharacter2, enemy2ModelHolder.transform);
        enemy2.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        enemy2.transform.localRotation = Quaternion.identity;
        enemy2.transform.localScale = Vector3.one;
        Animator animEnemy2 = enemy2.GetComponent<Animator>();
        animEnemy2.runtimeAnimatorController = animatorContHome;
        animEnemy2.Play("Waving 2");
    }
}
