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
    public int jumpVelocityStartingLevel = 0;
    public float jumpVelocityUpgradePerLevel = 0.75f;
    public int maxHealthStartingLevel = 0;
    public int maxHealthUpgradePerLevel = 10;
}

[CreateAssetMenu(fileName = "DefualtGameSaveSO", menuName = "Scriptable Objects/DefualtGameSaveSO")]
public class DefualtGameSaveSO : ScriptableObject
{
    [Header("Player Defualts")] public int startingNewMoney = 0;

    [Header("Upgrade Defualts   ")] public PlayerSpaceshipUpgradesSO playerSpaceshipUpgradesSO;

    [Header("Ground Trooper Defaults")] public GroundTrooperUpgradeDefaults groundTrooperDefaults = new GroundTrooperUpgradeDefaults();
}
