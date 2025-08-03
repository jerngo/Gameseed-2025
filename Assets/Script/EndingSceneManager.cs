using UnityEngine;
using System.Collections;

public class EndingSceneManager : MonoBehaviour
{
    public EaseOutScript loadingManager;
    public float duration = 5;
    public int sceneIndexToGo = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CountdownAndGoTohere(sceneIndexToGo));
    }

    
    IEnumerator CountdownAndGoTohere(int index) {
        yield return new WaitForSeconds(duration);
        loadingManager.EaseIn(index);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
