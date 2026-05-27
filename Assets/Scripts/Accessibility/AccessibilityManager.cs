using UnityEngine;

public class AccessibilityManager : MonoBehaviour
{
    public GameObject grayscaleOverlay;

    public void ToggleGrayscale()
    {
        grayscaleOverlay.SetActive(!grayscaleOverlay.activeSelf);
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}