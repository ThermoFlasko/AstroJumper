using NUnit.Framework;
using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    // 0 is false, 1 is true
    public bool isTooltipsActive;
    public TextMeshProUGUI activeText;
    public TextMeshProUGUI inactiveText;

    void Awake()
    {
        // if the key didnt exist, default to on
        if (!PlayerPrefs.HasKey("Tooltips Active"))
        {
            PlayerPrefs.SetInt("Tooltips Active", 1);
            isTooltipsActive = true;
            SwapActiveText();
            return;
        }

        if (PlayerPrefs.GetInt("Tooltips Active") == 0)
        {
            isTooltipsActive = false;
        }
        else
        {
            isTooltipsActive = true;
        }

        SwapActiveText();
    }

    public void ToggleTooltips()
    {
        isTooltipsActive = !isTooltipsActive;

        if (isTooltipsActive)
        {
            PlayerPrefs.SetInt("Tooltips Active", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Tooltips Active", 0); 
        }

        SwapActiveText();
    }

    void SwapActiveText()
    {
        if (isTooltipsActive)
        {
            activeText.gameObject.SetActive(true);
            inactiveText.gameObject.SetActive(false);
        }
        else
        {
            activeText.gameObject.SetActive(false);
            inactiveText.gameObject.SetActive(true);
        }
    }
}
