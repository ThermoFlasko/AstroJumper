using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayscaleToggle : MonoBehaviour
{
    [SerializeField] private Volume grayscaleVolume;

    private ColorAdjustments colorAdjustments;
    private bool grayscaleEnabled = false;

    private void Awake()
    {
        if (grayscaleVolume == null)
        {
            Debug.LogError("Grayscale Volume is not assigned.");
            return;
        }

        if (!grayscaleVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("Color Adjustments override not found on the assigned Volume profile.");
            return;
        }

        SetGrayscale(false);
    }

    public void ToggleGrayscale()
    {
        SetGrayscale(!grayscaleEnabled);
    }

    public void SetGrayscale(bool enabled)
    {
        grayscaleEnabled = enabled;

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.Override(enabled ? -100f : 0f);
        }
    }
}