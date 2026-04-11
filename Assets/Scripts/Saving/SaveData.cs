using UnityEngine;
using System;

[System.Serializable]
public class SaveData
{
    public const float CurrentVersion = 0.68f;

    public float version = CurrentVersion;

    public int newMoney; //Important

    //------Upgrades (Player Spaceship current levels)-------
    public SpaceshipUpgradeData spaceshipUpgradeData = new SpaceshipUpgradeData();
    public GroundTrooperUpgradeData groundTrooperUpgradeData = new GroundTrooperUpgradeData();

    [Serializable]
    public class SpaceshipUpgradeData
    {
        //Needs a matching one for everyu player spachip upgrade
        public int moveForceLevel;
        public int maxSpeedLevel;
        public int boostForceLevel;
        public int barrelRollDistanceLevel;
        public int barrelRollSpeedLevel;
        public int fireRateLevel;
        public int maxShieldsLevel;
        public int maxHealthLevel;
    }

    [Serializable]
    public class GroundTrooperUpgradeData
    {
        public int moveSpeedLevel;
        public int jumpVelocityLevel;
        public int maxHealthLevel;
    }

    public void EnsureInitialized(DefualtGameSaveSO defaults)
    {
        if (spaceshipUpgradeData == null)
        {
            spaceshipUpgradeData = CreateDefaultUpgradeData(defaults != null ? defaults.playerSpaceshipUpgradesSO : null);
        }

        if (groundTrooperUpgradeData == null)
        {
            groundTrooperUpgradeData = CreateDefaultGroundTrooperUpgradeData(defaults != null ? defaults.groundTrooperDefaults : null);
        }
    }

    public static SaveData CreateDefualtSaveData(DefualtGameSaveSO defaults)
    {
        var d = new SaveData();
        //Init everything to the defualt value
        d.newMoney = (defaults != null) ? defaults.startingNewMoney : 0;
        d.spaceshipUpgradeData = CreateDefaultUpgradeData(defaults != null ? defaults.playerSpaceshipUpgradesSO : null);
        d.groundTrooperUpgradeData = CreateDefaultGroundTrooperUpgradeData(defaults != null ? defaults.groundTrooperDefaults : null);

        return d;
    }

    private static SpaceshipUpgradeData CreateDefaultUpgradeData(PlayerSpaceshipUpgradesSO upgradeDefaults)
    {
        var upgradeData = new SpaceshipUpgradeData();

        if (upgradeDefaults != null)
        {
            upgradeData.moveForceLevel = upgradeDefaults.moveForceStartingLevel;
            upgradeData.maxSpeedLevel = upgradeDefaults.maxSpeedStartingLevel;
            upgradeData.boostForceLevel = upgradeDefaults.boostForceStartingLevel;
            upgradeData.barrelRollDistanceLevel = upgradeDefaults.barrelRollDistanceStartingLevel;
            upgradeData.barrelRollSpeedLevel = upgradeDefaults.barrelRollSpeedStartingLevel;
            upgradeData.fireRateLevel = upgradeDefaults.fireRateStartingLevel;
            upgradeData.maxHealthLevel = upgradeDefaults.maxHealthStartingLevel;
            upgradeData.maxShieldsLevel = upgradeDefaults.maxShieldsStartingLevel;
        }

        return upgradeData;
    }

    private static GroundTrooperUpgradeData CreateDefaultGroundTrooperUpgradeData(GroundTrooperUpgradeDefaults upgradeDefaults)
    {
        var upgradeData = new GroundTrooperUpgradeData();

        if (upgradeDefaults != null)
        {
            upgradeData.moveSpeedLevel = upgradeDefaults.moveSpeedStartingLevel;
            upgradeData.jumpVelocityLevel = upgradeDefaults.jumpVelocityStartingLevel;
            upgradeData.maxHealthLevel = upgradeDefaults.maxHealthStartingLevel;
        }

        return upgradeData;
    }
}
