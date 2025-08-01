using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Quit Game");

        // Keluar dari game saat build
        Application.Quit();

        // Saat di editor, stop play mode (hanya untuk testing di Editor)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
