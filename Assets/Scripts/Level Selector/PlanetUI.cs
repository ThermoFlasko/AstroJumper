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
        SaveManager.instance.CurrentLevelSaveData.currLevel = sceneToLoad;
        if (sceneToLoad == "PCG_Sample")
        {
            SaveManager.instance.CurrentLevelSaveData.currLevel = "PCG_Sample";
            SaveManager.instance.CurrentLevelSaveData.isPlanetLevel = true;
            print("PCG");
        }
        else if (sceneToLoad == "Space Level 1")
        {
            SaveManager.instance.CurrentLevelSaveData.currLevel = "Space Level 1";
            SaveManager.instance.CurrentLevelSaveData.isPlanetLevel = false;
        }

        SceneLoader.Instance.LoadNextScene(sceneToLoad);
    }
}
