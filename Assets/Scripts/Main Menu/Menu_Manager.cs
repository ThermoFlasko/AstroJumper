using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Localization.Settings;

public class Menu_Manager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    public GameObject keybindPanel;
    //public GameObject upgradesPanel;

    //[Header("Other stuff")] public TMP_Text upgradesScrapCounterText;

    [Header("Audio Settings")]
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public TMP_Text volumeText;

    private void OnEnable()
    {
        //SaveManager.NewMoneyChanged += UpdateUpgradesScrapCounter;
    }

    private void OnDisable()
    {
        //SaveManager.NewMoneyChanged -= UpdateUpgradesScrapCounter;

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowMainMenu();

        if (PlayerPrefs.GetInt("OpenOptionsOnLoad", 0) == 1)
        {
            ShowOptionsMenu();
            PlayerPrefs.SetInt("OpenOptionsOnLoad", 0);
        }

        string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "en");
        StartCoroutine(LoadSavedLanguage(savedLanguage));

        if (volumeSlider != null && volumeText != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            volumeSlider.value = savedVolume;

            UpdateVolumeText(savedVolume);

            volumeSlider.onValueChanged.AddListener(SetMasterVolume);

            SetMasterVolume(savedVolume);
        }
    }

    public void SetMasterVolume(float sliderValue)
    {
        AudioSource bgMusic = FindFirstObjectByType<AudioSource>();
        if (bgMusic != null)
        {
            bgMusic.volume = sliderValue;
        }

        float volumedB = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat("MasterVolume", volumedB);

        UpdateVolumeText(sliderValue);

        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }

    void UpdateVolumeText(float sliderValue)
    {
        if (volumeText != null)
        {
            int volumePercent = Mathf.RoundToInt(sliderValue * 100);
            volumeText.text = volumePercent.ToString() + "%";
        }
    }

    private void CloseAllMenus()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        keybindPanel.SetActive(false);
        //upgradesPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        CloseAllMenus();
        mainMenuPanel.SetActive(true);
    }

    public void StartGame()
    {
        SceneLoader.Instance.LoadNextScene("Tutorial Ground");
    }

    public void ShowOptionsMenu()
    {
        CloseAllMenus();
        optionsPanel.SetActive(true);
    }

    public void ShowCreditsMenu()
    {
        CloseAllMenus();
        creditsPanel.SetActive(true);
    }

    public void ShowKeybindMenu()
    {
        CloseAllMenus();
        keybindPanel.SetActive(true);
    }

    public void ShowUpgradePannelsMenu()
    {
        CloseAllMenus();
        //upgradesPanel.SetActive(true);
        //RefreshUpgradesScrapCounter();
    }

    System.Collections.IEnumerator LoadSavedLanguage(string savedLanguage)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = LocalizationSettings.AvailableLocales.GetLocale(savedLanguage);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            Debug.Log("Loaded saved language: " + savedLanguage);
        }
        else
        {
            Debug.LogWarning("Saved language not found: " + savedLanguage);
        }
    }

    //private void RefreshUpgradesScrapCounter()
    //{
    //    if (upgradesScrapCounterText == null)
    //        return;

    //    int currentMoney = SaveManager.instance != null ? SaveManager.instance.GetNewMoney() : 0;
    //    upgradesScrapCounterText.text = currentMoney.ToString();
    //}

    //private void UpdateUpgradesScrapCounter(int currentMoney)
    //{
    //    if (upgradesScrapCounterText == null)
    //        return;

    //    upgradesScrapCounterText.text = currentMoney.ToString();
    //}
}
