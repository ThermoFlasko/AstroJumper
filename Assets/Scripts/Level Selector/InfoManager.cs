using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

// NOTE: csv files can be found in Assets/Level/Prefabs/Level Selector/Planet CSV

public class InfoManager : MonoBehaviour
{
    public TextAsset textAssetData;
    public TextAsset EnText;
    public TextAsset CnText;
    public TextAsset KorText;
    public string currentLanguage = "en";
    public GameObject[] planets;

    void Awake()
    {
        SetLanguage(PlayerPrefs.GetString("SelectedLanguage", "en"));
        readCSV();
        
    }

    void Start()
    {



    }

    public void readCSV()
    {
        string[] data = textAssetData.text.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < planets.Length; i++)
        {
            Planet planet = planets[i].GetComponent<Planet>();
            planet.planetName = data[(i + 1)];
            planet.planetDescription = data[(i + 1) + 5 * 1];
            //planet.resources = data[(i + 1) + 5 * 2];
            //planet.dificulty = data[(i + 1) + 5 * 3];
            //planet.faction = data[(i + 1) + 5 * 4];
            print($"at i: {i} {data[(i + 1) + 4 * 5]}");
            planet.sceneToLoad = data[(i + 1) + 5 * 5];
            planet.displayName();
        }
    }

    public void SetLanguage(string localeCode)
    {
        
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;

            PlayerPrefs.SetString("SelectedLanguage", localeCode);
            Debug.Log("Language set to: " + localeCode);
        }

        if (!PlayerPrefs.HasKey("SelectedLanguage"))
        {
            PlayerPrefs.SetString("SelectedLanguage", "en");
        }

        currentLanguage = PlayerPrefs.GetString("SelectedLanguage", "en"); 
        print(currentLanguage);

        if (currentLanguage == "zh-Hans")
        {
            textAssetData = CnText;
        }
        else if (currentLanguage == "en")
        {
            textAssetData = EnText;
        }
        else if (currentLanguage == "ko")
        {
            textAssetData = KorText;
        }
    } 
}
