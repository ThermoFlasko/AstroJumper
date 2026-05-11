using System;
using TMPro;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
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
        RefreshMenu();
    }

    private void Start()
    {
        RefreshMenu();
    }

    private void OnDisable()
    {
        SaveManager.NewMoneyChanged -= HandleMoneyChanged;
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
        string slotName = attackType == GroundAttackType.Melee ? "Melee" : "Ranged";
        string weaponName = attack != null ? attack.DisplayName : "None";

        label.text = $"{slotName} Slot: {weaponName}";
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
