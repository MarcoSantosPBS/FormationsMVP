using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    [field: SerializeField] public CombatUnitSO CombatUnitSO { get; private set; }
    [field: SerializeField] public Health Health { get; private set; }

    [SerializeField] private Unit unit;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private float detectionRadius;
    [SerializeField] private CombatCalculator CombatCalculator;

    private CombatUnit _targetUnit;
    private float _lastAttackTime = -1;
    private SquadController _squadController;

    private void Awake()
    {
        Health = GetComponent<Health>();
    }

    private void Start()
    {
        _squadController = unit.Squad;
    }

    private void Update()
    {
        _lastAttackTime += Time.deltaTime;
        GetEnemyInRange();

        if (_targetUnit != null)
        {
            bool isEnemyDead = TryToAttack();

            if (isEnemyDead)
            {
                _targetUnit = null;   
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
                if (targetUnit.GetSquadType() == GetSquadType())
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
        }

        this._targetUnit = targetUnit;
    }

    public SquadFriendlyType GetSquadType() => unit.Squad.Type;

    #region gizmos

    void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Vector3 direction = unit.Squad.transform.forward;
        float radius = detectionRadius;
        float maxDistance = CombatUnitSO.AttackRange;

        //// Desenhar a esfera de origem
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(origin, radius);

        //// Desenhar a direção do cast
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(origin, origin + direction * maxDistance);

        //// Desenhar a esfera final (no fim do cast)
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(origin + direction * maxDistance, radius);

        if (_targetUnit == null) { return; }
        if (GetComponent<Unit>().Squad.Type == SquadFriendlyType.Allied) { return; }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, _targetUnit.transform.position);
    }
    #endregion
}
