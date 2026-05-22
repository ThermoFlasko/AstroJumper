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
    public Slider volumeSlider;
    public Slider musicSlider;
    public Slider soundSlider;
    public TMP_Text volumeText;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
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
        SceneLoader.Instance.LoadNextScene("Tutorial Ground");
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