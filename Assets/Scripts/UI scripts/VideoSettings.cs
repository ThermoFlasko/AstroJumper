using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoSettings : MonoBehaviour
{

    //THESE SETTINGS AUTO APPLY, a better solution should be to confirm changes

    //current solutions for auto looping settings at the moment is not very optimized

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
        // set the standard resolution size before anything else
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
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
        resolutionText.text = $"{defaultResolution.resolutionString}";

        // look through the list of StoredResolutions, if there is a matching resolution string

        foreach (var res in storedResolutions)
        {
            string currResString = res.resolutionString;

            if (currResString == defaultResolution.resolutionString)
            {
                currentResolution = storedResolutions.IndexOf(res);
            }
        }

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

        displayModeText.text = displayModeStrings[(int)currentDisplayMode];
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

        displayModeText.text = displayModeStrings[(int)currentDisplayMode];
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
        switch (currentDisplayMode)
        {
            case DisplayModes.Fullscreen:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case DisplayModes.Maximized:
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                break;
            case DisplayModes.Windowed:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            default:
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

    public void ChangeResolution(int resolutionIndex)
    {
        resolutionText.text = storedResolutions[resolutionIndex].resolutionString;

        Screen.SetResolution(storedResolutions[resolutionIndex].width, storedResolutions[resolutionIndex].height, GetFullScreenMode());
    }

}

[System.Serializable]
public class StoredResolution
{
    public string resolutionString;
    public int width;
    public int height;
}
