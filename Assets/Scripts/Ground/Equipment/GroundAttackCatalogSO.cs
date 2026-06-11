using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "GroundAttackCatalog", menuName = "Scriptable Object/Ground Attack Catalog")]
public class GroundAttackCatalogSO : ScriptableObject
{
    [SerializeField] private List<GroundAttackDefinition> attacks = new List<GroundAttackDefinition>();

    [Header("Defualts")]
    [SerializeField] private GroundAttackDefinition deufaltMeleeAttack;
    [SerializeField] private GroundAttackDefinition deufaltRangedAttack;

    public IReadOnlyList<GroundAttackDefinition> Attacks => attacks;

    public GroundAttackDefinition GetAttackById(string attackId)
    {
        if (string.IsNullOrWhiteSpace(attackId))
            return null;

        foreach (GroundAttackDefinition attack in attacks)
        {
            if (attack != null && attack.AttackId == attackId)
                return attack;
        }

        return null;
    }

    public GroundAttackDefinition GetDefualtAttack(GroundAttackType attackType)
    {
        return attackType == GroundAttackType.Melee ? deufaltMeleeAttack : deufaltRangedAttack;


    }

    public GroundAttackDefinition GetSafeAttack(string attackId, GroundAttackType expectedType)
    {
        GroundAttackDefinition attack = GetAttackById(attackId);

        if (IsValidAttack(attack, expectedType))
            return attack;

        GroundAttackDefinition fallback = GetDefualtAttack(expectedType);

        if (IsValidAttack(fallback, expectedType))
            return fallback;


        return null;


    }

    public bool IsValidAttack(GroundAttackDefinition attack, GroundAttackType expectedType)
    {
        if (attack == null || attack.AttackType != expectedType || attack.HitBoxPrefab == null)
            return false;

        HitBox hitBox = attack.HitBoxPrefab.GetComponent<HitBox>();

        if (hitBox == null)
            return false;

        bool prefabIsMelee = hitBox.GetIsMelee();

        return expectedType == GroundAttackType.Melee ? prefabIsMelee : !prefabIsMelee;
    }
}
