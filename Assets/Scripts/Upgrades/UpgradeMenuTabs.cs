using UnityEngine;

public class UpgradeMenuTabs : MonoBehaviour
{
    public enum UpgradeMenuPage
    {
        Spaceship,
        Player,
    }

    [SerializeField] private GameObject spaceshipPage;
    [SerializeField] private GameObject playerPage;
    [SerializeField] private UpgradeMenuPage defaultPage = UpgradeMenuPage.Spaceship;
    [SerializeField] private UpgradeMenuPage currentPage = UpgradeMenuPage.Spaceship;

    private void Awake()
    {
        ShowPage(defaultPage);
    }

    private void OnEnable()
    {
        ShowPage(currentPage);
    }

    public void ShowSpaceshipPage()
    {
        ShowPage(UpgradeMenuPage.Spaceship);
    }

    public void ShowPlayerPage()
    {
        ShowPage(UpgradeMenuPage.Player);
    }

    public void ShowPage(UpgradeMenuPage page)
    {
        currentPage = page;

        if (spaceshipPage != null)
            spaceshipPage.SetActive(page == UpgradeMenuPage.Spaceship);

        if (playerPage != null)
            playerPage.SetActive(page == UpgradeMenuPage.Player);

        RefreshButtonsForActivePage();
    }

    private void RefreshButtonsForActivePage()
    {
        GameObject activePage = currentPage == UpgradeMenuPage.Spaceship ? spaceshipPage : playerPage;
        if (activePage == null)
            return;

        UpgradeButton[] buttons = activePage.GetComponentsInChildren<UpgradeButton>(true);
        foreach (UpgradeButton button in buttons)
            button?.RefreshView();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            ShowPage(currentPage);
    }
#endif
}
