using UnityEngine;
using UnityEngine.UI;

public class TrackAccessibility : MonoBehaviour
{

    private Button button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(FindAccessibilityManager);
    }

    private void FindAccessibilityManager()
    {
        GrayscaleToggle.Instance.ToggleGrayscale();
        Debug.Log("BRAAAH");
    }
}
