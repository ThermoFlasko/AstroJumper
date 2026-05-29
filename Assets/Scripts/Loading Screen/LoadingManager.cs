using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Linq;

public class LoadingManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text loadingText; // Reference to the TextMeshProUGUI component for displaying loading text

    [Header("Loading Settings")]
    public string sceneToLoad = ""; // Name of the scene to load
    private string[] levelNames = { "Tutorial Ground", "Space Level 1", "PCG_Sample" };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sceneToLoad = SceneLoader.Instance.SceneToLoad;
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false; // Prevent the scene from activating immediately when loading is complete

        float minLoadingTime = 2f; // Minimum time to show the loading screen
        float elapsedTime = 0f;

        // While the scene is still loading
        while (!asyncLoad.isDone)
        {
            elapsedTime += Time.deltaTime;
            if (asyncLoad.progress >= 0.9f && elapsedTime >= minLoadingTime) // Check if the scene has finished loading (progress is 0.9 when loading is complete)
            {
                if (levelNames.Contains(sceneToLoad))
                {
                    SaveManager.instance.IsInLevel = true;
                }
                else
                {
                    SaveManager.instance.IsInLevel = false;
                }
                asyncLoad.allowSceneActivation = true; // Allow the scene to activate
            }
            yield return null;
        }
    }
}
