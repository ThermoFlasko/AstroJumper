using TMPro;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{

    public TMP_Text upgradesScrapCounterText;

    private void OnEnable()
    {
        SaveManager.NewMoneyChanged += UpdateUpgradesScrapCounter;
    }

    private void OnDisable()
    {
        SaveManager.NewMoneyChanged -= UpdateUpgradesScrapCounter;
    }

    private void Start()
    {
        RefreshUpgradesScrapCounter();
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
}
