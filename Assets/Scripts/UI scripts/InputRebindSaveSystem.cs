using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindSaveSystem : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private const string REBINDS_KEY = "InputRebinds";

    private void Awake()
    {
        LoadRebinds();
    }



    public void SaveRebinds()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActions not assigned");
            return;
        }

        string json = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(REBINDS_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Keybinds saved");
    }



    public void LoadRebinds()
    {
        if (!PlayerPrefs.HasKey(REBINDS_KEY))
        {
            Debug.Log("No saved keybinds found");
            return;
        }

        string json = PlayerPrefs.GetString(REBINDS_KEY);
        inputActions.LoadBindingOverridesFromJson(json);
        Debug.Log("Keybinds loaded");
    }

    public void ResetRebinds()
    {
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(REBINDS_KEY);
        Debug.Log("Keybinds reset");
    }
}