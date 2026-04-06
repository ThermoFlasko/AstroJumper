using UnityEngine;

public class UpgradeMenuLauncher : MonoBehaviour
{
    [SerializeField] private UpgradeMenu existingMenuInstance;
    [SerializeField] private GameObject[] hideWhileMenuOpen;

    private void Awake()
    {
        SubscribeToMenu(existingMenuInstance);
    }

    private void OnDestroy()
    {
        if (existingMenuInstance != null)
            existingMenuInstance.MenuClosed -= HandleMenuClosed;
    }

    public void OpenMenu()
    {
        if (existingMenuInstance == null)
        {
            Debug.LogWarning($"{nameof(UpgradeMenuLauncher)} on {name} has no scene menu assigned.", this);
            return;
        }

        SetHiddenObjects(false);
        existingMenuInstance.OpenMenu();
    }

    private void SubscribeToMenu(UpgradeMenu menu)
    {
        if (menu == null)
            return;

        menu.MenuClosed -= HandleMenuClosed;
        menu.MenuClosed += HandleMenuClosed;
    }

    private void HandleMenuClosed()
    {
        SetHiddenObjects(true);
    }

    private void SetHiddenObjects(bool visible)
    {
        if (hideWhileMenuOpen == null)
            return;

        foreach (GameObject target in hideWhileMenuOpen)
        {
            if (target != null)
                target.SetActive(visible);
        }
    }
}
