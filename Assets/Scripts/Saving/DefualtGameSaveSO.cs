using System;
using UnityEngine;

public enum GroundTrooperUpgradeType
{
    MoveSpeed,
    JumpVelocity,
    MaxHealth,
}

[Serializable]
public class GroundTrooperUpgradeDefaults
{
    public int universalUpgradeCostPerLevel = 5;
    public int moveSpeedStartingLevel = 1;
    public float moveSpeedUpgradePerLevel = 0.75f;
    [Min(0)] public int moveSpeedMaxLevel = 5;
    public int jumpVelocityStartingLevel = 0;
    public float jumpVelocityUpgradePerLevel = 0.75f;
    [Min(0)] public int jumpVelocityMaxLevel = 5;
    public int maxHealthStartingLevel = 0;
    public int maxHealthUpgradePerLevel = 10;

    [Min(0)] public int maxHealthMaxLevel = 5;

    public int GetMaxUpgradeLevel(GroundTrooperUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case GroundTrooperUpgradeType.MoveSpeed:
                return Mathf.Max(0, moveSpeedMaxLevel);
            case GroundTrooperUpgradeType.JumpVelocity:
                return Mathf.Max(0, jumpVelocityMaxLevel);
            case GroundTrooperUpgradeType.MaxHealth:
                return Mathf.Max(0, maxHealthMaxLevel);
        }

        return 0;
    }

    public int ClampUpgradeLevel(GroundTrooperUpgradeType upgradeType, int level)
    {
        return Mathf.Clamp(level, 0, GetMaxUpgradeLevel(upgradeType));
    }
}

[Serializable]
public class GroundEquipmentDefaults
{
    public string equippedMeleeAttackId;
    public string equippedRangedAttackId;
}

[CreateAssetMenu(fileName = "DefualtGameSaveSO", menuName = "Scriptable Objects/DefualtGameSaveSO")]
public class DefualtGameSaveSO : ScriptableObject
{
    [Header("Player Defualts")] public int startingNewMoney = 0;

    [Header("Upgrade Defualts   ")] public PlayerSpaceshipUpgradesSO playerSpaceshipUpgradesSO;

    [Header("Ground Trooper Defaults")] public GroundTrooperUpgradeDefaults groundTrooperDefaults = new GroundTrooperUpgradeDefaults();

    [Header("Ground Equipment Defaults")] public GroundEquipmentDefaults groundEquipmentDefaults = new GroundEquipmentDefaults();
}
