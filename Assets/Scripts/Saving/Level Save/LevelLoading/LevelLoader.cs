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

        //double check that the levelsavedata is not just the default data
        if (SaveManager.instance.CurrentLevelSaveData.isPlanetLevel)
        {
            if (SaveManager.instance.CurrentLevelSaveData.planetLevelData.playerPosition == new Vector3(0,0,0) && SaveManager.instance.CurrentLevelSaveData.planetLevelData.meleeEnemies.Count < 1)
            {
                return;
            }

        }
        else
        {
            if (SaveManager.instance.CurrentLevelSaveData.spaceLevelData.enemyFlagshipData.position == new Vector3(0,0,0) && SaveManager.instance.CurrentLevelSaveData.spaceLevelData.playerPosition == new Vector3(0,0,0))
            {
                return;
            }
        }

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
        else
        {
            SetUpSpaceLevel(levelSaveData);
        }
    }

    public void SetUpPlanetLevel(LevelSaveData levelSaveData)
    {
        if (SceneManager.GetActiveScene().name == "PCG_Sample")
        {
            GroundLevelGenerator groundLevelGenerator = FindFirstObjectByType<GroundLevelGenerator>();
            groundLevelGenerator.snapPlayerToStartChunk = false;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = levelSaveData.planetLevelData.playerPosition;
            player.GetComponent<Unit>().Health = levelSaveData.planetLevelData.playerHealth;
            return;
        }

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        playerGO.transform.position = levelSaveData.planetLevelData.playerPosition;
        playerGO.GetComponent<Unit>().Health = levelSaveData.planetLevelData.playerHealth;

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

        // set scrap count
        Inventory playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();

        for (int i = 0; i < SaveManager.instance.CurrentLevelSaveData.scrapCount; i++)
        {
            playerInventory.GiveScrap();
        }
    }

    public void SetUpSpaceLevel(LevelSaveData levelSaveData)
    {
        print($"setting up space level {SceneManager.GetActiveScene().name}");
        // set player data
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        playerGO.transform.position = levelSaveData.spaceLevelData.playerPosition;

        SpaceshipHealthComponent playerHealth = playerGO.GetComponent<SpaceshipHealthComponent>();
        playerHealth.SetShipHealth(levelSaveData.spaceLevelData.playerHealth);
        playerHealth.SetShipShield(levelSaveData.spaceLevelData.playerShield);

        // set flagship data
        GameObject[] flagships = GameObject.FindGameObjectsWithTag("Flagship");
        print(flagships[0].name);
        flagships[1].transform.position = levelSaveData.spaceLevelData.enemyFlagshipData.position;
        flagships[1].GetComponent<SpaceshipHealthComponent>().SetShipHealth(levelSaveData.spaceLevelData.enemyFlagshipData.health);
        flagships[1].GetComponent<SpaceshipHealthComponent>().SetShipShield(levelSaveData.spaceLevelData.enemyFlagshipData.shield);
        
        flagships[0].transform.position = levelSaveData.spaceLevelData.allyFlagshipData.position;
        flagships[0].GetComponent<SpaceshipHealthComponent>().SetShipHealth(levelSaveData.spaceLevelData.allyFlagshipData.health);
        flagships[0].GetComponent<SpaceshipHealthComponent>().SetShipShield(levelSaveData.spaceLevelData.allyFlagshipData.shield);

        FlagshipShieldNode[] enemyShieldNodes = flagships[1].GetComponentsInChildren<FlagshipShieldNode>();
        FlagshipShieldNode[] allyShieldNodes = flagships[0].GetComponentsInChildren<FlagshipShieldNode>();
        for (int i = 0; i < 3; i++)
        {
            enemyShieldNodes[i].TakeDamage(200-levelSaveData.spaceLevelData.enemyFlagshipData.shieldNodes[i]);
            allyShieldNodes[i].TakeDamage(200-levelSaveData.spaceLevelData.allyFlagshipData.shieldNodes[i]);
        }


    }
}
