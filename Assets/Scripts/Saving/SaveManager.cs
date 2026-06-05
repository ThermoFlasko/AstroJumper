using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;
using System.IO;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance { get; private set; }
    public static event Action<int> NewMoneyChanged;

    [Header("Defualts + Files")]
    [SerializeField]
    private DefualtGameSaveSO defualtGameSaveSO;
    private DefaultLevelSaveSO defaultLevelSaveSO;

    [SerializeField] private string saveFolderName = "Saves";
    [SerializeField] private string saveFileName = "savegame.json";
    [SerializeField] private string saveLevelFileName = "savelevel.json";

    private const string LegacySaveFileName = "importantYAaaaa.json";

    private string SaveDirectoryPath => Path.Combine(Application.persistentDataPath, saveFolderName);
    private string SaveFilePath => Path.Combine(SaveDirectoryPath, saveFileName);
    private string SaveLevelFilePath => Path.Combine(SaveDirectoryPath, saveLevelFileName);

    private string LegacySaveFilePath => Path.Combine(Application.persistentDataPath, LegacySaveFileName);

    public SaveData CurrentSaveData { get; private set; }
    public LevelSaveData CurrentLevelSaveData {get; private set;}
    public DefualtGameSaveSO DefaultGameSaveSO => defualtGameSaveSO;


    [SerializeField] private float dirtyDelaySaveTime = 2.0f;

    private bool dirty;
    private float dirtyTimer;

    [Header("AutoSave")][SerializeField] private bool autoSave = true;
    [FormerlySerializedAs("autoSaveDelay")]
    [SerializeField] private float autoSaveDelaySaveTime = 10.0f;

    [Header("Debug")]
    [SerializeField] private bool enableMoneyTestHotkey = true;
    [SerializeField] private KeyCode addMoneyTestKey = KeyCode.M;
    [SerializeField] private int addMoneyTestAmount = 100;

    [Header("Level Saving")]
    public bool isLoadingSaveData = false;
    public bool IsInLevel = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        Debug.Log($"Save root: {Application.persistentDataPath}");
        LoadGame();

        if (autoSave)
        {
            StartCoroutine(AutoSave());
        }
    }

    private IEnumerator AutoSave()
    {
        while (autoSave)
        {
            yield return new WaitForSeconds(autoSaveDelaySaveTime);
            if (dirty)
            {
                print("saving");
                SaveGame();
            }
        }
    }

    private void Update()
    {
        HandleDebugHotkeys();

        if (!autoSave || !dirty) return;
        dirtyTimer += Time.unscaledDeltaTime;
        if (dirtyTimer >= dirtyDelaySaveTime)
        {
            SaveGame();
        }
    }

    private void MakeDirty()
    {
        dirty = true;
        dirtyTimer = 0f;
    }

    private void HandleDebugHotkeys()
    {
        if (!enableMoneyTestHotkey || !Input.GetKeyDown(addMoneyTestKey))
            return;

        AddNewMoney(addMoneyTestAmount);
        Debug.Log($"Save test hotkey pressed. Added {addMoneyTestAmount} money. Current money is now {CurrentSaveData.newMoney}.");
    }

    private void LoadGame()
    {
        TryMigrateLegacySaveFile();

        if (!File.Exists(SaveFilePath) || !File.Exists(SaveLevelFilePath))
        {
            CreateDefaultSaveFile("No save file found.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            bool repairedMissingSpaceshipUpgradeData = !json.Contains("\"spaceshipUpgradeData\"");
            bool repairedMissingGroundUpgradeData = !json.Contains("\"groundTrooperUpgradeData\"");
            bool repairedMissingGroundEquipmentData = !json.Contains("\"groundEquipmentData\"");

            CurrentSaveData = JsonUtility.FromJson<SaveData>(json);

            if (CurrentSaveData == null)
            {
                CreateDefaultSaveFile($"Save file was empty or invalid JSON at {SaveFilePath}.");
                return;
            }

            bool upgradedSaveVersion = CurrentSaveData.version < SaveData.CurrentVersion;
            CurrentSaveData.EnsureInitialized(defualtGameSaveSO);
            CurrentSaveData.version = SaveData.CurrentVersion;
            bool clampedUpgradeLevels = ClampUpgradeLevelsToCaps();

            if (repairedMissingSpaceshipUpgradeData || repairedMissingGroundUpgradeData || repairedMissingGroundEquipmentData || upgradedSaveVersion || clampedUpgradeLevels)
            {
                Debug.LogWarning($"Save file at {SaveFilePath} was missing migrated data, was on an older version, or had upgrade levels outside their caps. Rewriting it with the current schema.");
                WriteToDisk();
            }
        }
        catch (Exception ex)
        {
            CreateDefaultSaveFile($"Failed reading save file at {SaveFilePath}. {ex.Message}");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveLevelFilePath);
            CurrentLevelSaveData = JsonUtility.FromJson<LevelSaveData>(json);

            if (CurrentLevelSaveData == null)
            {
                CreateDefaultSaveFile($"Save file was empty or invalid JSON at {SaveLevelFilePath}.");
                return;
            }

            // CurrentLevelSaveData.EnsureInitialized()

            // bool upgradedSaveVersion = CurrentLevelSaveData.version < SaveData.CurrentVersion;
            // CurrentLevelSaveData.EnsureInitialized(defualtGameSaveSO);
            // CurrentSaveData.version = SaveData.CurrentVersion;

        }
        catch (Exception ex)
        {
            CreateDefaultSaveFile($"Failed reading save file at {SaveLevelFilePath}. {ex.Message}");
            return;
        }

        NotifyMoneyChanged();
    }

    public void SaveGame()
    {
        EnsureCurrentSaveData();

        dirty = false;
        dirtyTimer = 0f;

        UpdateLevelData();

        WriteToDisk();
    }

    private void WriteToDisk()
    {
        EnsureCurrentSaveData();

        try
        {
            Directory.CreateDirectory(SaveDirectoryPath);
            string json = JsonUtility.ToJson(CurrentSaveData, true);
            string tempSavePath = SaveFilePath + ".tmp";
            File.WriteAllText(tempSavePath, json);

            if (File.Exists(SaveFilePath))
            {
                File.Copy(tempSavePath, SaveFilePath, true);
                File.Delete(tempSavePath);
            }
            else
            {
                File.Move(tempSavePath, SaveFilePath);
            }

            Debug.Log($"Saved game to {SaveFilePath}. Money={CurrentSaveData.newMoney}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed writing save file at {SaveFilePath}. {ex}");
        }

        // for level save data
        try
        {
            string json = JsonUtility.ToJson(CurrentLevelSaveData, true);
            string tempSavePath = SaveLevelFilePath + ".tmp";
            File.WriteAllText(tempSavePath, json);

            if (File.Exists(SaveLevelFilePath))
            {
                File.Copy(tempSavePath, SaveLevelFilePath, true);
                File.Delete(tempSavePath);
            }
            else
            {
                File.Move(tempSavePath, SaveLevelFilePath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed writing save file at {SaveLevelFilePath}. {ex}");
        }

    }

    private void OnDisable()
    {
        if (instance == this && dirty)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && dirty) SaveGame();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && dirty) SaveGame();
    }

    #region Helper Functions

    public int GetNewMoney()
    {
        EnsureCurrentSaveData();
        return (CurrentSaveData != null) ? CurrentSaveData.newMoney : 0;
    }

    public void SetNewMoney(int newMoney)
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData == null) return;

        CurrentSaveData.newMoney = newMoney;
        NotifyMoneyChanged();
        MakeDirty();
    }

    public void AddNewMoney(int amount)
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData == null) return;

        CurrentSaveData.newMoney += amount;
        NotifyMoneyChanged();
        MakeDirty();
    }

    public void ResetSave()
    {
        CurrentSaveData = SaveData.CreateDefualtSaveData(defualtGameSaveSO);

        LevelSaveData newSaveData = LevelSaveData.CreateDefaultSaveData();
        newSaveData.currLevel = "Tutorial Ground";
        newSaveData.isPlanetLevel = true;
        CurrentLevelSaveData = newSaveData;

        isLoadingSaveData = false;
        NotifyMoneyChanged();
        MakeDirty();
    }

    public void UpdateLevelData()
    {
        if (!IsInLevel)
        {
            print("not in level, not saving level data");
            return;
        }
        
        CurrentLevelSaveData.currLevel = SceneManager.GetActiveScene().name;
        if (CurrentLevelSaveData.isPlanetLevel)
        {
            // check if pcg level
            if (SceneManager.GetActiveScene().name == "PCG_Sample")
            {
                GroundLevelGenerator groundLevelGenerator = FindAnyObjectByType<GroundLevelGenerator>();

                CurrentLevelSaveData.planetLevelData.PCGSeed = groundLevelGenerator.GetSeed();
                print($"got pcg level seed {CurrentLevelSaveData.planetLevelData.PCGSeed}");

                CurrentLevelSaveData.planetLevelData.playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Unit>().Health;
                CurrentLevelSaveData.planetLevelData.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

                Inventory inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();

                CurrentLevelSaveData.scrapCount = inventory.GetScrapCount();
                return;
            }

            // generate new planet save data, use method to update data
            PlanetLevelData planetLevelData = new PlanetLevelData();

            planetLevelData.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
            planetLevelData.playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Unit>().Health;

            GameObject meleeEnemies = GameObject.FindGameObjectWithTag("MeleeRoot");

            print(meleeEnemies.transform.GetChild(0));
            
            foreach (Transform child in meleeEnemies.transform)
            {
                MeleeSaveData saveData = new MeleeSaveData();
                saveData = (MeleeSaveData)LoadEnemyData(saveData, child.gameObject);
                planetLevelData.meleeEnemies.Add(saveData);
            }

            GameObject rangedEnemies = GameObject.FindGameObjectWithTag("RangedRoot");

            foreach (Transform child in rangedEnemies.transform)
            {
                RangedSaveData saveData = new RangedSaveData();
                saveData = (RangedSaveData)LoadEnemyData(saveData, child.gameObject);
                planetLevelData.rangedEnemies.Add(saveData);
            }

            CurrentLevelSaveData.UpdatePlanetLevelData(planetLevelData);
            
            Inventory playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();

            CurrentLevelSaveData.scrapCount = playerInventory.GetScrapCount();

        }
        else
        {
            // fill out player data
            SpaceLevelData spaceLevelData = new SpaceLevelData();

            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");

            spaceLevelData.playerPosition = playerGO.transform.position;

            spaceLevelData.playerHealth = playerGO.GetComponent<SpaceshipHealthComponent>().GetShipHealth();
            spaceLevelData.playerShield = playerGO.GetComponent<SpaceshipHealthComponent>().GetShipShield();

            print(playerGO.GetComponent<SpaceshipHealthComponent>().GetShipHealth());

            // fill out flagship data
            FlagShipData allyFlagShipData = new FlagShipData();
            FlagShipData enemyFlagShipData = new FlagShipData();

            FlagshipController[] flagships = FindObjectsByType<FlagshipController>(FindObjectsSortMode.InstanceID);

            enemyFlagShipData.position = flagships[0].gameObject.transform.position;
            enemyFlagShipData.health = flagships[0].gameObject.GetComponent<SpaceshipHealthComponent>().GetShipHealth();
            enemyFlagShipData.shield = flagships[0].gameObject.GetComponent<SpaceshipHealthComponent>().GetShipShield();
            allyFlagShipData.position = flagships[1].gameObject.transform.position;
            allyFlagShipData.health = flagships[1].gameObject.GetComponent<SpaceshipHealthComponent>().GetShipHealth();
            allyFlagShipData.shield = flagships[1].gameObject.GetComponent<SpaceshipHealthComponent>().GetShipShield();

            FlagshipShieldNode[] enemyNodes = flagships[0].gameObject.GetComponentsInChildren<FlagshipShieldNode>();
            FlagshipShieldNode[] allyNodes = flagships[1].gameObject.GetComponentsInChildren<FlagshipShieldNode>();

            for (int i = 0; i < enemyNodes.Length; i++)
            {
                enemyFlagShipData.shieldNodes[i] = enemyNodes[i].GetNodeHealth();
            }

            for (int i = 0; i < allyNodes.Length; i++)
            {
                allyFlagShipData.shieldNodes[i] = allyNodes[i].GetNodeHealth();
            }

            spaceLevelData.allyFlagshipData = allyFlagShipData;
            spaceLevelData.enemyFlagshipData = enemyFlagShipData;
 
            
            print(flagships[0].name);

            CurrentLevelSaveData.UpdateSpaceLevelData(spaceLevelData);
            print(CurrentLevelSaveData.spaceLevelData.allyFlagshipData.health);
        }
    }

    public int GetUpgradeLevel(PlayerUpgradeState.UpgradeType upgradeType)
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData == null) return 0;

        switch (upgradeType)
        {
            case PlayerUpgradeState.UpgradeType.MoveForce:
                return CurrentSaveData.spaceshipUpgradeData.moveForceLevel;
            case PlayerUpgradeState.UpgradeType.MaxSpeed:
                return CurrentSaveData.spaceshipUpgradeData.maxSpeedLevel;
            case PlayerUpgradeState.UpgradeType.BoostForce:
                return CurrentSaveData.spaceshipUpgradeData.boostForceLevel;
            case PlayerUpgradeState.UpgradeType.BarrelRollDistance:
                return CurrentSaveData.spaceshipUpgradeData.barrelRollDistanceLevel;
            case PlayerUpgradeState.UpgradeType.BarrelRollSpeed:
                return CurrentSaveData.spaceshipUpgradeData.barrelRollSpeedLevel;
            case PlayerUpgradeState.UpgradeType.FireRate:
                return CurrentSaveData.spaceshipUpgradeData.fireRateLevel;
            case PlayerUpgradeState.UpgradeType.MaxHealth:
                return CurrentSaveData.spaceshipUpgradeData.maxHealthLevel;
            case PlayerUpgradeState.UpgradeType.MaxShields:
                return CurrentSaveData.spaceshipUpgradeData.maxShieldsLevel;

        }

        return 0;
    }

    public int GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType upgradeType)
    {
        EnsureCurrentSaveData();

        PlayerSpaceshipUpgradesSO upgrades = defualtGameSaveSO != null ? defualtGameSaveSO.playerSpaceshipUpgradesSO : null;
        return upgrades != null ? upgrades.GetMaxUpgradeLevel(upgradeType) : int.MaxValue;
    }

    public bool IsUpgradeAtMaxLevel(PlayerUpgradeState.UpgradeType upgradeType)
    {
        return GetUpgradeLevel(upgradeType) >= GetMaxUpgradeLevel(upgradeType);
    }

    public bool TryAddUpgradeLevel(PlayerUpgradeState.UpgradeType upgradeType)
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData == null || IsUpgradeAtMaxLevel(upgradeType))
            return false;

        int maxLevel = GetMaxUpgradeLevel(upgradeType);

        switch (upgradeType)
        {
            case PlayerUpgradeState.UpgradeType.MoveForce:
                CurrentSaveData.spaceshipUpgradeData.moveForceLevel = Mathf.Min(CurrentSaveData.spaceshipUpgradeData.moveForceLevel + 1, maxLevel);
                break;
            case PlayerUpgradeState.UpgradeType.MaxSpeed:
                CurrentSaveData.spaceshipUpgradeData.maxSpeedLevel = Mathf.Min(CurrentSaveData.spaceshipUpgradeData.maxSpeedLevel + 1, maxLevel);
                break;
            case PlayerUpgradeState.UpgradeType.BoostForce:
                CurrentSaveData.spaceshipUpgradeData.boostForceLevel = Mathf.Min(CurrentSaveData.spaceshipUpgradeData.boostForceLevel + 1, maxLevel);
                break;
            case PlayerUpgradeState.UpgradeType.BarrelRollSpeed:
                CurrentSaveData.spaceshipUpgradeData.barrelRollSpeedLevel = Mathf.Min(CurrentSaveData.spaceshipUpgradeData.barrelRollSpeedLevel + 1, maxLevel);
                break;
            case PlayerUpgradeState.UpgradeType.BarrelRollDistance:
                CurrentSaveData.spaceshipUpgradeData.barrelRollDistanceLevel = Mathf.Min(CurrentSaveData.spaceshipUpgradeData.barrelRollDistanceLevel + 1, maxLevel);
                break;
            case PlayerUpgradeState.UpgradeType.FireRate:
                CurrentSaveData.spaceshipUpgradeData.fireRateLevel = Mathf.Min(CurrentSaveData.spaceshipUpgradeData.fireRateLevel + 1, maxLevel);
                break;
            case PlayerUpgradeState.UpgradeType.MaxHealth:
                CurrentSaveData.spaceshipUpgradeData.maxHealthLevel = Mathf.Min(CurrentSaveData.spaceshipUpgradeData.maxHealthLevel + 1, maxLevel);
                break;
            case PlayerUpgradeState.UpgradeType.MaxShields:
                CurrentSaveData.spaceshipUpgradeData.maxShieldsLevel = Mathf.Min(CurrentSaveData.spaceshipUpgradeData.maxShieldsLevel + 1, maxLevel);
                break;
        }

        MakeDirty();
        return true;
    }

    public void AddUpgradeLevel(PlayerUpgradeState.UpgradeType upgradeType)
    {
        TryAddUpgradeLevel(upgradeType);
    }

    public float GetGroundMoveSpeedUpgradeBoost()
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData?.groundTrooperUpgradeData == null || defualtGameSaveSO == null)
            return 0f;

        return CurrentSaveData.groundTrooperUpgradeData.moveSpeedLevel *
               defualtGameSaveSO.groundTrooperDefaults.moveSpeedUpgradePerLevel;
    }

    public float GetGroundJumpVelocityUpgradeBoost()
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData?.groundTrooperUpgradeData == null || defualtGameSaveSO == null)
            return 0f;

        return CurrentSaveData.groundTrooperUpgradeData.jumpVelocityLevel *
               defualtGameSaveSO.groundTrooperDefaults.jumpVelocityUpgradePerLevel;
    }

    public int GetGroundMaxHealthUpgradeBoost()
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData?.groundTrooperUpgradeData == null || defualtGameSaveSO == null)
            return 0;

        return CurrentSaveData.groundTrooperUpgradeData.maxHealthLevel *
               defualtGameSaveSO.groundTrooperDefaults.maxHealthUpgradePerLevel;
    }

    public int GetGroundUpgradeLevel(GroundTrooperUpgradeType upgradeType)
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData?.groundTrooperUpgradeData == null)
            return 0;

        switch (upgradeType)
        {
            case GroundTrooperUpgradeType.MoveSpeed:
                return CurrentSaveData.groundTrooperUpgradeData.moveSpeedLevel;
            case GroundTrooperUpgradeType.JumpVelocity:
                return CurrentSaveData.groundTrooperUpgradeData.jumpVelocityLevel;
            case GroundTrooperUpgradeType.MaxHealth:
                return CurrentSaveData.groundTrooperUpgradeData.maxHealthLevel;
        }

        return 0;
    }

    public int GetGroundMaxUpgradeLevel(GroundTrooperUpgradeType upgradeType)
    {
        EnsureCurrentSaveData();

        GroundTrooperUpgradeDefaults groundDefaults = defualtGameSaveSO != null ? defualtGameSaveSO.groundTrooperDefaults : null;
        return groundDefaults != null ? groundDefaults.GetMaxUpgradeLevel(upgradeType) : int.MaxValue;
    }

    public bool IsGroundUpgradeAtMaxLevel(GroundTrooperUpgradeType upgradeType)
    {
        return GetGroundUpgradeLevel(upgradeType) >= GetGroundMaxUpgradeLevel(upgradeType);
    }

    public int GetGroundUpgradeCost(GroundTrooperUpgradeType upgradeType)
    {
        EnsureCurrentSaveData();
        if (defualtGameSaveSO?.groundTrooperDefaults == null)
            return 0;

        return defualtGameSaveSO.groundTrooperDefaults.universalUpgradeCostPerLevel;
    }

    public bool TryAddGroundUpgradeLevel(GroundTrooperUpgradeType upgradeType)
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData?.groundTrooperUpgradeData == null || IsGroundUpgradeAtMaxLevel(upgradeType))
            return false;

        int maxLevel = GetGroundMaxUpgradeLevel(upgradeType);

        switch (upgradeType)
        {
            case GroundTrooperUpgradeType.MoveSpeed:
                CurrentSaveData.groundTrooperUpgradeData.moveSpeedLevel = Mathf.Min(CurrentSaveData.groundTrooperUpgradeData.moveSpeedLevel + 1, maxLevel);
                break;
            case GroundTrooperUpgradeType.JumpVelocity:
                CurrentSaveData.groundTrooperUpgradeData.jumpVelocityLevel = Mathf.Min(CurrentSaveData.groundTrooperUpgradeData.jumpVelocityLevel + 1, maxLevel);
                break;
            case GroundTrooperUpgradeType.MaxHealth:
                CurrentSaveData.groundTrooperUpgradeData.maxHealthLevel = Mathf.Min(CurrentSaveData.groundTrooperUpgradeData.maxHealthLevel + 1, maxLevel);
                break;
        }

        MakeDirty();
        return true;
    }

    public void AddGroundUpgradeLevel(GroundTrooperUpgradeType upgradeType)
    {
        TryAddGroundUpgradeLevel(upgradeType);
    }

    public string GetEquippedGroundAttackId(GroundAttackType attackType)
    {
        EnsureCurrentSaveData();

        if (CurrentSaveData?.groundEquipmentData == null)
            return string.Empty;

        return attackType == GroundAttackType.Melee ? CurrentSaveData.groundEquipmentData.equippedMeleeAttackId : CurrentSaveData.groundEquipmentData.equippedRangedAttackId;
    }

    public void SetEquippedGroundAttackId
(GroundAttackType attackType, string attackId)
    {
        EnsureCurrentSaveData();

        if (CurrentSaveData?.groundEquipmentData == null)
            return;

        if (attackType == GroundAttackType.Melee)
            CurrentSaveData.groundEquipmentData.equippedMeleeAttackId = attackId;
        else
            CurrentSaveData.groundEquipmentData.equippedRangedAttackId = attackId;

        MakeDirty();
    }

    public LevelSaveData GetCurrentLevelData()
    {
        return CurrentLevelSaveData;
    }

    public EnemySaveData LoadEnemyData(EnemySaveData data, GameObject go)
    {
        EnemySaveData saveData;
        if (data is MeleeSaveData)
        {
            saveData = new MeleeSaveData();

            Unit unit = go.GetComponent<Unit>();
            saveData.health = unit.Health;

            saveData.position = go.transform.position;
        }
        else if (data is RangedSaveData)
        {
            saveData = new RangedSaveData();

            Unit unit = go.GetComponent<Unit>();
            saveData.health = unit.Health;

            saveData.position = go.transform.position;
        }
        else
        {
            // filler
            saveData = new MeleeSaveData();

        }


        return saveData;
    }

    #endregion

    private void EnsureCurrentSaveData()
    {
        if (CurrentSaveData == null)
        {
            CurrentSaveData = SaveData.CreateDefualtSaveData(defualtGameSaveSO);
        }

        CurrentSaveData.EnsureInitialized(defualtGameSaveSO);
        CurrentSaveData.version = SaveData.CurrentVersion;

        if (ClampUpgradeLevelsToCaps())
        {
            MakeDirty();
        }

        if (CurrentLevelSaveData == null)
        {
            CurrentLevelSaveData = LevelSaveData.CreateDefaultSaveData();
        }
    }

    private bool ClampUpgradeLevelsToCaps()
    {
        bool changed = false;

        PlayerSpaceshipUpgradesSO spaceshipUpgrades = defualtGameSaveSO != null ? defualtGameSaveSO.playerSpaceshipUpgradesSO : null;
        if (CurrentSaveData?.spaceshipUpgradeData != null && spaceshipUpgrades != null)
        {
            SaveData.SpaceshipUpgradeData data = CurrentSaveData.spaceshipUpgradeData;
            changed |= ClampUpgradeLevel(ref data.moveForceLevel, spaceshipUpgrades.GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType.MoveForce));
            changed |= ClampUpgradeLevel(ref data.maxSpeedLevel, spaceshipUpgrades.GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType.MaxSpeed));
            changed |= ClampUpgradeLevel(ref data.boostForceLevel, spaceshipUpgrades.GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType.BoostForce));
            changed |= ClampUpgradeLevel(ref data.barrelRollDistanceLevel, spaceshipUpgrades.GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType.BarrelRollDistance));
            changed |= ClampUpgradeLevel(ref data.barrelRollSpeedLevel, spaceshipUpgrades.GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType.BarrelRollSpeed));
            changed |= ClampUpgradeLevel(ref data.fireRateLevel, spaceshipUpgrades.GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType.FireRate));
            changed |= ClampUpgradeLevel(ref data.maxHealthLevel, spaceshipUpgrades.GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType.MaxHealth));
            changed |= ClampUpgradeLevel(ref data.maxShieldsLevel, spaceshipUpgrades.GetMaxUpgradeLevel(PlayerUpgradeState.UpgradeType.MaxShields));
        }

        GroundTrooperUpgradeDefaults groundDefaults = defualtGameSaveSO != null ? defualtGameSaveSO.groundTrooperDefaults : null;
        if (CurrentSaveData?.groundTrooperUpgradeData != null && groundDefaults != null)
        {
            SaveData.GroundTrooperUpgradeData data = CurrentSaveData.groundTrooperUpgradeData;
            changed |= ClampUpgradeLevel(ref data.moveSpeedLevel, groundDefaults.GetMaxUpgradeLevel(GroundTrooperUpgradeType.MoveSpeed));
            changed |= ClampUpgradeLevel(ref data.jumpVelocityLevel, groundDefaults.GetMaxUpgradeLevel(GroundTrooperUpgradeType.JumpVelocity));
            changed |= ClampUpgradeLevel(ref data.maxHealthLevel, groundDefaults.GetMaxUpgradeLevel(GroundTrooperUpgradeType.MaxHealth));
        }

        return changed;
    }

    private static bool ClampUpgradeLevel(ref int level, int maxLevel)
    {
        int clampedLevel = Mathf.Clamp(level, 0, Mathf.Max(0, maxLevel));
        if (level == clampedLevel)
            return false;

        level = clampedLevel;
        return true;
    }

    private void CreateDefaultSaveFile(string reason)
    {
        CurrentSaveData = SaveData.CreateDefualtSaveData(defualtGameSaveSO);
        Debug.LogWarning($"{reason} Created a new save file at {SaveFilePath}.");
        WriteToDisk();
        NotifyMoneyChanged();
    }

    private void TryMigrateLegacySaveFile()
    {
        if (File.Exists(SaveFilePath) || !File.Exists(LegacySaveFilePath))
            return;

        try
        {
            Directory.CreateDirectory(SaveDirectoryPath);
            File.Copy(LegacySaveFilePath, SaveFilePath);
            Debug.Log($"Migrated legacy save file from {LegacySaveFilePath} to {SaveFilePath}.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to migrate legacy save file from {LegacySaveFilePath} to {SaveFilePath}. {ex.Message}");
        }
    }

    private void NotifyMoneyChanged()
    {
        if (CurrentSaveData == null)
            return;

        NewMoneyChanged?.Invoke(CurrentSaveData.newMoney);
    }

    [ContextMenu("Debug Equip Green Blast Melee")]
    private void DebugEquipGreenBlastMelee()
    {
        SetEquippedGroundAttackId(GroundAttackType.Melee, "Green Blast");
        SaveGame();
    }

    [ContextMenu("Debug Clear Ground Equipment")]
    private void DebugClearGroundEquipment()
    {
        SetEquippedGroundAttackId(GroundAttackType.Melee, string.Empty);
        SetEquippedGroundAttackId(GroundAttackType.Ranged, string.Empty);
        SaveGame();
    }
}
