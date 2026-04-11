using UnityEngine;
using UnityEngine.InputSystem;


public class LevelInputManager : MonoBehaviour
{

    [SerializeField] private InputActionAsset actionsAsset;
    [SerializeField] private string menuActionName = "Menu";
    private InputAction menuAction;

    public GamePauseTrigger pauseManager;

    private void OnEnable()
    {
        menuAction.Enable();
        menuAction.performed += OpenMenu;
    }

    private void OnDisable()
    {
        menuAction.Disable();
        menuAction.performed -= OpenMenu;
    }

    void Awake()
    {
        var map = actionsAsset.FindActionMap("UI", true);
        menuAction = map.FindAction(menuActionName);
    }   

    private void OpenMenu(InputAction.CallbackContext context)
    {
        Debug.Log("Menu button pressed, opening menu...");
        pauseManager.PauseGame();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
