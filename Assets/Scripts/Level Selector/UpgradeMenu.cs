using System;
using TMPro;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    public event Action MenuOpened;
    public event Action MenuClosed;

    public TMP_Text upgradesScrapCounterText;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            RefreshReferences();
    }
#endif
}
