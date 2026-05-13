using UnityEngine;

public class UpgradeMenuTabs : MonoBehaviour
{
    public enum UpgradeMenuPage
    {
        Spaceship,
        Player,
        PlayerWeapons,
    }

    [SerializeField] private GameObject spaceshipPage;
    [SerializeField] private GameObject playerPage;
    [SerializeField] private GameObject playerWeaponsPage;

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

    public void ShowPlayerWeaponsPage()
    {
        ShowPage(UpgradeMenuPage.PlayerWeapons);
    }

    public void ShowPage(UpgradeMenuPage page)
    {
        currentPage = page;

        if (spaceshipPage != null)
            spaceshipPage.SetActive(page == UpgradeMenuPage.Spaceship);

        if (playerPage != null)
            playerPage.SetActive(page == UpgradeMenuPage.Player);

        if (playerWeaponsPage != null)
            playerWeaponsPage.SetActive(page == UpgradeMenuPage.PlayerWeapons);

        RefreshButtonsForActivePage();
    }

    private void RefreshButtonsForActivePage()
    {
        GameObject activePage = GetActivePage();
        if (activePage == null)
            return;

        UpgradeButton[] buttons = activePage.GetComponentsInChildren<UpgradeButton>(true);
        foreach (UpgradeButton button in buttons)
            button?.RefreshView();
    }

    private GameObject GetActivePage()
    {
        switch (currentPage)
        {
            case UpgradeMenuPage.Spaceship:
                return spaceshipPage;
            case UpgradeMenuPage.Player:
                return playerPage;
            case UpgradeMenuPage.PlayerWeapons:
                return playerWeaponsPage;
            default:
                return spaceshipPage;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            ShowPage(currentPage);
    }
#endif
}
