using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoSettings : MonoBehaviour
{

    //THESE SETTINGS AUTO APPLY, a better solution should be to confirm changes

    //current solutions for auto looping settings at the moment is not very optimized

    //does not account for a any fallback should a monitor not be capable of showcasing 1920x1080

    // the start is gonna run anytime options are checked right now

    public enum DisplayModes
    {
        Fullscreen,
        Maximized,
        Windowed
    }

    [Header("Display Modes")]
    public DisplayModes currentDisplayMode = DisplayModes.Fullscreen;
    public List<TextMeshProUGUI> displayModeTextObj;
    public GameObject activeDisplayModeObject;

    [Header("Resolutions")]
    public int currentResolution;
    public List<StoredResolution> storedResolutions;
    public TextMeshProUGUI resolutionText;

    public StoredResolution defaultResolution;

    private void Awake()
    {
        defaultResolution = new()
        {
            resolutionString = $"{PlayerPrefs.GetInt("Screenmanager Resolution Width Default", 1920)} x {PlayerPrefs.GetInt("Screenmanager Resolution Width Default", 1080)}",
            width = PlayerPrefs.GetInt("Screenmanager Resolution Width Default", 1920),
            height = PlayerPrefs.GetInt("Screenmanager Resolution Width Default", 1080)
        };

        // every time the game is opened, check if the resolution was ever changed before, if not, set and store defaults
        CheckPlayerPrefResolution();

        activeDisplayModeObject = displayModeTextObj[0].gameObject;
        activeDisplayModeObject.SetActive(true);

        storedResolutions = new();

        Resolution[] possibleRes = Screen.resolutions;

        // Print the resolutions
        foreach (var res in possibleRes)
        {
            float resolutionRatio = (float)res.width / (float)res.height;

            // 16:10, 16:9, and 21:9 ratios ONLY
            if (resolutionRatio == 1.6f || (resolutionRatio >= 1.775f && resolutionRatio <= 1.81f) || (resolutionRatio >= 2.33f && resolutionRatio <= 2.4f))
            {
                StoredResolution newRes = new()
                {
                    resolutionString = $"{res.width} x {res.height}",
                    width = res.width,
                    height = res.height
                };

                storedResolutions.Add(newRes);
            }
        }

        // look through the list of possible resolutions and store the index so the resolution can be adjusted
        foreach (var res in storedResolutions)
        {
            string currResString = res.resolutionString;

            if (currResString == PlayerPrefs.GetString("Resolution String"))
            {
                currentResolution = storedResolutions.IndexOf(res);
            }
        }

        // check if the PlayerPref in the registry exists already, if not set the default string, if it is then set the resolution
        if (PlayerPrefs.GetString("Resolution String") == string.Empty)
        {
            SetDefaultVideoSettings();
            return;
        }

        ChangeResolution(currentResolution);
        GetFullScreenModeFromPlayerPref();
        ChangeDisplayMode();
    }

    public void ChangeDisplayModeTextBack()
    {
        //if we at left end
        if (currentDisplayMode == DisplayModes.Fullscreen)
        {
            currentDisplayMode = DisplayModes.Windowed;
        }
        else
        {
            currentDisplayMode--;
        }
        ChangeDisplayMode();
    }

    public void ChangeDisplayModeTextForward()
    {
        // if we at right end
        if(currentDisplayMode == DisplayModes.Windowed)
        {
            currentDisplayMode = DisplayModes.Fullscreen;
        }
        else
        {
            currentDisplayMode++;
        }
        ChangeDisplayMode();
    }

    public void ChangeResolutionTextBack()
    {
        currentResolution--;
        if(currentResolution < 0)
        {
            currentResolution = storedResolutions.Count - 1;
        }

        ChangeResolution(currentResolution);
    }

    public void ChangeResolutionTextForward()
    {
        currentResolution++;
        if(currentResolution > storedResolutions.Count - 1)
        {
            currentResolution = 0;
        }

        ChangeResolution(currentResolution);
    }

    public void ChangeDisplayMode()
    {
        //displayModeText.text = displayModeStrings[(int)currentDisplayMode];

        activeDisplayModeObject.SetActive(false);
        activeDisplayModeObject = displayModeTextObj[(int)currentDisplayMode].gameObject;
        activeDisplayModeObject.SetActive(true);

        switch (currentDisplayMode)
        {
            case DisplayModes.Fullscreen:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 1);
                break;
            case DisplayModes.Maximized:
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 2);
                currentResolution = storedResolutions.Count - 1;
                ChangeResolution(currentResolution);
                break;
            case DisplayModes.Windowed:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 3);
                break;
            default:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 1);
                Debug.LogWarning("Error in changing Display Mode");
                break;
        }

        PlayerPrefs.Save();
    }

    public FullScreenMode GetFullScreenMode()
    {
        switch (currentDisplayMode)
        {
            case DisplayModes.Fullscreen:
                return FullScreenMode.FullScreenWindow;
            case DisplayModes.Maximized:
                return FullScreenMode.MaximizedWindow;
            case DisplayModes.Windowed:
                return FullScreenMode.Windowed;
            default:
                Debug.LogWarning("Error in changing Display Mode");
                return FullScreenMode.FullScreenWindow;
        }
    }
    
    public FullScreenMode GetFullScreenModeFromPlayerPref()
    {
        switch (PlayerPrefs.GetInt("Screenmanager Fullscreen mode"))
        {
            case 1:
                currentDisplayMode = DisplayModes.Fullscreen;
                return FullScreenMode.FullScreenWindow;
            case 2:
                currentDisplayMode = DisplayModes.Maximized;
                return FullScreenMode.MaximizedWindow;
            case 3:
                currentDisplayMode = DisplayModes.Windowed;
                return FullScreenMode.Windowed;
            default:
                currentDisplayMode = DisplayModes.Fullscreen;
                Debug.LogWarning("Error in locating PlayerPref Display Mode");
                return FullScreenMode.FullScreenWindow;
        }
    }

    public void ChangeResolution(int resolutionIndex)
    {
        resolutionText.text = storedResolutions[resolutionIndex].resolutionString;

        PlayerPrefs.SetString("Resolution String", storedResolutions[resolutionIndex].resolutionString);
        PlayerPrefs.SetInt("Screenmanager Resolution Width", storedResolutions[resolutionIndex].width);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", storedResolutions[resolutionIndex].height);
        PlayerPrefs.Save();

        Screen.SetResolution(storedResolutions[resolutionIndex].width, storedResolutions[resolutionIndex].height, GetFullScreenMode());
    }


    public void CheckPlayerPrefResolution()
    {
        if (PlayerPrefs.GetInt("Screenmanager Resolution Width") != 0 && PlayerPrefs.GetInt("Screenmanager Resolution Height") != 0 && PlayerPrefs.GetString("Resolution String") != string.Empty)
        {
            print("Found and setting to saved resolution!");
            Screen.SetResolution(PlayerPrefs.GetInt("Screenmanager Resolution Width"), PlayerPrefs.GetInt("Screenmanager Resolution Height"), GetFullScreenModeFromPlayerPref());
            return;
        }

        if (PlayerPrefs.GetInt("Screenmanager Resolution Width", 0) == 0 || PlayerPrefs.GetInt("Screenmanager Resolution Height", 0) == 0 || PlayerPrefs.GetString("Resolution String", string.Empty) == string.Empty)
        {
            print("No saved resolutions found, setting to default resolution!");
            SetDefaultVideoSettings();
        }
    }

    // this would only get called if either the player hits reset on video settings or if this is the first time opening the game
    public void SetDefaultVideoSettings()
    {
        PlayerPrefs.SetString("Resolution String", defaultResolution.resolutionString);
        PlayerPrefs.SetInt("Screenmanager Resolution Width", defaultResolution.width);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", defaultResolution.height);
        resolutionText.text = $"{defaultResolution.resolutionString}";

        currentDisplayMode = DisplayModes.Fullscreen;
        ChangeDisplayMode();

        PlayerPrefs.Save();
        Screen.SetResolution(PlayerPrefs.GetInt("Screenmanager Resolution Width", defaultResolution.width), PlayerPrefs.GetInt("Screenmanager Resolution Height", defaultResolution.height), FullScreenMode.FullScreenWindow);
    }

}

[System.Serializable]
public class StoredResolution
{
    public string resolutionString;
    public int width;
    public int height;
}
