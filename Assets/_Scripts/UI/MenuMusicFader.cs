using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

class MenuMusicFader : MonoBehaviour
{
    public float fadeDuration;

    AudioSource aS;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        aS = GetComponent<AudioSource>();
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
    }

    private void SceneManager_sceneUnloaded(Scene arg0)
    {
        StartCoroutine(FadeOutMusic());
    }

    IEnumerator FadeOutMusic()
    {
        float t = 1;
        while (t >= 0)
        {
            t -= Time.deltaTime / fadeDuration;
            aS.volume = t;
            yield return true;
        }
        SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
        Destroy(gameObject);
    }
}

