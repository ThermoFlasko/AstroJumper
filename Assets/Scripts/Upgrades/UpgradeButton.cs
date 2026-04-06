using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private PlayerUpgradeState.UpgradeType upgradeType;

    [Header("Buttons")] [SerializeField] private PlayerSpaceshipUpgradesSO upgradesSO;
    [SerializeField] private GameObject upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeLabel;
    [SerializeField] private TextMeshProUGUI upgradeCostLabel;
    [SerializeField] private TextMeshProUGUI currentLevelLabel;
    private Button cachedButton;

    private void Awake()
    {
        cachedButton = upgradeButton != null ? upgradeButton.GetComponent<Button>() : GetComponent<Button>();
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
        print($"Player clickd on {this.upgradeType.ToString()} type button.");

        if (SaveManager.instance == null || upgradesSO == null)
        {
            RefreshView();
            return;
        }

        int upgradeCost = Mathf.RoundToInt(upgradesSO.GetUpgradeCost(upgradeType));
        if (SaveManager.instance.GetNewMoney() >= upgradeCost)
        {
            print("can afford upgrade");
            SaveManager.instance.AddNewMoney(-upgradeCost);
            SaveManager.instance.AddUpgradeLevel(upgradeType);
        }

        RefreshView();
    }

    public void RefreshView()
    {
        if (upgradeLabel != null)
            upgradeLabel.text = upgradeType.ToString();

        if (SaveManager.instance == null || upgradesSO == null)
        {
            if (upgradeCostLabel != null)
                upgradeCostLabel.text = "Cost: -";

            if (currentLevelLabel != null)
                currentLevelLabel.text = "Lv. 0";

            if (cachedButton != null)
                cachedButton.interactable = false;

            return;
        }

        int upgradeCost = Mathf.RoundToInt(upgradesSO.GetUpgradeCost(upgradeType));
        int currentLevel = SaveManager.instance.GetUpgradeLevel(upgradeType);
        int currentMoney = SaveManager.instance.GetNewMoney();

        if (upgradeCostLabel != null)
            upgradeCostLabel.text = $"Cost: {upgradeCost}";

        if (currentLevelLabel != null)
            currentLevelLabel.text = $"Lv. {currentLevel}";

        if (cachedButton != null)
            cachedButton.interactable = currentMoney >= upgradeCost;
    }

    private void HandleMoneyChanged(int _)
    {
        RefreshView();
    }
}
