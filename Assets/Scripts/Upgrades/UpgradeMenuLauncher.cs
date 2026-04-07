using UnityEngine;

public class UpgradeMenuLauncher : MonoBehaviour
{
    [SerializeField] private UpgradeMenu existingMenuInstance;
    [SerializeField] private UpgradeMenu menuPrefab;
    [SerializeField] private Transform menuParent;
    [SerializeField] private GameObject[] hideWhileMenuOpen;

    private void Awake()
    {
        existingMenuInstance = ResolveExistingMenu();
        SubscribeToMenu(existingMenuInstance);
        SetHiddenObjects(!IsMenuOpen());
    }

    private void OnDestroy()
    {
        if (existingMenuInstance != null)
            existingMenuInstance.MenuClosed -= HandleMenuClosed;
    }

    public void OpenMenu()
    {
        UpgradeMenu menu = GetOrCreateMenu();
        if (menu == null)
        {
            Debug.LogWarning($"{nameof(UpgradeMenuLauncher)} on {name} could not find or create an upgrade menu.", this);
            return;
        }

        SetHiddenObjects(false);
        menu.OpenMenu();
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

    private UpgradeMenu GetOrCreateMenu()
    {
        if (existingMenuInstance != null)
            return existingMenuInstance;

        existingMenuInstance = ResolveExistingMenu();
        if (existingMenuInstance != null)
        {
            SubscribeToMenu(existingMenuInstance);
            return existingMenuInstance;
        }

        if (menuPrefab == null)
            return null;

        Transform parent = ResolveMenuParent();
        existingMenuInstance = parent != null ? Instantiate(menuPrefab, parent) : Instantiate(menuPrefab);
        existingMenuInstance.name = menuPrefab.name;
        existingMenuInstance.DeactivateGameObjectWhenClosed = true;
        existingMenuInstance.gameObject.SetActive(false);

        SubscribeToMenu(existingMenuInstance);
        return existingMenuInstance;
    }

    private UpgradeMenu ResolveExistingMenu()
    {
        if (existingMenuInstance != null)
            return existingMenuInstance;

        return FindFirstObjectByType<UpgradeMenu>(FindObjectsInactive.Include);
    }

    private Transform ResolveMenuParent()
    {
        if (menuParent != null)
            return menuParent;

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
            return parentCanvas.transform;

        Canvas sceneCanvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        return sceneCanvas != null ? sceneCanvas.transform : null;
    }

    private bool IsMenuOpen()
    {
        return existingMenuInstance != null && existingMenuInstance.IsOpen;
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
