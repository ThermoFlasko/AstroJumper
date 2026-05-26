using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;

[System.Serializable]
public class LevelSaveData
{
    public int currLevel = 0;
    public PlanetLevelData planetLevelData = new PlanetLevelData();
    public SpaceLevelData spaceLevelData = new SpaceLevelData();

    public static LevelSaveData CreateDefaultSaveData()
    {
        return new LevelSaveData();
    }

    [Serializable]
    public class PlanetLevelData
    {
        public string levelName = "";
        public Vector3 playerPosition = new Vector3(0,0,0);
        public GameObject[] enemies = null;
        public List<string> completedEvents = new List<string>();
    }

    [Serializable]
    public class SpaceLevelData
    {
        public Vector3 playerPosition;
        public string levelName = "";
    }
}
