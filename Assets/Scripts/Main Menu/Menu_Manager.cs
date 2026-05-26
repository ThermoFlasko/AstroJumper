using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Collections;

public class Menu_Manager : MonoBehaviour
{
    [Header("Menu Panels")]
    public Canvas mainMenuPanel;
    public Canvas optionsPanel;
    public Canvas creditsPanel;
    public Canvas keybindPanel;
    //public GameObject upgradesPanel;

    //[Header("Other stuff")] public TMP_Text upgradesScrapCounterText;

    [Header("Audio Settings")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicSlider;
    public Slider soundSlider;
    public TMP_Text masterVolumeText;
    public TMP_Text musicVolumeText;
    public TMP_Text soundVolumeText;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        }
        if (musicSlider != null)
        {

        }
        if (soundSlider != null)
        {

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

        if (masterVolumeSlider != null && masterVolumeText != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            masterVolumeSlider.value = savedVolume;

            UpdateVolumeText(savedVolume, masterVolumeText);

            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

            SetMasterVolume(savedVolume);
        }

        if (musicSlider != null && musicVolumeText != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            musicSlider.value = savedVolume;
        }

        if (soundSlider != null && soundVolumeText != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("SoundVolume", 0.75f);
            soundSlider.value = savedVolume;
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

        UpdateVolumeText(sliderValue, masterVolumeText);

        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }

    void UpdateVolumeText(float sliderValue, TMP_Text sliderText)
    {
        if (sliderText != null)
        {
            int volumePercent = Mathf.RoundToInt(sliderValue * 100);
            sliderText.text = volumePercent.ToString() + "%";
        }
    }

    private void CloseAllMenus()
    {
        mainMenuPanel.enabled = false;
        optionsPanel.enabled = false;
        creditsPanel.enabled = false;
        keybindPanel.enabled = false;
    }

    public void ShowMainMenu()
    {
        CloseAllMenus();
        mainMenuPanel.enabled = true;
    }

    public void StartGame()
    {
        LevelSaveData levelSaveData = SaveManager.instance.GetCurrentLevelData();
        levelSaveData.currLevel = "Tutorial Ground";
        SceneLoader.Instance.LoadNextScene("Tutorial Ground");
    }

    public void ContinueGame()
    {
        // based on current Level, load right scene
        LevelSaveData levelSaveData = SaveManager.instance.GetCurrentLevelData();
        print($"level save data: {levelSaveData.currLevel}");

        SceneLoader.Instance.LoadNextScene(levelSaveData.currLevel);

        
    }

    public void ShowOptionsMenu()
    {
        CloseAllMenus();
        optionsPanel.enabled = true;
    }

    public void ShowCreditsMenu()
    {
        CloseAllMenus();
        creditsPanel.enabled = true;
    }

    public void ShowKeybindMenu()
    {
        CloseAllMenus();
        keybindPanel.enabled = true;
    }

    public void StartQuitGame()
    {
        StartCoroutine(QuitGame());
    }

    public IEnumerator QuitGame()
    {
        Application.Quit();
        yield return null;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        Debug.Log($"Menu_Manager detected language change to: {newLocale.Identifier.Code}");
    }

    System.Collections.IEnumerator LoadSavedLanguage(string savedLanguage)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = LocalizationSettings.AvailableLocales.GetLocale(savedLanguage);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            Debug.Log("Loaded saved language: " + savedLanguage);
            OnLocaleChanged(locale);
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




//public class OptionsScript : MonoBehaviour
//{
//    public AudioMixer audioMixer;

//    [SerializeField] private GameObject[] sliders;

//    // Alternate
//    // [SerializeField] private Slider[] sliders;
//    private float currSliderVolume;

//    private void Start()
//    {
//        GameManager.Instance.OnOptionsAccessed += OptionsAccessed;
//    }

//    public void SetMasterVolume(float newVolume)
//    {
//        audioMixer.SetFloat("masterVolume", newVolume);
//    }
//    public void SetSFXVolume(float newVolume)
//    {
//        audioMixer.SetFloat("SFXVolume", newVolume);
//    }
//    public void SetMusicVolume(float newVolume)
//    {
//        audioMixer.SetFloat("musicVolume", newVolume);
//    }

//    public void SetFullscreen(bool isFullscreen)
//    {
//        Screen.fullScreen = isFullscreen;
//    }


//    void OptionsAccessed()
//    {
//        foreach (GameObject obj in sliders)
//        {
//            Slider slider = obj.GetComponentInChildren<Slider>();

//            if (obj.name == "Volume Slider" && obj != null)
//            {
//                audioMixer.GetFloat("masterVolume", out currSliderVolume);

//                slider.value = (int)currSliderVolume;
//            }
//            else if (obj.name == "SFX Volume Slider" && obj != null)
//            {
//                audioMixer.GetFloat("SFXVolume", out currSliderVolume);

//                slider.value = (int)currSliderVolume;
//            }
//            else if (obj.name == "Music Volume Slider" && obj != null)
//            {
//                audioMixer.GetFloat("musicVolume", out currSliderVolume);

//                slider.value = (int)currSliderVolume;
//            }
//        }

//        // Alternate:

//        //foreach (GameObject obj in sliders)
//        //{
//        //    Slider slider = obj.GetComponent<Slider>();

//        //    if (obj.name == "Volume Slider" && obj != null)
//        //    {
//        //        audioMixer.GetFloat("masterVolume", out currSliderVolume);

//        //        slider.value = (int)currSliderVolume;
//        //    }
//        //    else if (obj.name == "SFX Volume Slider" && obj != null)
//        //    {
//        //        audioMixer.GetFloat("SFXVolume", out currSliderVolume);

//        //        slider.value = (int)currSliderVolume;
//        //    }
//        //    else if (obj.name == "Music Volume Slider" && obj != null)
//        //    {
//        //        audioMixer.GetFloat("musicVolume", out currSliderVolume);

//        //        slider.value = (int)currSliderVolume;
//        //    }
//        //}
//    }

//    // Need to unsubscribe the function after Main Menu loads the Level scene, it is still trying to look at the sliders from the Main Menu
//    private void OnDestroy()
//    {
//        GameManager.Instance.OnOptionsAccessed -= OptionsAccessed;
//    }
//}