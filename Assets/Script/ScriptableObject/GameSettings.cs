using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public Character PlayerCharacter1;
    public Character PlayerCharacter2;
    public Character EnemyCharacter1;
    public Character EnemyCharacter2;

    public Stage StageDetail;

    public Character Winner1;
    public Character Winner2;

    public Character Loser1;
    public Character Loser2;

}
