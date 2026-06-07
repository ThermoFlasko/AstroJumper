using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackScriptRework : MonoBehaviour
{

   [Header("Player Hitbox Attack Prefabs")]
   [SerializeField] public GameObject BlasterPrefab;
   [SerializeField] public GameObject EnergyAxePrefab;
   [SerializeField] public GameObject GrenadeLauncherPrefab;
   [SerializeField] public GameObject PhaserPulsePrefab;

   //Set of projectiles that players has
   [SerializeField] public ProjectilePool PlayerProjectilePool;

   [SerializeField] private InputActionAsset actionsAsset; //this is jsut to test, will move to GroundMovement when it is updated
   [SerializeField] private string actionMapName = "Player";
   [SerializeField] private string attackActionName = "Attack";
   [SerializeField] private string attackActionName2 = "Attack2";
   [SerializeField] private GroundAttackCatalogSO groundAttackCatalog;

   private InputAction attackAction;
   private InputAction attackAction2;
   private bool isAttacking2 = false;









   // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Awake()
    {

      var map = actionsAsset.FindActionMap(actionMapName, true);
      attackAction = map.FindAction(attackActionName);
      if (attackAction == null)
      {
         Debug.LogError("Player: Attack action not found in the InputActionAsset.");
      }
      attackAction2 = map.FindAction(attackActionName2);

      if (attackAction2 == null)
      {
         Debug.LogError("Player: Attack2 action not found in the InputActionAsset.");
      }
     // RefreshGroundAttackLoadout();
    //  ApplyGroundTrooperDefaultUpgrades();
   }



   private void PrimaryAttack(InputAction.CallbackContext context)
   {

   }

    
    
}
