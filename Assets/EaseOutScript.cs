using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class EaseOutScript : MonoBehaviour
{
    [Header("Animation")]
    public GameObject loadingScreen;

    public float duration = 0.3f;
    public float delayIfToHideNull = 0f;
    public Ease easeOut = Ease.InBack;
    public Ease easeIn = Ease.OutBack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EaseOut();
    }

    public void EaseOut() {
        loadingScreen.transform.DOKill();

        if (loadingScreen.transform.localScale == Vector3.zero)
            loadingScreen.transform.localScale = Vector3.one;

        loadingScreen.transform.DOScale(0f, duration).SetEase(easeOut).OnComplete(() =>
        {
            loadingScreen.gameObject.SetActive(false);
        });
    }

    public void EaseIn(int sceneIndex)
    {
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.transform.DOKill();
        loadingScreen.transform.localScale = Vector3.zero;
        loadingScreen.transform.DOScale(1f, duration).SetEase(easeIn).OnComplete(() =>
        {
            StartCoroutine(GoToNextSceneIndex(sceneIndex));
        });
    }

    IEnumerator GoToNextSceneIndex(int index)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
