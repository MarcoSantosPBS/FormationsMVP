using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    [field: SerializeField] public CombatUnitSO CombatUnitSO { get; private set; }
    [field: SerializeField] public Health Health { get; private set; }

    [SerializeField] private Unit unit;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private float detectionRadius;
    [SerializeField] private CombatCalculator CombatCalculator;
    [SerializeField] private GameObject unitModel;

    private CombatUnit _targetUnit;
    private float _lastAttackTime = -1;

    private void Awake()
    {
        Health = GetComponent<Health>();
    }

    private void Update()
    {
        _lastAttackTime += Time.deltaTime;

        if (!unit.Squad.IsRanged)
            GetEnemyInRange();
        else
            GetEnemyInRangedRange();

        if (_targetUnit != null)
        {
            bool isEnemyDead = TryToAttack();

            if (isEnemyDead)
            {
                _targetUnit = null;
            }
        }
    }

    private void GetEnemyInRangedRange()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, detectionRadius, unit.Squad.transform.forward, CombatUnitSO.AttackRange, unitLayer);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent(out CombatUnit targetUnit))
            {
                if (targetUnit.GetUnitFaction() != GetUnitFaction())
                {
                    unit.Squad.OnEngaggingEnemyInRangedAttack(targetUnit.GetUnitSquad());
                }
            }
        }
    }

    private void GetEnemyInRange()
    {
        bool hasHit = Physics.SphereCast(transform.position, detectionRadius, unit.Squad.transform.forward, out RaycastHit hit, CombatUnitSO.AttackRange, unitLayer);

        if (hasHit)
        {
            if (hit.collider.TryGetComponent(out CombatUnit targetUnit))
            {
                if (targetUnit.GetUnitFaction() == GetUnitFaction())
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

    public bool TryToAttack()
    {
        float noiseAttackinterval = CombatUnitSO.AttackInterval + Random.Range(0.1f, 1f);

        if (_lastAttackTime > noiseAttackinterval)
        {
            _lastAttackTime = 0f;
            if (CalculateAttack())
            {
                AttackAnimation();
                return _targetUnit.TakeDamage(CombatUnitSO.Damage, this);
            }
        }

        return false;
    }

    public void DebugKillUnit()
    {
        Health.TakeDamage(10000000);
    }

    public bool CalculateAttack()
    {
        return CombatCalculator.CalculateAttack(CombatUnitSO, _targetUnit.CombatUnitSO);
    }

    public void AttackAnimation()
    {
        if (unit.Animator == null) { return; }
        unit.Animator.SetTrigger("attack");
    }

    public bool TakeDamage(int damage, CombatUnit attacker)
    {
        if (_targetUnit == null && attacker != null)
        {
            SetTargetUnit(attacker);
        }

        return Health.TakeDamage(damage);
    }

    public void SetTargetUnit(CombatUnit targetUnit)
    {
        if (targetUnit != null)
        {
            unit.Squad.OnEngaggingEnemy();
            Vector3 dirToEnemy = (targetUnit.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dirToEnemy);
        }

        _targetUnit = targetUnit;
    }

    public void DeactivateModel()
    {
        unitModel.SetActive(false);
        GetComponent<Collider>().enabled = false;
    }

    public Factions GetUnitFaction() => unit.Squad.Faction;
    public SquadController GetUnitSquad() => unit.Squad;
    public bool HasTargetUnit() => _targetUnit != null;

    #region gizmos

    void OnDrawGizmos()
    {
        if (unit.squadPosition.y != unit.Squad.Lines - 1) { return; }

        Vector3 origin = transform.position;
        Vector3 direction = unit.Squad.transform.forward;
        float radius = detectionRadius;
        float maxDistance = CombatUnitSO.AttackRange;

        // Desenhar a esfera de origem
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, radius);

        // Desenhar a direção do cast
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + direction * maxDistance);

        // Desenhar a esfera final (no fim do cast)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin + direction * maxDistance, radius);

        if (_targetUnit == null) { return; }
        if (GetComponent<Unit>().Squad.Faction == Factions.Rome) { return; }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, _targetUnit.transform.position);
    }
    #endregion
}
