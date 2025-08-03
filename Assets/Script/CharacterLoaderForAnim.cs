using UnityEngine;
using UnityEngine.Animations;

public class CharacterLoaderForAnim : MonoBehaviour
{
    public GameSettings gameSettings;
    public RuntimeAnimatorController animatorContWinner;
    public RuntimeAnimatorController animatorContLoser;


    public GameObject winner1ModelHolder;

    public GameObject winner2ModelHolder;

    public GameObject loser1ModelHolder;

    public GameObject loser2ModelHolder;

    private void Awake()
    {
        // Player 1
        GameObject winner1 = Instantiate(gameSettings.Winner1, winner1ModelHolder.transform);
        winner1.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        winner1.transform.localRotation = Quaternion.identity;
        winner1.transform.localScale = Vector3.one;
        winner1.GetComponent<Animator>().runtimeAnimatorController = animatorContWinner;

        // Player 2
        GameObject winner2 = Instantiate(gameSettings.Winner2, winner2ModelHolder.transform);
        winner2.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        winner2.transform.localRotation = Quaternion.identity;
        winner2.transform.localScale = Vector3.one;
        winner2.GetComponent<Animator>().runtimeAnimatorController = animatorContWinner;

        // Enemy 1
        GameObject loser1 = Instantiate(gameSettings.Loser1, loser1ModelHolder.transform);
        loser1.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        loser1.transform.localRotation = Quaternion.identity;
        loser1.transform.localScale = Vector3.one;
        loser1.GetComponent<Animator>().runtimeAnimatorController = animatorContLoser;

        // Enemy 2
        GameObject loser2 = Instantiate(gameSettings.Loser2, loser2ModelHolder.transform);
        loser2.transform.localPosition = new Vector3(0f, -1.04f, 0f);
        loser2.transform.localRotation = Quaternion.identity;
        loser2.transform.localScale = Vector3.one;
        loser2.GetComponent<Animator>().runtimeAnimatorController = animatorContLoser;
    }
}
