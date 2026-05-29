using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;
using System.IO;
using Unity.VisualScripting;
using System.Linq;
using UnityEditor;

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

            if (repairedMissingSpaceshipUpgradeData || repairedMissingGroundUpgradeData || repairedMissingGroundEquipmentData || upgradedSaveVersion)
            {
                Debug.LogWarning($"Save file at {SaveFilePath} was missing migrated data or was on an older version. Rewriting it with the current schema.");
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

    public void UpdateLevelData()
    {
        if (!IsInLevel)
        {
            print("not in level, not saving level data");
            return;
        }
        
        if (CurrentLevelSaveData.isPlanetLevel)
        {
            // generate new planet save data, use method to update data
            PlanetLevelData planetLevelData = new PlanetLevelData();

            planetLevelData.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

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

        }
        else
        {
            
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

    public void AddUpgradeLevel(PlayerUpgradeState.UpgradeType upgradeType)
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData == null) return;

        switch (upgradeType)
        {
            case PlayerUpgradeState.UpgradeType.MoveForce:
                CurrentSaveData.spaceshipUpgradeData.moveForceLevel++;
                break;
            case PlayerUpgradeState.UpgradeType.MaxSpeed:
                CurrentSaveData.spaceshipUpgradeData.maxSpeedLevel++;
                break;
            case PlayerUpgradeState.UpgradeType.BoostForce:
                CurrentSaveData.spaceshipUpgradeData.boostForceLevel++;
                break;
            case PlayerUpgradeState.UpgradeType.BarrelRollSpeed:
                CurrentSaveData.spaceshipUpgradeData.barrelRollSpeedLevel++;
                break;
            case PlayerUpgradeState.UpgradeType.BarrelRollDistance:
                CurrentSaveData.spaceshipUpgradeData.barrelRollDistanceLevel++;
                break;
            case PlayerUpgradeState.UpgradeType.FireRate:
                CurrentSaveData.spaceshipUpgradeData.fireRateLevel++;
                break;
            case PlayerUpgradeState.UpgradeType.MaxHealth:
                CurrentSaveData.spaceshipUpgradeData.maxHealthLevel++;
                break;
            case PlayerUpgradeState.UpgradeType.MaxShields:
                CurrentSaveData.spaceshipUpgradeData.maxShieldsLevel++;
                break;
        }

        MakeDirty();
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

    public int GetGroundUpgradeCost(GroundTrooperUpgradeType upgradeType)
    {
        EnsureCurrentSaveData();
        if (defualtGameSaveSO?.groundTrooperDefaults == null)
            return 0;

        return defualtGameSaveSO.groundTrooperDefaults.universalUpgradeCostPerLevel;
    }

    public void AddGroundUpgradeLevel(GroundTrooperUpgradeType upgradeType)
    {
        EnsureCurrentSaveData();
        if (CurrentSaveData?.groundTrooperUpgradeData == null)
            return;

        switch (upgradeType)
        {
            case GroundTrooperUpgradeType.MoveSpeed:
                CurrentSaveData.groundTrooperUpgradeData.moveSpeedLevel++;
                break;
            case GroundTrooperUpgradeType.JumpVelocity:
                CurrentSaveData.groundTrooperUpgradeData.jumpVelocityLevel++;
                break;
            case GroundTrooperUpgradeType.MaxHealth:
                CurrentSaveData.groundTrooperUpgradeData.maxHealthLevel++;
                break;
        }

        MakeDirty();
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

        if (CurrentLevelSaveData == null)
        {
            CurrentLevelSaveData = LevelSaveData.CreateDefaultSaveData();
        }
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
