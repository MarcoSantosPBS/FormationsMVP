using System;
using UnityEditor.Animations;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] public float radius;
    [SerializeField] public float mass;
    [SerializeField] public float neighborhoodRange;
    [SerializeField] private Health health;
    [SerializeField] public CombatUnit combatUnit;
    [SerializeField] private Animator animator;

    public bool IsAlive { get; private set; }
    public IUnitMover Mover { get; set; }
    public SquadController Squad { get; set; }

    public Vector2Int squadPosition;

    private void Awake()
    {
        Mover = GetComponent<IUnitMover>();
        IsAlive = true;
    }

    private void Start()
    {
        health.OnDeath += health_OnDeath;
        UnitCollider.Instance.units.Add(this);
    }

    private void Update()
    {
        if (Squad == null) return;
        if (animator == null) { return; }

        //passar isso aqui para a classe mover
        animator.SetBool("isWalking", Mover.IsMoving());
    }

    private void health_OnDeath()
    {
        UnitCollider.Instance.units.Remove(this);
        IsAlive = false;
        gameObject.SetActive(false);
        Squad.ReplaceDeadUnit(this);
    }

    public void ApplyPush(Vector3 displacement)
    {
        transform.position += displacement;
    }

    public void AttackAnimation()
    {
        if (animator == null) { return; }
        animator.SetTrigger("attack");
    }

    public Rect GetNeighborhoodRange()
    {
        Vector3 unitPos = transform.position;
        return new Rect(unitPos.x - neighborhoodRange, unitPos.z - neighborhoodRange, neighborhoodRange * 2f, neighborhoodRange * 2f);
    }

    public bool TakeDamage(int damage, Unit attacker)
    {
        if (GetTargetUnit() == null && attacker != null)
        {
            SetTargetUnit(attacker);
        }

        return health.TakeDamage(damage);
    }

    public Unit GetTargetUnit() => combatUnit.targetUnit;
    public void SetTargetUnit(Unit targetUnit) => combatUnit.SetTargetUnit(targetUnit);
}
