using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoSettings : MonoBehaviour
{

    //THESE SETTINGS AUTO APPLY, a better solution should be to confirm changes

    //current solutions for auto looping settings at the moment is not very optimized

    //does not account for a any fallback should a monitor not be capable of showcasing 1920x1080

    public enum DisplayModes
    {
        Fullscreen,
        Maximized,
        Windowed
    }

    [Header("Display Modes")]
    public DisplayModes currentDisplayMode = DisplayModes.Fullscreen;
    public List<string> displayModeStrings;
    public TextMeshProUGUI displayModeText;

    [Header("Resolutions")]
    public int currentResolution;
    public List<StoredResolution> storedResolutions;
    public TextMeshProUGUI resolutionText;

    public StoredResolution defaultResolution = new()
    {
        resolutionString = "1920 x 1080",
        width = 1920,
        height = 1080
    };

    private void Awake()
    {
        // every time the game is opened, check if the resolution was ever changed before, if not, set and store defaults
        CheckPlayerPrefResolution();
    }

    private void Start()
    {
        displayModeStrings = new() { "Fullscreen", "Maximized Window", "Windowed" };
        displayModeText.text = displayModeStrings[0];

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

        // look through the list of StoredResolutions, if there is a matching resolution string
        foreach (var res in storedResolutions)
        {
            string currResString = res.resolutionString;

            if (currResString == PlayerPrefs.GetString("Resolution String"))
            {
                currentResolution = storedResolutions.IndexOf(res);
            }
        }

        if(PlayerPrefs.GetString("Resolution String") == null)
        {
            resolutionText.text = $"{defaultResolution.resolutionString}";
        }
        else
        {
            ChangeResolution(currentResolution);
        }

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
        displayModeText.text = displayModeStrings[(int)currentDisplayMode];

        switch (currentDisplayMode)
        {
            case DisplayModes.Fullscreen:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 0);
                break;
            case DisplayModes.Maximized:
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 1);
                break;
            case DisplayModes.Windowed:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 2);
                break;
            default:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 0);
                Debug.LogWarning("Error in changing Display Mode");
                break;
        }
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
            case 0:
                currentDisplayMode = DisplayModes.Fullscreen;
                return FullScreenMode.FullScreenWindow;
            case 1:
                currentDisplayMode = DisplayModes.Maximized;
                return FullScreenMode.MaximizedWindow;
            case 2:
                currentDisplayMode = DisplayModes.Windowed;
                return FullScreenMode.Windowed;
            default:
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

        Screen.SetResolution(storedResolutions[resolutionIndex].width, storedResolutions[resolutionIndex].height, GetFullScreenMode());
    }


    public void CheckPlayerPrefResolution()
    {
        if (PlayerPrefs.GetInt("Screenmanager Resolution Width") != 0 && PlayerPrefs.GetInt("Screenmanager Resolution Height") != 0 && PlayerPrefs.GetString("Resolution String") != null)
        {
            print("Found and setting to saved resolution!");
            Screen.SetResolution(PlayerPrefs.GetInt("Screenmanager Resolution Width"), PlayerPrefs.GetInt("Screenmanager Resolution Height"), GetFullScreenModeFromPlayerPref());
            return;
        }

        if (PlayerPrefs.GetInt("Screenmanager Resolution Width") == 0 || PlayerPrefs.GetInt("Screenmanager Resolution Height") == 0 || PlayerPrefs.GetString("Resolution String") == null)
        {
            print("No saved resolutions found, setting to default resolution!");
            SetDefaultVideoSettings();
        }
    }

    // this would only get called if either the player hits reset on video settings or if there is no default resolution
    public void SetDefaultVideoSettings()
    {
        PlayerPrefs.SetString("Resolution String", defaultResolution.resolutionString);
        PlayerPrefs.SetInt("Screenmanager Resolution Width", defaultResolution.width);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", defaultResolution.height);
        PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 0);
        Screen.SetResolution(defaultResolution.width, defaultResolution.height, FullScreenMode.FullScreenWindow);
    }

}

[System.Serializable]
public class StoredResolution
{
    public string resolutionString;
    public int width;
    public int height;
}
