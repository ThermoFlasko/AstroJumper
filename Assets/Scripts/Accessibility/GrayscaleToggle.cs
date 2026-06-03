using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GrayscaleToggle : MonoBehaviour
{
    public static GrayscaleToggle Instance { get; private set; }

    [SerializeField] private Volume grayscaleVolume;

    private ColorAdjustments colorAdjustments;
    private bool grayscaleEnabled;

    private const string GrayscalePrefKey = "GrayscaleEnabled";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        grayscaleEnabled = PlayerPrefs.GetInt(GrayscalePrefKey, 0) == 1;

        SetupVolume();
        SetGrayscale(grayscaleEnabled);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupVolume();
        SetGrayscale(grayscaleEnabled);
    }

    private void SetupVolume()
    {
        if (grayscaleVolume == null)
        {
            grayscaleVolume = FindFirstObjectByType<Volume>();
        }

        if (grayscaleVolume == null)
        {
            Debug.LogWarning("No Volume found for grayscale.");
            return;
        }

        if (!grayscaleVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogWarning("Color Adjustments override not found on the Volume profile.");
        }
    }

    public void ToggleGrayscale()
    {
        SetGrayscale(!grayscaleEnabled);
    }

    public void SetGrayscale(bool enabled)
    {
        grayscaleEnabled = enabled;

        PlayerPrefs.SetInt(GrayscalePrefKey, grayscaleEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.Override(grayscaleEnabled ? -100f : 0f);
        }
    }

    public bool IsGrayscaleEnabled()
    {
        return grayscaleEnabled;
    }
}