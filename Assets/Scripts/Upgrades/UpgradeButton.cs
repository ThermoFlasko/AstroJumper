using TMPro;
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
        RefreshView();
    }

    private void OnDisable()
    {
        SaveManager.NewMoneyChanged -= HandleMoneyChanged;
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
        if (upgradeCost >= 0 && SaveManager.instance.GetNewMoney() >= upgradeCost)
        {
            print("can afford upgrade");
            SaveManager.instance.AddNewMoney(-upgradeCost);

            if (IsSpaceshipUpgrade())
                SaveManager.instance.AddUpgradeLevel(ToSpaceshipUpgradeType());
            else
                SaveManager.instance.AddGroundUpgradeLevel(ToGroundUpgradeType());
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
        int currentMoney = SaveManager.instance.GetNewMoney();
        string currentSummary = GetUpgradeSummary(currentLevel);
        string nextSummary = GetUpgradeSummary(currentLevel + 1);

        ApplyButtonState(upgradeCost, currentLevel, currentMoney, currentSummary, nextSummary);
    }

    private void HandleMoneyChanged(int _)
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

        int previewLevel = Mathf.Max(0, editorPreviewLevel);
        int previewMoney = Mathf.Max(0, editorPreviewMoney);
        int upgradeCost = GetUpgradeCost();
        string currentSummary = GetUpgradeSummary(previewLevel);
        string nextSummary = GetUpgradeSummary(previewLevel + 1);

        ApplyButtonState(upgradeCost, previewLevel, previewMoney, currentSummary, nextSummary);
    }

    private void ApplyButtonState(int upgradeCost, int currentLevel, int currentMoney, string currentSummary, string nextSummary)
    {
        if (upgradeCostLabel != null)
            upgradeCostLabel.text = upgradeCost >= 0 ? $"Cost: {upgradeCost}\nNext: {nextSummary}" : "Cost: -\nNext: -";

        if (currentLevelLabel != null)
            currentLevelLabel.text = $"Lv. {currentLevel}\nNow: {currentSummary}";

        if (cachedButton != null)
            cachedButton.interactable = upgradeCost >= 0 && currentMoney >= upgradeCost;
    }

    private void ApplyUnavailableState(int previewLevel)
    {
        if (upgradeCostLabel != null)
            upgradeCostLabel.text = "Cost: -\nNext: -";

        if (currentLevelLabel != null)
            currentLevelLabel.text = $"Lv. {Mathf.Max(0, previewLevel)}\nNow: Base";

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

    private string GetUpgradeDisplayName()
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

    private string GetUpgradeSummary(int level)
    {
        if (IsSpaceshipUpgrade())
        {
            return upgradesSO != null ? upgradesSO.GetUpgradeSummary(ToSpaceshipUpgradeType(), level) : "Base";
        }

        return GetGroundUpgradeSummary(ToGroundUpgradeType(), level, ResolveGroundDefaults());
    }

    private string GetGroundUpgradeSummary(GroundTrooperUpgradeType groundUpgradeType, int level, GroundTrooperUpgradeDefaults groundDefaults)
    {
        if (level <= 0)
            return "Base";

        if (groundDefaults == null)
            return $"Lv. {level}";

        float totalUpgrade = GetGroundUpgradeAmountForLevel(groundUpgradeType, level, groundDefaults);

        switch (groundUpgradeType)
        {
            case GroundTrooperUpgradeType.MoveSpeed:
                return $"+{totalUpgrade:0.##} move";
            case GroundTrooperUpgradeType.JumpVelocity:
                return $"+{totalUpgrade:0.##} jump";
            case GroundTrooperUpgradeType.MaxHealth:
                return $"+{totalUpgrade:0} health";
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
