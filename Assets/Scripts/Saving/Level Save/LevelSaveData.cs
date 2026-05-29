using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;


[Serializable]
public class LevelSaveData
{
    public string currLevel = "";
    public bool isPlanetLevel = false;
    public PlanetLevelData planetLevelData = new PlanetLevelData();
    public SpaceLevelData spaceLevelData = new SpaceLevelData();
    public List<string> completedEvents = new List<string>();

    public static LevelSaveData CreateDefaultSaveData()
    {
        return new LevelSaveData();
    }

    public void UpdateCompletedEvents(string completedEvent)
    {
        completedEvents.Add(completedEvent);
    }

    public void UpdatePlanetLevelData(PlanetLevelData levelData)
    {
        planetLevelData = levelData;
    }

    public void UpdateSpaceLevelData(SpaceLevelData levelData)
    {
        spaceLevelData = levelData;
    }
}

[Serializable]
public class PlanetLevelData
{
    public Vector3 playerPosition = new Vector3(0,0,0);
    public List<MeleeSaveData> meleeEnemies = new();
    public int totalMeleeEnemies = 0;
    public List<RangedSaveData> rangedEnemies = new();
    public int totalRangedEnemies = 0;
}

[Serializable]
public class SpaceLevelData
{
    public Vector3 playerPosition;
    public string levelName;
}
