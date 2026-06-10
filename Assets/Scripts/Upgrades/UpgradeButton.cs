using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine;
using UnityEngine.UI;

public enum UpgradeButtonType
{
    MoveForce = 0,
    MaxSpeed = 1,
    BoostForce = 2,
    BarrelRollDistance = 3,
    BarrelRollSpeed = 4,
    FireRate = 5,
    MaxHealth = 6,
    MaxShields = 7,
    PlayerMoveSpeed = 8,
    PlayerJumpVelocity = 9,
    PlayerMaxHealth = 10,
}

public class UpgradeButton : MonoBehaviour
{
    private const string UiTextTable = "UI Text";
    private const string CostLabelKey = "UPGRADES_LABEL_COST";
    private const string CurrentValueLabelKey = "UPGRADES_LABEL_NEWCOST";
    private const string LevelShortLabelKey = "UPGRADES_LABEL_LEVEL_TRUNCATED";
    private const string NextLabelKey = "UPGRADES_LABEL_NEXT";
    private const string MaxLabelKey = "UPGRADES_LABEL_MAX";
    private const string BaseValueKey = "UPGRADES_VALUE_BASE";

    [Header("References")] [SerializeField]
    private UpgradeButtonType upgradeType;

    [Header("Buttons")] [SerializeField] private PlayerSpaceshipUpgradesSO upgradesSO;
    [SerializeField] private DefualtGameSaveSO defaultGameSaveSO;
    [SerializeField] private GameObject upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeLabel;
    [SerializeField] private TextMeshProUGUI upgradeCostLabel;
    [SerializeField] private TextMeshProUGUI currentLevelLabel;

    [Header("Editor Preview")] [SerializeField]
    private bool previewInEditor = true;

    [SerializeField] [Min(0)] private int editorPreviewLevel = 1;
    [SerializeField] [Min(0)] private int editorPreviewMoney = 999;

    private Button cachedButton;

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        SaveManager.NewMoneyChanged += HandleMoneyChanged;
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
        RefreshView();
    }

    private void OnDisable()
    {
        SaveManager.NewMoneyChanged -= HandleMoneyChanged;
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    public void Start()
    {
        RefreshView();
    }

    public void PrintToConsole()
    {
        print($"Player clickd on {upgradeType} type button.");

        if (SaveManager.instance == null || !HasRequiredData())
        {
            RefreshView();
            return;
        }

        int upgradeCost = GetUpgradeCost();
        int currentLevel = GetCurrentUpgradeLevel();
        int maxLevel = GetMaxUpgradeLevel();
        if (upgradeCost >= 0 && !IsAtMaxLevel(currentLevel, maxLevel) && SaveManager.instance.GetNewMoney() >= upgradeCost)
        {
            print("can afford upgrade");
            bool upgraded = IsSpaceshipUpgrade()
                ? SaveManager.instance.TryAddUpgradeLevel(ToSpaceshipUpgradeType())
                : SaveManager.instance.TryAddGroundUpgradeLevel(ToGroundUpgradeType());

            if (upgraded)
                SaveManager.instance.AddNewMoney(-upgradeCost);
        }

        RefreshView();
    }

    public void RefreshView()
    {
        CacheReferences();

        if (upgradeLabel != null)
            upgradeLabel.text = GetUpgradeDisplayName();

        if (!Application.isPlaying)
        {
            RefreshEditorPreview();
            return;
        }

        if (SaveManager.instance == null || !HasRequiredData())
        {
            ApplyUnavailableState(0);
            return;
        }

        int upgradeCost = GetUpgradeCost();
        int currentLevel = GetCurrentUpgradeLevel();
        int maxLevel = GetMaxUpgradeLevel();
        int currentMoney = SaveManager.instance.GetNewMoney();
        string currentSummary = GetUpgradeSummary(currentLevel);
        string nextSummary = IsAtMaxLevel(currentLevel, maxLevel) ? string.Empty : GetUpgradeSummary(currentLevel + 1);

        ApplyButtonState(upgradeCost, currentLevel, maxLevel, currentMoney, currentSummary, nextSummary);
    }

    private void HandleMoneyChanged(int _)
    {
        RefreshView();
    }

    private void HandleLocaleChanged(Locale _)
    {
        RefreshView();
    }

    [ContextMenu("Refresh Preview")]
    private void RefreshPreviewFromContextMenu()
    {
        RefreshView();
    }

    private void CacheReferences()
    {
        if (upgradeButton == null)
            upgradeButton = gameObject;

        cachedButton = upgradeButton != null ? upgradeButton.GetComponent<Button>() : GetComponent<Button>();
    }

    private void RefreshEditorPreview()
    {
        if (!previewInEditor)
            return;

        if (!HasRequiredData())
        {
            ApplyUnavailableState(editorPreviewLevel);
            return;
        }

        int maxLevel = GetMaxUpgradeLevel();
        int previewLevel = Mathf.Clamp(editorPreviewLevel, 0, maxLevel);
        int previewMoney = Mathf.Max(0, editorPreviewMoney);
        int upgradeCost = GetUpgradeCost();
        string currentSummary = GetUpgradeSummary(previewLevel);
        string nextSummary = IsAtMaxLevel(previewLevel, maxLevel) ? string.Empty : GetUpgradeSummary(previewLevel + 1);

        ApplyButtonState(upgradeCost, previewLevel, maxLevel, previewMoney, currentSummary, nextSummary);
    }

    private void ApplyButtonState(int upgradeCost, int currentLevel, int maxLevel, int currentMoney, string currentSummary, string nextSummary)
    {
        bool isAtMaxLevel = IsAtMaxLevel(currentLevel, maxLevel);
        string costLabel = GetLocalizedText(CostLabelKey, "Cost: ");
        string currentValueLabel = GetLocalizedText(CurrentValueLabelKey, "Now:");
        string levelShortLabel = GetLocalizedText(LevelShortLabelKey, "Lv.");
        string nextLabel = GetLocalizedText(NextLabelKey, "Next:");
        string maxLabel = GetLocalizedText(MaxLabelKey, "MAX");

        if (upgradeCostLabel != null)
            upgradeCostLabel.text = isAtMaxLevel ? $"{maxLabel}\n{nextLabel} -" : upgradeCost >= 0 ? $"{costLabel}{upgradeCost}\n{nextLabel} {nextSummary}" : $"{costLabel}-\n{nextLabel} -";

        if (currentLevelLabel != null)
            currentLevelLabel.text = $"{levelShortLabel} {currentLevel}/{maxLevel}\n{currentValueLabel} {currentSummary}";

        if (cachedButton != null)
            cachedButton.interactable = !isAtMaxLevel && upgradeCost >= 0 && currentMoney >= upgradeCost;
    }

    private void ApplyUnavailableState(int previewLevel)
    {
        string costLabel = GetLocalizedText(CostLabelKey, "Cost: ");
        string currentValueLabel = GetLocalizedText(CurrentValueLabelKey, "Now:");
        string levelShortLabel = GetLocalizedText(LevelShortLabelKey, "Lv.");
        string nextLabel = GetLocalizedText(NextLabelKey, "Next:");
        string baseValue = GetLocalizedText(BaseValueKey, "Base");

        if (upgradeCostLabel != null)
            upgradeCostLabel.text = $"{costLabel}-\n{nextLabel} -";

        if (currentLevelLabel != null)
            currentLevelLabel.text = $"{levelShortLabel} {Mathf.Max(0, previewLevel)}\n{currentValueLabel} {baseValue}";

        if (cachedButton != null)
            cachedButton.interactable = false;
    }

    private bool HasRequiredData()
    {
        if (IsSpaceshipUpgrade())
            return upgradesSO != null;

        return ResolveGroundDefaults() != null;
    }

    private bool IsSpaceshipUpgrade()
    {
        return upgradeType <= UpgradeButtonType.MaxShields;
    }

    private int GetUpgradeCost()
    {
        if (IsSpaceshipUpgrade())
        {
            return upgradesSO != null ? Mathf.RoundToInt(upgradesSO.GetUpgradeCost(ToSpaceshipUpgradeType())) : -1;
        }

        GroundTrooperUpgradeDefaults groundDefaults = ResolveGroundDefaults();
        return groundDefaults != null ? groundDefaults.universalUpgradeCostPerLevel : -1;
    }

    private int GetCurrentUpgradeLevel()
    {
        if (SaveManager.instance == null)
            return 0;

        if (IsSpaceshipUpgrade())
            return SaveManager.instance.GetUpgradeLevel(ToSpaceshipUpgradeType());

        return SaveManager.instance.GetGroundUpgradeLevel(ToGroundUpgradeType());
    }

    private int GetMaxUpgradeLevel()
    {
        if (Application.isPlaying && SaveManager.instance != null)
        {
            if (IsSpaceshipUpgrade())
                return SaveManager.instance.GetMaxUpgradeLevel(ToSpaceshipUpgradeType());

            return SaveManager.instance.GetGroundMaxUpgradeLevel(ToGroundUpgradeType());
        }

        if (IsSpaceshipUpgrade())
            return upgradesSO != null ? upgradesSO.GetMaxUpgradeLevel(ToSpaceshipUpgradeType()) : 0;

        GroundTrooperUpgradeDefaults groundDefaults = ResolveGroundDefaults();
        return groundDefaults != null ? groundDefaults.GetMaxUpgradeLevel(ToGroundUpgradeType()) : 0;
    }

    private bool IsAtMaxLevel(int currentLevel, int maxLevel)
    {
        return currentLevel >= maxLevel;
    }

    private string GetUpgradeDisplayName()
    {
        return GetLocalizedText(GetUpgradeDisplayNameKey(), GetUpgradeDisplayNameFallback());
    }

    private string GetUpgradeDisplayNameFallback()
    {
        if (IsSpaceshipUpgrade())
            return upgradesSO != null ? upgradesSO.GetUpgradeDisplayName(ToSpaceshipUpgradeType()) : upgradeType.ToString();

        switch (ToGroundUpgradeType())
        {
            case GroundTrooperUpgradeType.MoveSpeed:
                return "Move Speed";
            case GroundTrooperUpgradeType.JumpVelocity:
                return "Jump Power";
            case GroundTrooperUpgradeType.MaxHealth:
                return "Player Health";
        }

        return upgradeType.ToString();
    }

    private string GetUpgradeDisplayNameKey()
    {
        switch (upgradeType)
        {
            case UpgradeButtonType.MoveForce:
                return "Upgrades_Astrojumper_Thrust";
            case UpgradeButtonType.MaxSpeed:
                return "Upgrades_Astrojumper_MaxSpeed";
            case UpgradeButtonType.BoostForce:
                return "Upgrades_Astrojumper_Boost";
            case UpgradeButtonType.BarrelRollDistance:
                return "Upgrades_Astrojumper_RollDistance";
            case UpgradeButtonType.BarrelRollSpeed:
                return "Upgrades_Astrojumper_RollSpeed";
            case UpgradeButtonType.FireRate:
                return "Upgrades_Astrojumper_Firerate";
            case UpgradeButtonType.MaxHealth:
                return "Upgrades_Astrojumper_Health";
            case UpgradeButtonType.MaxShields:
                return "Upgrades_Astrojumper_Shields";
            case UpgradeButtonType.PlayerMoveSpeed:
                return "Upgrades_Exosuit_Speed";
            case UpgradeButtonType.PlayerJumpVelocity:
                return "Upgrades_Exosuit_Jump";
            case UpgradeButtonType.PlayerMaxHealth:
                return "Upgrades_Exosuit_Health";
            default:
                return string.Empty;
        }
    }

    private string GetUpgradeSummary(int level)
    {
        if (IsSpaceshipUpgrade())
        {
            return upgradesSO != null ? GetSpaceshipUpgradeSummary(ToSpaceshipUpgradeType(), level) : GetLocalizedText(BaseValueKey, "Base");
        }

        return GetGroundUpgradeSummary(ToGroundUpgradeType(), level, ResolveGroundDefaults());
    }

    private string GetSpaceshipUpgradeSummary(PlayerUpgradeState.UpgradeType spaceshipUpgradeType, int level)
    {
        if (level <= 0)
            return GetLocalizedText(BaseValueKey, "Base");

        float totalUpgrade = GetSpaceshipUpgradeAmountForLevel(spaceshipUpgradeType, level);

        switch (spaceshipUpgradeType)
        {
            case PlayerUpgradeState.UpgradeType.MoveForce:
            case PlayerUpgradeState.UpgradeType.MaxSpeed:
            case PlayerUpgradeState.UpgradeType.BoostForce:
            case PlayerUpgradeState.UpgradeType.BarrelRollDistance:
            case PlayerUpgradeState.UpgradeType.MaxShields:
                return $"+{totalUpgrade:0.##}";
            case PlayerUpgradeState.UpgradeType.MaxHealth:
                return $"+{totalUpgrade:0}";
            case PlayerUpgradeState.UpgradeType.BarrelRollSpeed:
            case PlayerUpgradeState.UpgradeType.FireRate:
                return $"-{totalUpgrade:0.##}s";
            default:
                return $"+{totalUpgrade:0.##}";
        }
    }

    private float GetSpaceshipUpgradeAmountForLevel(PlayerUpgradeState.UpgradeType spaceshipUpgradeType, int level)
    {
        if (upgradesSO == null)
            return 0f;

        int clampedLevel = Mathf.Max(0, level);

        switch (spaceshipUpgradeType)
        {
            case PlayerUpgradeState.UpgradeType.MoveForce:
                return clampedLevel * upgradesSO.moveForceUpgradePerLevel;
            case PlayerUpgradeState.UpgradeType.MaxSpeed:
                return clampedLevel * upgradesSO.maxSpeedUpgradePerLevel;
            case PlayerUpgradeState.UpgradeType.BoostForce:
                return clampedLevel * upgradesSO.boostForceUpgradePerLevel;
            case PlayerUpgradeState.UpgradeType.BarrelRollDistance:
                return clampedLevel * upgradesSO.barrelRollDistanceUpgradePerLevel;
            case PlayerUpgradeState.UpgradeType.BarrelRollSpeed:
                return clampedLevel * upgradesSO.barrelRollSpeedUpgradePerLevel;
            case PlayerUpgradeState.UpgradeType.FireRate:
                return clampedLevel * upgradesSO.fireRateUpgradePerLevel;
            case PlayerUpgradeState.UpgradeType.MaxHealth:
                return clampedLevel * upgradesSO.maxHealthUpgradePerLevel;
            case PlayerUpgradeState.UpgradeType.MaxShields:
                return clampedLevel * upgradesSO.maxShieldsPerLevel;
            default:
                return 0f;
        }
    }

    private string GetGroundUpgradeSummary(GroundTrooperUpgradeType groundUpgradeType, int level, GroundTrooperUpgradeDefaults groundDefaults)
    {
        if (level <= 0)
            return GetLocalizedText(BaseValueKey, "Base");

        if (groundDefaults == null)
            return $"{GetLocalizedText(LevelShortLabelKey, "Lv.")} {level}";

        float totalUpgrade = GetGroundUpgradeAmountForLevel(groundUpgradeType, level, groundDefaults);

        switch (groundUpgradeType)
        {
            case GroundTrooperUpgradeType.MoveSpeed:
            case GroundTrooperUpgradeType.JumpVelocity:
                return $"+{totalUpgrade:0.##}";
            case GroundTrooperUpgradeType.MaxHealth:
                return $"+{totalUpgrade:0}";
        }

        return $"+{totalUpgrade:0.##}";
    }

    private float GetGroundUpgradeAmountForLevel(GroundTrooperUpgradeType groundUpgradeType, int level, GroundTrooperUpgradeDefaults groundDefaults)
    {
        if (groundDefaults == null)
            return 0f;

        int clampedLevel = Mathf.Max(0, level);

        switch (groundUpgradeType)
        {
            case GroundTrooperUpgradeType.MoveSpeed:
                return clampedLevel * groundDefaults.moveSpeedUpgradePerLevel;
            case GroundTrooperUpgradeType.JumpVelocity:
                return clampedLevel * groundDefaults.jumpVelocityUpgradePerLevel;
            case GroundTrooperUpgradeType.MaxHealth:
                return clampedLevel * groundDefaults.maxHealthUpgradePerLevel;
        }

        return 0f;
    }

    private GroundTrooperUpgradeDefaults ResolveGroundDefaults()
    {
        if (defaultGameSaveSO != null)
            return defaultGameSaveSO.groundTrooperDefaults;

        if (SaveManager.instance != null && SaveManager.instance.DefaultGameSaveSO != null)
            return SaveManager.instance.DefaultGameSaveSO.groundTrooperDefaults;

        return null;
    }

    private string GetLocalizedText(string textKey, string fallback)
    {
        if (string.IsNullOrEmpty(textKey) || !Application.isPlaying)
            return fallback;

        string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString(UiTextTable, textKey);
        return string.IsNullOrEmpty(localizedText) ? fallback : localizedText;
    }

    private PlayerUpgradeState.UpgradeType ToSpaceshipUpgradeType()
    {
        switch (upgradeType)
        {
            case UpgradeButtonType.MoveForce:
                return PlayerUpgradeState.UpgradeType.MoveForce;
            case UpgradeButtonType.MaxSpeed:
                return PlayerUpgradeState.UpgradeType.MaxSpeed;
            case UpgradeButtonType.BoostForce:
                return PlayerUpgradeState.UpgradeType.BoostForce;
            case UpgradeButtonType.BarrelRollDistance:
                return PlayerUpgradeState.UpgradeType.BarrelRollDistance;
            case UpgradeButtonType.BarrelRollSpeed:
                return PlayerUpgradeState.UpgradeType.BarrelRollSpeed;
            case UpgradeButtonType.FireRate:
                return PlayerUpgradeState.UpgradeType.FireRate;
            case UpgradeButtonType.MaxHealth:
                return PlayerUpgradeState.UpgradeType.MaxHealth;
            case UpgradeButtonType.MaxShields:
                return PlayerUpgradeState.UpgradeType.MaxShields;
            default:
                return PlayerUpgradeState.UpgradeType.MoveForce;
        }
    }

    private GroundTrooperUpgradeType ToGroundUpgradeType()
    {
        switch (upgradeType)
        {
            case UpgradeButtonType.PlayerMoveSpeed:
                return GroundTrooperUpgradeType.MoveSpeed;
            case UpgradeButtonType.PlayerJumpVelocity:
                return GroundTrooperUpgradeType.JumpVelocity;
            case UpgradeButtonType.PlayerMaxHealth:
                return GroundTrooperUpgradeType.MaxHealth;
            default:
                return GroundTrooperUpgradeType.MoveSpeed;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheReferences();
        RefreshView();
    }
#endif
}
