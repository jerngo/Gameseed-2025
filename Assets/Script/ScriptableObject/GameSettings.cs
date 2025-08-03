using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public GameObject PlayerCharacter1;
    public GameObject PlayerCharacter2;
    public GameObject EnemyCharacter1;
    public GameObject EnemyCharacter2;

    public Stage StageDetail;

    public GameObject Winner1;
    public GameObject Winner2;

    public GameObject Loser1;
    public GameObject Loser2;

    public string winnerName = "Player";

}
