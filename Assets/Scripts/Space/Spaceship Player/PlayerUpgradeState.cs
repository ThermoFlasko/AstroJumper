using UnityEngine;


//This needs to be renamed, its really just getting the level and ettings for what each level means and defiening the upgrades we have
//More like a PlayerUpgradeStatsInfo
public class PlayerUpgradeState : MonoBehaviour
{
    public enum UpgradeType
    {
        MoveForce,
        MaxSpeed,
        BoostForce,
        BarrelRollDistance,
        BarrelRollSpeed,
        FireRate,
        MaxHealth,
        MaxShields,
        
    }

    [SerializeField] private PlayerSpaceshipUpgradesSO upgradesSO;


    public PlayerSpaceshipUpgradesSO Defs => upgradesSO;

    /// <summary>
    /// Uses the upgrade type to get the players level form save manager and the player upgrade so to find the correct boost amount
    /// </summary>
    /// <param name="upgradeType"></param>
    /// <returns></returns>
    public float GetUpgradeBoost(UpgradeType upgradeType)
    {
        if (SaveManager.instance == null || upgradesSO == null)
            return 0f;

        int level = Mathf.Max(0, SaveManager.instance.GetUpgradeLevel(upgradeType));

        switch (upgradeType)
        {
            case UpgradeType.MoveForce:
                return level * upgradesSO.moveForceUpgradePerLevel;
            case UpgradeType.MaxSpeed:
                return level * upgradesSO.maxSpeedUpgradePerLevel;
            case UpgradeType.BoostForce:
                return level * upgradesSO.boostForceUpgradePerLevel;
            case UpgradeType.BarrelRollDistance:
                return level * upgradesSO.barrelRollDistanceUpgradePerLevel;
            case UpgradeType.BarrelRollSpeed:
                return level * upgradesSO.barrelRollSpeedUpgradePerLevel;
            case UpgradeType.FireRate:
                return level * upgradesSO.fireRateUpgradePerLevel;
            case UpgradeType.MaxHealth:
                return level * upgradesSO.maxHealthUpgradePerLevel;
            case UpgradeType.MaxShields:
                return level * upgradesSO.maxShieldsPerLevel;
        }

        return 0f;
    }
}
