using UnityEngine;

public class CombatCalculator : MonoBehaviour
{
    [SerializeField] private int _baseHitChance = 35;
    [SerializeField] private int _minimunHitChance = 10;
    [SerializeField] private int _maxHitChance = 100;

    public bool CalculateAttack(CombatUnitSO attacker, CombatUnitSO defender)
    {
        int hitChance = _baseHitChance + attacker.MeleeAttack - defender.MeleeDefense;
        hitChance = Mathf.Clamp(hitChance, _minimunHitChance, _maxHitChance);

        int hitRool =  Random.Range(0, 100);

        return hitRool < hitChance;
    }
}
