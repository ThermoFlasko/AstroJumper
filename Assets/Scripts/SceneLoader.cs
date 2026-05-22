using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set;}
    public static event Action<string> OnSceneChanged;

    public string SceneToLoad;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene("LoadingScreen");
    }

    public void LoadNextScene(string nextScene)
    {
        SceneToLoad = nextScene;
        OnSceneChanged?.Invoke(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("LoadingScreen");
    }
}
