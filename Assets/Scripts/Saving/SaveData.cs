using UnityEngine;
using System;
using Unity.VisualScripting;

[System.Serializable]
public class SaveData
{
    //Any time we make changes pdate this so the game nows theres no information to be saved 
    public const float CurrentVersion = 0.8f;

    public float version = CurrentVersion;

    public int newMoney; //Important

    //------Upgrades (Player Spaceship current levels)-------
    public SpaceshipUpgradeData spaceshipUpgradeData = new SpaceshipUpgradeData();
    public GroundTrooperUpgradeData groundTrooperUpgradeData = new GroundTrooperUpgradeData();
    public GroundEquipmentData groundEquipmentData = new GroundEquipmentData();
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

    [Serializable]
    public class GroundEquipmentData
    {
        public string equippedMeleeAttackId;
        public string equippedRangedAttackId;
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

        if (groundEquipmentData == null)
        {
            groundEquipmentData = CreateDefaultGroundEquipmentData(defaults != null ? defaults.groundEquipmentDefaults : null);
        }

        ApplyDefaultGroundEquipmentData(groundEquipmentData, defaults != null ? defaults.groundEquipmentDefaults : null);

    }

    public static SaveData CreateDefualtSaveData(DefualtGameSaveSO defaults)
    {
        var d = new SaveData();
        //Init everything to the defualt value
        d.newMoney = (defaults != null) ? defaults.startingNewMoney : 0;
        d.spaceshipUpgradeData = CreateDefaultUpgradeData(defaults != null ? defaults.playerSpaceshipUpgradesSO : null);
        d.groundTrooperUpgradeData = CreateDefaultGroundTrooperUpgradeData(defaults != null ? defaults.groundTrooperDefaults : null);
        d.groundEquipmentData = CreateDefaultGroundEquipmentData(defaults != null ? defaults.groundEquipmentDefaults : null);

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

    private static GroundEquipmentData CreateDefaultGroundEquipmentData(GroundEquipmentDefaults equipmentDefaults)
    {
        var equipmentData = new GroundEquipmentData();
        ApplyDefaultGroundEquipmentData(equipmentData, equipmentDefaults);
        return equipmentData;
    }

    private static void ApplyDefaultGroundEquipmentData(GroundEquipmentData equipmentData, GroundEquipmentDefaults equipmentDefaults)
    {
        if (equipmentData == null || equipmentDefaults == null)
            return;

        if (string.IsNullOrWhiteSpace(equipmentData.equippedMeleeAttackId))
            equipmentData.equippedMeleeAttackId = equipmentDefaults.equippedMeleeAttackId;

        if (string.IsNullOrWhiteSpace(equipmentData.equippedRangedAttackId))
            equipmentData.equippedRangedAttackId = equipmentDefaults.equippedRangedAttackId;
    }
}
