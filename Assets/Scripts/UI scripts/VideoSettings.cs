using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoSettings : MonoBehaviour
{
    public enum DisplayModes
    {
        Fullscreen,
        Borderless,
        Windowed
    }

    public DisplayModes currentDisplayMode = DisplayModes.Fullscreen;
    public List<string> displayModeStrings;
    public TextMeshProUGUI displayModeText;


    public List<string> resolutions = new();

    private void Awake()
    {
        displayModeStrings = new() { "Fullscreen", "Maximized Window", "Windowed" };
        // set the standard resolution size before anything else
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        displayModeText.text = displayModeStrings[0];
    }

    private void Start()
    {
        resolutions = new();
        Resolution[] possibleRes = Screen.resolutions;

        // Print the resolutions
        foreach (var res in possibleRes)
        {
            string unregisteredRes = $"{res.width} x {res.height}";
            float resolutionRatio = (float)res.width / (float)res.height;

            // 16:10, 16:9, and 21:9 ratios ONLY
            if (resolutionRatio == 1.6f || (resolutionRatio >= 1.775f && resolutionRatio <= 1.81f) || (resolutionRatio >= 2.33f && resolutionRatio <= 2.4f))
            {
                resolutions.Add(unregisteredRes);
            }

        }
    }

    public void ChangeDisplayModeBack()
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
        ChangeDisplayModeText((int)currentDisplayMode);
    }

    public void ChangeDisplayModeForward()
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
        ChangeDisplayModeText((int)currentDisplayMode);
    }

    public void ChangeDisplayModeText(int modeNumber)
    {
        displayModeText.text = displayModeStrings[modeNumber];
    }
}
