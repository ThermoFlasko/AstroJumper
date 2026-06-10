using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public class UpgradeMenu : MonoBehaviour
{
    private const string UiTextTable = "UI Text";
    private const string ExosuitUpgradesKey = "Upgrades_Label_Exosuit";
    private const string AstrojumperUpgradesKey = "Upgrades_Label_Astrojumper";
    private const string ExosuitWeaponsKey = "Loadout_Label_ExosuitWeapon";
    private const string WeaponLoadoutKey = "Loadout_Label_Loadout";
    private const string EquippedWeaponsKey = "Loadout_Label_Equipped";
    private const string MeleeSlotKey = "Loadout_Label_MeleeSlot";
    private const string RangedSlotKey = "Loadout_Label_RangedSlot";
    private const string WeaponOptionsKey = "Loadout_Label_Options";
    private const string NoneValueKey = "UPGRADES_VALUE_NONE";
    private const string AxeWeaponKey = "Loadout_Weapon_Axe";
    private const string PhasorWeaponKey = "Loadout_Weapon_Phasor";
    private const string BlasterWeaponKey = "Loadout_Weapon_Blaster";
    private const string GrenadeLauncherWeaponKey = "Loadout_Weapon_GrenadeLauncher";

    public event Action MenuOpened;
    public event Action MenuClosed;

    public TMP_Text upgradesScrapCounterText;

    [Header("Ground Weapon Selection")]
    [SerializeField] private GroundAttackCatalogSO groundAttackCatalog;
    [SerializeField] private TextMeshProUGUI equippedMeleeAttackLabel;
    [SerializeField] private TextMeshProUGUI equippedRangedAttackLabel;

    [SerializeField] private UpgradeButton[] upgradeButtons;
    [SerializeField] private Canvas[] canvasesToToggle;
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private bool deactivateGameObjectWhenClosed;
    [SerializeField] private bool openOnStart;

    public bool DeactivateGameObjectWhenClosed
    {
        get => deactivateGameObjectWhenClosed;
        set => deactivateGameObjectWhenClosed = value;
    }

    public bool IsOpen => gameObject.activeInHierarchy && (menuCanvasGroup == null || menuCanvasGroup.alpha > 0.001f);

    private void Awake()
    {
        RefreshReferences();
        ApplyInitialState();
    }

    private void OnEnable()
    {
        SaveManager.NewMoneyChanged += HandleMoneyChanged;
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
        RefreshMenu();
    }

    private void Start()
    {
        RefreshMenu();
    }

    private void OnDisable()
    {
        SaveManager.NewMoneyChanged -= HandleMoneyChanged;
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    public void RefreshReferences()
    {
        if (menuCanvasGroup == null)
            menuCanvasGroup = GetComponent<CanvasGroup>();

        upgradeButtons = GetComponentsInChildren<UpgradeButton>(true);
        canvasesToToggle = GetComponentsInChildren<Canvas>(true);
    }

    private void ApplyInitialState()
    {
        SetMenuVisible(openOnStart);
    }

    public void OpenMenu()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        SetMenuVisible(true);
        RefreshMenu();
        MenuOpened?.Invoke();
    }

    public void CloseMenu()
    {
        MenuClosed?.Invoke();
        SetMenuVisible(false);

        if (deactivateGameObjectWhenClosed)
            gameObject.SetActive(false);
    }

    public void RefreshMenu()
    {
        if (upgradeButtons == null || upgradeButtons.Length == 0 || canvasesToToggle == null || canvasesToToggle.Length == 0)
            RefreshReferences();

        RefreshUpgradesScrapCounter();
        RefreshStaticLocalizedLabels();
        RefreshUpgradeButtons();
        RefreshGroundWeaponSelection();
    }

    public void EquipGroundAttack(string attackIdOrDisplayName)
    {
        GroundAttackDefinition attack = FindGroundAttack(attackIdOrDisplayName);
        if (attack == null)
        {
            Debug.LogWarning($"Could not equip ground attack '{attackIdOrDisplayName}'. It was not found in the ground attack catalog.", this);
            return;
        }

        if (groundAttackCatalog == null || !groundAttackCatalog.IsValidAttack(attack, attack.AttackType))
        {
            Debug.LogWarning($"Could not equip ground attack '{attack.DisplayName}'. The attack does not match its configured type or prefab.", this);
            return;
        }

        if (SaveManager.instance == null)
        {
            Debug.LogWarning($"Could not equip ground attack '{attack.DisplayName}' because there is no SaveManager in the scene.", this);
            RefreshGroundWeaponSelection();
            return;
        }

        SaveManager.instance.SetEquippedGroundAttackId(attack.AttackType, attack.AttackId);
        SaveManager.instance.SaveGame();
        RefreshGroundWeaponSelection();
    }

    private void RefreshUpgradeButtons()
    {
        if (upgradeButtons == null)
            return;

        foreach (UpgradeButton button in upgradeButtons)
            button?.RefreshView();
    }

    private void SetMenuVisible(bool visible)
    {
        if (canvasesToToggle == null || canvasesToToggle.Length == 0)
            RefreshReferences();

        if (canvasesToToggle != null)
        {
            foreach (Canvas canvas in canvasesToToggle)
            {
                if (canvas != null)
                    canvas.enabled = visible;
            }
        }

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = visible ? 1f : 0f;
            menuCanvasGroup.interactable = visible;
            menuCanvasGroup.blocksRaycasts = visible;
        }
    }

    private void HandleMoneyChanged(int currentMoney)
    {
        UpdateUpgradesScrapCounter(currentMoney);
        RefreshUpgradeButtons();
    }

    private void HandleLocaleChanged(Locale _)
    {
        RefreshMenu();
    }

    private void RefreshUpgradesScrapCounter()
    {
        if (upgradesScrapCounterText == null)
            return;

        int currentMoney = SaveManager.instance != null ? SaveManager.instance.GetNewMoney() : 0;
        upgradesScrapCounterText.text = currentMoney.ToString();
    }

    private void UpdateUpgradesScrapCounter(int currentMoney)
    {
        if (upgradesScrapCounterText == null)
            return;

        upgradesScrapCounterText.text = currentMoney.ToString();
    }

    private void RefreshGroundWeaponSelection()
    {
        SetGroundWeaponLabel(equippedMeleeAttackLabel, GroundAttackType.Melee);
        SetGroundWeaponLabel(equippedRangedAttackLabel, GroundAttackType.Ranged);
    }

    private void SetGroundWeaponLabel(TextMeshProUGUI label, GroundAttackType attackType)
    {
        if (label == null)
            return;

        GroundAttackDefinition attack = GetEquippedGroundAttack(attackType);
        string slotName = attackType == GroundAttackType.Melee
            ? GetLocalizedText(MeleeSlotKey, "Melee Slot")
            : GetLocalizedText(RangedSlotKey, "Ranged Slot");
        string weaponName = GetGroundAttackDisplayName(attack);

        label.text = $"{slotName}: {weaponName}";
    }

    private void RefreshStaticLocalizedLabels()
    {
        TextMeshProUGUI[] labels = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI label in labels)
        {
            if (label == null)
                continue;

            string textKey = GetStaticLabelKey(label);
            if (string.IsNullOrEmpty(textKey))
                continue;

            LocalizeStringEvent localizeStringEvent = label.GetComponent<LocalizeStringEvent>();
            if (localizeStringEvent != null)
                localizeStringEvent.enabled = false;

            label.text = GetLocalizedText(textKey, label.text);
        }
    }

    private string GetStaticLabelKey(TextMeshProUGUI label)
    {
        Transform parent = label.transform.parent;
        string textKey = parent != null ? GetStaticLabelKeyFromName(parent.name) : string.Empty;

        if (!string.IsNullOrEmpty(textKey))
            return textKey;

        textKey = GetStaticLabelKeyFromName(label.gameObject.name);
        if (!string.IsNullOrEmpty(textKey))
            return textKey;

        return GetStaticLabelKeyFromName(label.text);
    }

    private string GetStaticLabelKeyFromName(string labelName)
    {
        switch (labelName)
        {
            case "Exosuit Page":
            case "Exosuit Upgrades":
                return ExosuitUpgradesKey;
            case "Astrojumper Page":
            case "Astrojumper Upgrades":
            case "Spaceship Upgrades":
                return AstrojumperUpgradesKey;
            case "Exosuit Weapons Page":
            case "Exosuit Weapons":
                return ExosuitWeaponsKey;
            case "Weapon Loadout":
            case "Weapon Loadout Title":
                return WeaponLoadoutKey;
            case "Equipped Weapons":
            case "Equipped Slots Label":
                return EquippedWeaponsKey;
            case "Weapon Options":
            case "Weapon Options Label":
                return WeaponOptionsKey;
            case "Melee Attack Option Button":
            case "Melee Attack":
                return AxeWeaponKey;
            case "Green Blast Option Button":
            case "Green Blast":
                return PhasorWeaponKey;
            case "Ranged Attack Option Button":
            case "Ranged Attack":
                return BlasterWeaponKey;
            case "Lobbed Ranged Attack Option Button":
            case "Lobbed Ranged Attack Option Button (1)":
            case "Lobbed Ranged Attack":
            case "Lobbed Ranged\n Attack":
                return GrenadeLauncherWeaponKey;
            default:
                return string.Empty;
        }
    }

    private string GetGroundAttackDisplayName(GroundAttackDefinition attack)
    {
        if (attack == null)
            return GetLocalizedText(NoneValueKey, "None");

        string weaponKey = GetWeaponKeyForAttack(attack);
        return GetLocalizedText(weaponKey, attack.DisplayName);
    }

    private string GetWeaponKeyForAttack(GroundAttackDefinition attack)
    {
        if (attack == null)
            return string.Empty;

        string weaponKey = GetWeaponKeyForName(attack.AttackId);
        if (!string.IsNullOrEmpty(weaponKey))
            return weaponKey;

        return GetWeaponKeyForName(attack.DisplayName);
    }

    private string GetWeaponKeyForName(string attackName)
    {
        switch (attackName)
        {
            case "Melee Attack":
            case "Melee Defualt":
                return AxeWeaponKey;
            case "Green Blast":
                return PhasorWeaponKey;
            case "Ranged Attack":
            case "Defualt Ranged":
                return BlasterWeaponKey;
            case "Lobbed Ranged":
            case "Lobbed Ranged Attack":
            case "Lobbed Ranged\n Attack":
                return GrenadeLauncherWeaponKey;
            default:
                return string.Empty;
        }
    }

    private string GetLocalizedText(string textKey, string fallback)
    {
        if (string.IsNullOrEmpty(textKey) || !Application.isPlaying)
            return fallback;

        string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString(UiTextTable, textKey);
        return string.IsNullOrEmpty(localizedText) ? fallback : localizedText;
    }

    private GroundAttackDefinition GetEquippedGroundAttack(GroundAttackType attackType)
    {
        if (groundAttackCatalog == null)
            return null;

        string equippedAttackId = SaveManager.instance != null
            ? SaveManager.instance.GetEquippedGroundAttackId(attackType)
            : string.Empty;

        return groundAttackCatalog.GetSafeAttack(equippedAttackId, attackType);
    }

    private GroundAttackDefinition FindGroundAttack(string attackIdOrDisplayName)
    {
        if (groundAttackCatalog == null || string.IsNullOrWhiteSpace(attackIdOrDisplayName))
            return null;

        GroundAttackDefinition attack = groundAttackCatalog.GetAttackById(attackIdOrDisplayName);
        if (attack != null)
            return attack;

        foreach (GroundAttackDefinition catalogAttack in groundAttackCatalog.Attacks)
        {
            if (catalogAttack == null)
                continue;

            if (string.Equals(catalogAttack.AttackId, attackIdOrDisplayName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(catalogAttack.DisplayName, attackIdOrDisplayName, StringComparison.OrdinalIgnoreCase))
            {
                return catalogAttack;
            }
        }

        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            RefreshReferences();
    }
#endif
}
