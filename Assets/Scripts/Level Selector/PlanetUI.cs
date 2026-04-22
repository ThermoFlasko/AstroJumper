using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetUI : MonoBehaviour
{
    public string sceneToLoad;

    public void LoadScene()
    {

        if(sceneToLoad == null)
        {
            Debug.LogWarning("Attempted Scene Load That Does Not Exist");
            return;
        }
        
        print("Loading scene: " + sceneToLoad);
        SceneLoader.Instance.LoadNextScene(sceneToLoad);
    }
}
