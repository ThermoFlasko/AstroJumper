using Ilumisoft.HealthSystem;
using UnityEngine;


public enum GroundAttackType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName ="GroundAttackDefinition",menuName ="Scriptable Objects/Ground Attack Definition")]
public class GroundAttackDefinition : ScriptableObject
{
    [SerializeField] private string attackId;
    [SerializeField] private string displayName;
    [SerializeField]private GroundAttackType attackType;
    [SerializeField]private GameObject hitBoxPrefab;
    [SerializeField]private Animator meleeAttackAnimation;
    [SerializeField] private Sprite icon;
    
    public string AttackId=>attackId;
    public string DisplayName=>displayName;
    public GroundAttackType AttackType => attackType;
    public GameObject HitBoxPrefab=>hitBoxPrefab;
    public Animator MeleeAttackAnimation=>meleeAttackAnimation;
    public Sprite Icon=>icon;

}