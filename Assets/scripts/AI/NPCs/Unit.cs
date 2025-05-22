using System;
using UnityEditor.Animations;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] public float Radius;
    [SerializeField] public float Mass;
    [SerializeField] public CombatUnit combatUnit;

    [field: SerializeField] public Animator Animator { get; private set; }

    public bool IsAlive { get; set; }
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
        UnitCollider.Instance.units.Add(this);
    }

    private void Update()
    {
        if (Squad == null) return;
        if (Animator == null) { return; }

        //passar isso aqui para a classe mover
        Animator.SetBool("isWalking", Mover.IsMoving());
    }

    public void ApplyPush(Vector3 displacement)
    {
        transform.position += displacement;
    }

    internal void DebugKillUnit()
    {
        combatUnit.DebugKillUnit();
    }
}
