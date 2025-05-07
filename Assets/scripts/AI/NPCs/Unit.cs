using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] public float radius;
    [SerializeField] public float mass;
    [SerializeField] private Health health;
    [SerializeField] public CombatUnit combatUnit;

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
        //transform.rotation = Quaternion.Lerp(transform.rotation, Squad.transform.rotation, Time.deltaTime * 5f);
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

    public bool TakeDamage(int damage) => health.TakeDamage(damage);
    public Unit GetTargetUnit() => combatUnit.targetUnit;
    public void SetTargetUnit(Unit targetUnit) => combatUnit.SetTargetUnit(targetUnit);

}
