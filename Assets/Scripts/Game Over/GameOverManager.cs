using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")] public GameObject gameOverPanel;

    [Header("Buttons")] public string levelSelectScene = "Level Selector 2";
    public string mainMenuScene = "MainMenu";

    void Start()
    {
        // Make sure game over panel is visible
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Unpause the game if it was paused
        Time.timeScale = 1f;

        // check if it is from the ground level
    }

    // Called by Retry button
    public void RetryLevel()
    {
        SceneLoader.Instance.LoadNextScene("Tutorial Ground");
    }

    public void RetrySpaceLevel()
    {
        SceneLoader.Instance.LoadNextScene("Space Level 1");
    }

    public void RetryLastLevel()
    {
        SaveManager.instance.isLoadingSaveData = false;
        SceneLoader.Instance.LoadNextScene(SaveManager.instance.CurrentLevelSaveData.currLevel);
    }

    // Called by Level Select button
    public void GoToLevelSelect()
    {
        SaveManager.instance.CurrentLevelSaveData.currLevel = "Level Selector 2";
        SceneLoader.Instance.LoadNextScene("Level Selector 2");
    }

    // Called by Main Menu button
    public void GoToMainMenu()
    {
        SaveManager.instance.SaveGame();
        SceneLoader.Instance.LoadNextScene("Menus");
    }
}