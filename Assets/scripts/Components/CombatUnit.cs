using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    [SerializeField] public int damage;
    [SerializeField] private float attackInterval = 2.5f;
    [SerializeField] public Unit targetUnit;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private Unit unit;
    [SerializeField] public bool isEngaged;

    private float lastAttackTime = -1;

    private void Update()
    {
        lastAttackTime += Time.deltaTime;

        if (targetUnit != null)
        {
            if (!IsInRangeOfAttack() && !unit.Squad.KeepFormationInCombat)
            {
                Vector3 direction = targetUnit.transform.position - transform.position;
                unit.Mover.MoveToPosition(transform.position + direction);
                transform.forward = direction;
                return;
            }
            else
            {
                unit.Mover.Stop();
                isEngaged = true;
            }

            bool isEnemyDead = Attack();

            if (isEnemyDead)
            {
                targetUnit = null;   
            }
        }
    }

    private bool IsInRangeOfAttack()
    {
        return Vector3.Distance(transform.position, targetUnit.transform.position) < attackRange;
    }

    public bool Attack()
    {
        if (lastAttackTime > attackInterval)
        {
            lastAttackTime = 0f;
            unit.AttackAnimation();
            return targetUnit.TakeDamage(damage, unit);
        }

        return false;
    }

    public void StopAttacking()
    {
        targetUnit = null;
    }

    public void SetTargetUnit(Unit targetUnit)
    {
        this.targetUnit = targetUnit;
    }

    private void OnDrawGizmos()
    {
        if (targetUnit == null) { return; }
        if (GetComponent<Unit>().Squad.type == SquadFriendlyType.Enemy) { return; }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, targetUnit.transform.position);

    }
}
