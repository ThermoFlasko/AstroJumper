using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance { get; private set; }
    public bool isLoadingSaveData = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this);

        SceneManager.activeSceneChanged += CheckIfSetUpLevel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CheckIfSetUpLevel(Scene current, Scene next)
    {
        LevelSaveData levelSaveData = SaveManager.instance.GetCurrentLevelData();

        if (SaveManager.instance.isLoadingSaveData && next.name == levelSaveData.currLevel)
        {
            SetUpSavedLevel(levelSaveData);
        }
        else
        {
            print(next.name);
        }

        
    }

    public void SetUpSavedLevel(LevelSaveData levelSaveData)
    {
        print($"Setting up saved level {levelSaveData.currLevel}");
        print($"{SceneManager.GetSceneAt(SceneManager.sceneCount-1).name}");
        GameObject testTag = GameObject.FindGameObjectWithTag("TESTTAG");
        Scene loadingInScene = SceneManager.GetSceneByName(levelSaveData.currLevel);

        GameObject[] roots = loadingInScene.GetRootGameObjects();
        
    }
}
