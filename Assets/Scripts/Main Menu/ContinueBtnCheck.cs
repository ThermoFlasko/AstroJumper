using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContinueBtnCheck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelSaveData levelSaveData = SaveManager.instance.GetCurrentLevelData();
        if (levelSaveData.currLevel == "")
        {
            Image image = gameObject.GetComponent<Image>();
            Color c = image.color;
            c.a = 0.3f;
            image.color = c;

            TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            Color tc = text.color;
            tc.a = 0.3f;

            text.color = tc;

            Button btn = gameObject.GetComponent<Button>();
            btn.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
