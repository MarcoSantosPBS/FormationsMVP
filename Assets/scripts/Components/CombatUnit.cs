using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    [SerializeField] public int damage;
    [SerializeField] private float attackInterval = 2.5f;
    [SerializeField] public Unit targetUnit;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private Unit unit;
    [SerializeField] public bool isEngaged;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private float detectionRadius;

    private float lastAttackTime = -1;

    private void Update()
    {
        lastAttackTime += Time.deltaTime;
        GetEnemyInRange();

        if (targetUnit != null)
        {
            bool isEnemyDead = Attack();

            if (isEnemyDead)
            {
                targetUnit = null;   
            }
        }
    }

    private void GetEnemyInRange()
    {
        bool hasHit = Physics.SphereCast(transform.position, detectionRadius, unit.Squad.transform.forward, out RaycastHit hit, attackRange, unitLayer);

        if (hasHit)
        {
            if (hit.collider.TryGetComponent(out Unit targetUnit))
            {
                if (targetUnit.Squad.Type == unit.Squad.Type)
                {
                    SetTargetUnit(null);
                    return;
                }

                SetTargetUnit(targetUnit);
                return;
            }
        }

        SetTargetUnit(null);
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

    public void SetTargetUnit(Unit targetUnit)
    {
        if (targetUnit != null)
        {
            unit.Squad.OnEngaggingEnemy();
        }

        this.targetUnit = targetUnit;
    }

    void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Vector3 direction = unit.Squad.transform.forward;
        float radius = detectionRadius;
        float maxDistance = attackRange;

        // Desenhar a esfera de origem
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, radius);

        // Desenhar a direção do cast
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + direction * maxDistance);

        // Desenhar a esfera final (no fim do cast)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin + direction * maxDistance, radius);

        if (targetUnit == null) { return; }
        if (GetComponent<Unit>().Squad.Type == SquadFriendlyType.Enemy) { return; }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, targetUnit.transform.position);
    }
}
