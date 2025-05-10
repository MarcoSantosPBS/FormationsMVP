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

    public bool isAlive;
    public Vector2Int squadPosition;

    public IUnitMover Mover { get; set; }
    public SquadController Squad { get; set; }

    private void Awake()
    {
        Mover = GetComponent<IUnitMover>();
        isAlive = true;
    }

    private void Start()
    {
        health.OnDeath += health_OnDeath;
        UnitCollider.Instance.units.Add(this);
    }

    private void Update()
    {
        if (Squad == null) return;

        AttackNearUnit();
        //transform.rotation = Quaternion.Lerp(transform.rotation, Squad.transform.rotation, Time.deltaTime * 5f);

        if (animator == null) { return; }

        //passar isso aqui para a classe mover
        animator.SetBool("isWalking", Mover.IsMoving());
    }

    private void health_OnDeath()
    {
        UnitCollider.Instance.units.Remove(this);
        isAlive = false;
        gameObject.SetActive(false);
        Squad.RemoveUnit(this);
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

    public void AttackNearUnit()
    {
        if (combatUnit.targetUnit != null) { return; }

        var nearbUnits = UnitCollider.Instance.GetUnitsCloseTo(this, true, false);

        if (nearbUnits.Count == 0) { return; }

        SetTargetUnit(nearbUnits[0]);
    }

    public bool TakeDamage(int damage, Unit attacker)
    {
        if (GetTargetUnit() == null)
        {
            SetTargetUnit(attacker);
        }

        return health.TakeDamage(damage);
    }

    public Unit GetTargetUnit() => combatUnit.targetUnit;
    public void SetTargetUnit(Unit targetUnit) => combatUnit.SetTargetUnit(targetUnit);

}
