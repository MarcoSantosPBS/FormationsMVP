using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class SquadController : MonoBehaviour
{
    [SerializeField] GameObject unitPrefab;
    [SerializeField] public SquadFriendlyType type;

    [field: SerializeField] public bool KeepFormationInCombat { get; private set; }
    [field: SerializeField] public float UnitSpacing { get; private set; }
    [field: SerializeField] public int Columns { get; private set; }
    [field: SerializeField] public int Lines { get; private set; }

    public List<Unit> Units { get; private set; }
    public Transform Pivot { get; private set; }

    private bool _isEngaged;
    private SquadCombatBehaviour _combatBehaviour;
    private SquadIdleBehaviour _idleBehaviour;

    private void Awake()
    {
        Units = new List<Unit>();
        Pivot = transform;
        _combatBehaviour = GetComponent<SquadCombatBehaviour>();
        _idleBehaviour = GetComponent<SquadIdleBehaviour>();
    }

    private void Start()
    {
        UnitCollider.Instance.squads.Add(this);
        GenerateUnits();
        _isEngaged = false;
        _idleBehaviour.Activate();
    }

    private void Update()
    {
        var squadsInRange = UnitCollider.Instance.CheckSquadCollision(this, true);

        if (Input.GetMouseButtonDown(1) && type == SquadFriendlyType.Allied)
        {
            Vector3 destination = MouseWorld.Instance.GetMousePosition();
            Vector3 destDirection = (destination - transform.position).normalized;
            transform.position = destination;
            transform.rotation = Quaternion.LookRotation(destDirection);
        }

        if (squadsInRange.Count > 0 && !_isEngaged)
        {
            _idleBehaviour.Deactivate();
            _combatBehaviour.AllignFormationWithEnemy(squadsInRange[0], transform);
            _combatBehaviour.Activate();
            _isEngaged = true;
        }

        if (squadsInRange.Count == 0 && _isEngaged)
        {
            _combatBehaviour.Deactivate();
            _idleBehaviour.Activate();
            _isEngaged = false;
        }
    }

    public Vector3 GetCentroid()
    {
        if (Units == null) return Vector3.zero;

        Vector3 sum = Vector3.zero;

        foreach (Unit unit in Units)
        {
            sum += unit.transform.position;
        }

        return sum / Units.Count;
    }

    public Unit GetFrontlinePivot()
    {
        List<Unit> frontLine = Units
            .Where(u => u.squadPosition.y == 0)
            .OrderBy(u => u.squadPosition.x)
            .ToList();

        if (frontLine.Count > 0)
        {
            int middle = frontLine.Count / 2;
            return frontLine[middle];
        }

        return null;
    }

    public void GenerateUnits()
    {
        Units.Clear();

        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Vector3 offset = CalculateOffset(column, line);
                Vector3 startPosition = Pivot.position + offset;

                var unitGO = Instantiate(unitPrefab, startPosition, Quaternion.identity);
                Unit unit = unitGO.GetComponent<Unit>();
                unit.Squad = this;
                unit.squadPosition = new Vector2Int(column, line);

                if (type == SquadFriendlyType.Enemy)
                {
                    unit.name = $"Inimigo {line * Columns + column}";
                }
                else
                {
                    unit.name = $"({column},{line})";
                }
                Units.Add(unit);
            }
        }
    }

    public Vector3 CalculateOffset(int column, int line)
    {
        Vector3 offset = -line * UnitSpacing * transform.forward +
                        (column - Columns / 2) * UnitSpacing * transform.right;

        return offset;
    }

    public bool RecalculateFormation(Unit deadUnit)
    {
        Unit closestUnit = null;
        float currentDistance = 0;
        float Wx = 2;
        float Wy = 1;

        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                int index = line * Columns + column;
                if (index >= Units.Count) continue;

                Unit unit = Units[line * Columns + column];

                if (unit == deadUnit) { continue; }
                if (!unit.isAlive) { continue; }

                Vector2Int unitPosition = unit.squadPosition;
                Vector2Int deadUnitPosition = deadUnit.squadPosition;
                int penaltyY = (unitPosition.y <= deadUnitPosition.y) ? 5 : 0;
                int penaltyX = (unitPosition.x > deadUnitPosition.x) ? 2 : 0;

                float distance = Wx * Mathf.Abs(unitPosition.x - deadUnitPosition.x) + Wy * Mathf.Abs(unitPosition.y - deadUnitPosition.y);
                float heuristc = distance + penaltyX + penaltyY;

                if (closestUnit == null || heuristc < currentDistance)
                {
                    closestUnit = unit;
                    currentDistance = heuristc;
                }
            }
        }

        if (closestUnit == null)
        {
            return false;
        }

        if (closestUnit.squadPosition.y == deadUnit.squadPosition.y || closestUnit.squadPosition.y < deadUnit.squadPosition.y)
        {
            return true;
        }

        Vector2Int oldPos = closestUnit.squadPosition;

        Units[GetUnitIndex(deadUnit)] = closestUnit;
        Units[GetUnitIndex(closestUnit)] = deadUnit;

        closestUnit.squadPosition = deadUnit.squadPosition;
        deadUnit.squadPosition = oldPos;

        return RecalculateFormation(deadUnit);
    }

    public int GetUnitIndex(Unit unit)
    {
        Vector2Int position = unit.squadPosition;

        return position.y * Columns + position.x;
    }

    public void RemoveUnit(Unit unit)
    {
        RecalculateFormation(unit);
    }

    public Rect GetSquadAABB()
    {
        Unit frontLinePivot = GetFrontlinePivot();
        if (frontLinePivot == null) { return new Rect(); }

        Transform frontLinePivotPosition = frontLinePivot.transform;

        float width = Columns * UnitSpacing;
        float depth = Lines * UnitSpacing;

        Vector3 halfWidth = 0.5f * Columns * UnitSpacing * frontLinePivotPosition.right;
        Vector3 halfDepth = 0.5f * Lines * UnitSpacing * -frontLinePivotPosition.forward;

        Vector3 corner0 = frontLinePivotPosition.position - halfWidth - halfDepth * 2;
        Vector3 corner1 = frontLinePivotPosition.position + halfWidth - halfDepth * 2;
        Vector3 corner2 = frontLinePivotPosition.position - halfWidth * 2f + halfDepth * 3f;
        Vector3 corner3 = frontLinePivotPosition.position + halfWidth * 2f + halfDepth * 3f;

        Vector2[] points = new Vector2[4];
        points[0] = new Vector2(corner0.x, corner0.z);
        points[1] = new Vector2(corner1.x, corner1.z);
        points[2] = new Vector2(corner2.x, corner2.z);
        points[3] = new Vector2(corner3.x, corner3.z);

        float minX = points.Min(p => p.x);
        float maxX = points.Max(p => p.x);
        float minY = points.Min(p => p.y);
        float maxY = points.Max(p => p.y);

        Rect quadRect = new Rect(minX, minY, maxX - minX, maxY - minY);

        return quadRect;
    }

#if UNITY_EDITOR
    void OnApplicationQuit()
    {
        foreach (var unit in Units)
        {
            if (unit != null)
            {
                DestroyImmediate(unit.gameObject);
            }
        }

        DestroyImmediate(gameObject);
    }
#endif

    #region backup

    //columns = Mathf.CeilToInt(Mathf.Sqrt(numberOfUnits));
    //lines = Mathf.CeilToInt((float)numberOfUnits / columns);

    //public void RotateToEnemySquad(GameObject enemySquad)
    //{
    //    Vector3 direction = enemySquad.transform.position - transform.position;
    //    Quaternion rotation = Quaternion.LookRotation(direction);
    //    transform.rotation = rotation;
    //}

    //bool hasCollided = UnitCollider.Instance.CheckCollision(Units, HandleCollision, true);

    //if (goToCombateState)
    //{
    //    if (squadsInRange.Count > 0 && !isEngaged) 
    //    {
    //        idleBehaviour.Deactivate();
    //        combatBehaviour.AllignFormationWithEnemy(squadsInRange[0], transform);
    //        combatBehaviour.Activate();
    //    }
    //}

    //if (leaveCombatState)
    //{
    //    combatBehaviour.Deactivate();
    //    idleBehaviour.Activate();
    //    leaveCombatState = false;
    //}

    //public void Desengage()
    //{
    //    if (!_isEngaged) return;

    //    foreach (Unit unit in Units)
    //    {
    //        unit.SetTargetUnit(null);
    //    }

    //    _isEngaged = false;
    //    _combatBehaviour.Deactivate();
    //    _idleBehaviour.Activate();
    //    Debug.Log("Desengajou");
    //}

    #endregion
}
