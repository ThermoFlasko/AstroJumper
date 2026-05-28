using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Build your project on multiple platforms in a single action
public static class BuildAll
{
    [MenuItem("Build/Build All Platforms")]
    public static void BuildAllPlatforms()
    {
        string[] buildScenes =
        {
            "Assets/Level/Scenes/Main Menu/Menus.unity",
            "Assets/Level/Scenes/Loading Screen/LoadingScreen.unity",
            "Assets/Level/Scenes/Tutorial/Tutorial Ground.unity",
            "Assets/Level/Scenes/Space/Space Level 1.unity",
            "Assets/Level/Scenes/Game Over/GameOver.unity",
            "Assets/Level/Scenes/PauseMenu/PauseMenu.unity",
            "Assets/Level/Scenes/You Win/YouWin.unity",
            "Assets/Level/Scenes/Level Selector/Level Selector 2.unity",
            "Assets/Level/Scenes/Planet Levels/Planet 1.unity",
            "Assets/Level/Scenes/Ground/Boss scene/Boss Scene.unity",
            "Assets/Level/Scenes/Ground/Procedural/PCG_Sample.unity",
            "Assets/Level/Scenes/Level Selector/Level Select B.unity"
        };

        string projectName = "Astrojumper";

        // Build for Windows
        BuildReport Win32 = BuildPipeline.BuildPlayer(buildScenes, $"Builds/Windows32/{projectName}.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        BuildSummary summary1 = Win32.summary;

        // Build for Windows
        BuildReport Win64 = BuildPipeline.BuildPlayer(buildScenes, $"Builds/Windows64/{projectName}.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        BuildSummary summary2 = Win64.summary;

        // Build for macOS
        BuildReport Mac = BuildPipeline.BuildPlayer(buildScenes, $"Builds/macOS/{projectName}.app", BuildTarget.StandaloneOSX, BuildOptions.None);
        BuildSummary summary3 = Mac.summary;

        if (summary1.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary1.totalSize + " bytes");
        }

        if (summary1.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }

        if (summary2.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary2.totalSize + " bytes");
        }

        if (summary2.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }

        if (summary3.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary3.totalSize + " bytes");
        }

        if (summary3.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
}