using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingBar : MonoBehaviour {
    public Slider loadingBar;
    public GameObject loadingScreen;

    private AsyncOperation async;

    public void ClickAsync(int level)
    {
        loadingScreen.SetActive(true);
        StartCoroutine(LoadingProgress(level));
    }

    IEnumerator LoadingProgress(int level)
    {
        async = SceneManager.LoadSceneAsync(level);
        while (!async.isDone)
        {
            loadingBar.value = async.progress;
            yield return null;
        }
    }
}
