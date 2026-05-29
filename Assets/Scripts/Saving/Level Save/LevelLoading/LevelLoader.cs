using System.Collections;
using JetBrains.Annotations;
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
        Scene loadingInScene = SceneManager.GetSceneByName(levelSaveData.currLevel);

        GameObject[] roots = loadingInScene.GetRootGameObjects();
        

        if (levelSaveData.isPlanetLevel)
        {
            SetUpPlanetLevel(levelSaveData);    
        }
    }

    public void SetUpPlanetLevel(LevelSaveData levelSaveData)
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        playerGO.transform.position = levelSaveData.planetLevelData.playerPosition;

        // get data and update for the ones

        GameObject meleeEnemies = GameObject.FindGameObjectWithTag("MeleeRoot");

        for (int i = 0; i < meleeEnemies.transform.childCount; i++)
        {
            if ( i > levelSaveData.planetLevelData.meleeEnemies.Count-1)
            {
                Destroy(meleeEnemies.transform.GetChild(i).gameObject);
                continue;
            }

            MeleeSaveData enemySavaData = levelSaveData.planetLevelData.meleeEnemies[i];
            GameObject go = meleeEnemies.transform.GetChild(i).gameObject;

            Unit unit = go.GetComponent<Unit>();

            unit.Health = enemySavaData.health;
            go.transform.position = enemySavaData.position;
        }

        GameObject rangedEnemies = GameObject.FindGameObjectWithTag("RangedRoot");

        for (int i = 0; i < rangedEnemies.transform.childCount; i++)
        {
            if ( i > levelSaveData.planetLevelData.rangedEnemies.Count-1)
            {
                Destroy(rangedEnemies.transform.GetChild(i).gameObject);
                continue;
            }

            RangedSaveData enemySavaData = levelSaveData.planetLevelData.rangedEnemies[i];
            GameObject go = rangedEnemies.transform.GetChild(i).gameObject;

            Unit unit = go.GetComponent<Unit>();

            unit.Health = enemySavaData.health;
            go.transform.position = enemySavaData.position;
        }
    }

    public void SetUpSpaceLevel(LevelSaveData levelSaveData)
    {
        
    }
}
