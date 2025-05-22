using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "ScriptableObjects/UnitSO")]
public class CombatUnitSO : ScriptableObject
{
    [SerializeField] public int MeleeAttack;
    [SerializeField] public int MeleeDefense;
    [SerializeField] public int Damage;
    [SerializeField] public float AttackInterval;
    [SerializeField] public float AttackRange;
}
