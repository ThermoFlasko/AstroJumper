using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panels")] public GameObject gameOverPanel;

    [Header("Buttons")] public string levelSelectScene = "LevelSelect";
    public string mainMenuScene = "MainMenu";

    void Start()
    {
        // Make sure game over panel is visible
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Unpause the game if it was paused
        Time.timeScale = 1f;
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

    // Called by Level Select button
    public void GoToLevelSelect()
    {
        SceneLoader.Instance.LoadNextScene("LevelSelect");
    }

    // Called by Main Menu button
    public void GoToMainMenu()
    {
        SceneLoader.Instance.LoadNextScene("Menus");
    }
}