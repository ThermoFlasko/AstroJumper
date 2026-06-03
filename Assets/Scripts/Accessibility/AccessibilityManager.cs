using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AccessibilityManager : MonoBehaviour
{
    [SerializeField] private Volume volume;

    private ColorAdjustments colorAdjustments;
    private bool grayscaleOn = false;

    private void Start()
    {
        if (volume.profile.TryGet(out colorAdjustments))
        {
            SetGrayscale(false);
        }
    }

    public void ToggleGrayscale()
    {
        grayscaleOn = !grayscaleOn;
        SetGrayscale(grayscaleOn);
    }

    public void SetGrayscale(bool enabled)
    {
        if (colorAdjustments == null) return;

        colorAdjustments.saturation.Override(enabled ? -100f : 0f);
    }
}