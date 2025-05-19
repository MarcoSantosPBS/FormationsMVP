using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    [field: SerializeField] public float UnitSpacing { get; private set; }
    [field: SerializeField] public int Columns { get; private set; }
    [field: SerializeField] public int Lines { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }

    public List<Unit> Units { get; private set; }
    public Unit[,] UnitsGrid { get; private set; }
    public bool _isEngaged { get; set; }

    [SerializeField] GameObject UnitPrefab;
    [SerializeField] public SquadFriendlyType Type;
    [SerializeField] public Factions Faction;

    private SquadCombatBehaviour _combatBehaviour;
    private SquadMovingBehaviour _idleBehaviour;

    private void Awake()
    {
        Units = new List<Unit>();
        UnitsGrid = new Unit[Columns, Lines];
        _combatBehaviour = GetComponent<SquadCombatBehaviour>();
        _idleBehaviour = GetComponent<SquadMovingBehaviour>();
    }

    private void Start()
    {
        UnitCollider.Instance.squads.Add(this);
        _isEngaged = false;
        _idleBehaviour.Activate();
    }

    public void InitSquad(SquadScriptableObject squadSO, SquadFriendlyType type, Factions faction)
    {
        UnitPrefab = squadSO.unitPrefab;
        Type = type;
        Faction = faction;
        GenerateUnits();
    }

    private void Update()
    {
        var squadsInRange = UnitCollider.Instance.CheckSquadCollision(this, true);

        if (squadsInRange.Count == 0) Desengage();
    }

    public void OnEngaggingEnemy()
    {
        if (_isEngaged) { return; }

        _idleBehaviour.Deactivate();
        _combatBehaviour.Activate();
        _isEngaged = true;
    }

    private void Desengage()
    {
        if (!_isEngaged) return;

        _idleBehaviour.Activate();
        _combatBehaviour.Deactivate();
        _isEngaged = false;
    }

    private void OnDestroy()
    {
        foreach (var unit in Units)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }

        UnitCollider.Instance.squads.Remove(this);
    }

    private bool IsSquadDefeated()
    {
        foreach (Unit unit in Units)
        {
            if (unit.IsAlive) { return false; }
        }

        return true;
    }

    private void GenerateUnits()
    {
        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Vector3 startPosition = GridPositionToWorld(column, line);
                Vector2Int positionInGrid = new Vector2Int(column, line);

                var unitGO = Instantiate(UnitPrefab, startPosition, Quaternion.LookRotation(transform.forward));

                Unit unit = unitGO.GetComponent<Unit>();
                unit.Squad = this;
                unit.squadPosition = positionInGrid;
                Units.Add(unit);
                if (Type == SquadFriendlyType.Enemy)
                {
                    unit.name = $"(Inimigo: {column}, {line})";
                }
                else
                    unit.name = $"({column}, {line})";

                UnitsGrid[column, line] = unit;
            }
        }
    }

    public Vector3 GridPositionToWorld(int column, int line)
    {
        float x = (column - Columns / 2) * UnitSpacing;
        float z = (line - Lines / 2) * UnitSpacing;
        return transform.position + transform.rotation * new Vector3(x, 0, z);
    }

    public bool ReplaceDeadUnit(Unit deadUnit)
    {
        if (IsSquadDefeated()) 
        {
            Destroy(gameObject);
            return false;
        }


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

                Unit unit = UnitsGrid[column, line];

                if (unit == deadUnit) { continue; }
                if (!unit.IsAlive) { continue; }

                Vector2Int unitPosition = unit.squadPosition;
                Vector2Int deadUnitPosition = deadUnit.squadPosition;

                int penaltyY = (unitPosition.y >= deadUnitPosition.y) ? 5 : 0;
                int penaltyX = (unitPosition.x > deadUnitPosition.x) ? 0 : 0;

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

        if (closestUnit.squadPosition.y == deadUnit.squadPosition.y || closestUnit.squadPosition.y > deadUnit.squadPosition.y)
        {
            return true;
        }

        Vector2Int oldPos = deadUnit.squadPosition;

        UnitsGrid[deadUnit.squadPosition.x, deadUnit.squadPosition.y] = closestUnit;
        UnitsGrid[closestUnit.squadPosition.x, closestUnit.squadPosition.y] = deadUnit;

        deadUnit.squadPosition = closestUnit.squadPosition;
        closestUnit.squadPosition = oldPos;

        closestUnit.Mover.MoveToPosition(GridPositionToWorld(closestUnit.squadPosition.x, closestUnit.squadPosition.y));

        return ReplaceDeadUnit(deadUnit);
    }

    public void SetCentroidToFormationCenter()
    {
        Vector3 centroid = new Vector3();

        foreach (Unit unit in Units)
        {
            centroid += unit.transform.position;
        }

        Vector3 center = centroid / Units.Count;
        Vector3 centroidPos = new Vector3(transform.position.x, transform.position.y, center.z);

        transform.position = centroidPos;
    }

    public Rect GetSquadAABB()
    {
        Transform frontLinePivotPosition = transform;

        float width = Columns * UnitSpacing;
        float depth = Lines * UnitSpacing;

        Vector3 halfWidth = 0.5f * Columns * UnitSpacing * frontLinePivotPosition.right;
        Vector3 halfDepth = 0.5f * Lines * UnitSpacing * -frontLinePivotPosition.forward;

        Vector3 corner0 = frontLinePivotPosition.position - halfWidth - halfDepth * 1.3f;
        Vector3 corner1 = frontLinePivotPosition.position + halfWidth - halfDepth * 1.3f;
        Vector3 corner2 = frontLinePivotPosition.position - halfWidth * 1.3f + halfDepth * 1.3f;
        Vector3 corner3 = frontLinePivotPosition.position + halfWidth * 1.3f + halfDepth * 1.3f;

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

    public Unit DebugKill()
    {
        return UnitsGrid[1, 4];
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
    //public void ReplaceDeadUnit(Unit deadUnit)
    //{
    //    Vector2Int replacementSlot = deadUnit.squadPosition + new Vector2Int(deadUnit.squadPosition.x, deadUnit.squadPosition.y - 1);
    //    Unit replacement = null;

    //    if (_positionsInGrid.ContainsKey(replacementSlot))
    //    {
    //        replacement = UnitsGrid[replacementSlot.x, replacementSlot.y];
    //        Vector2Int oldSlot = deadUnit.squadPosition;

    //        UnitsGrid[deadUnit.squadPosition.x, deadUnit.squadPosition.y] = replacement;
    //        UnitsGrid[replacement.squadPosition.x, replacement.squadPosition.y] = deadUnit;

    //        deadUnit.squadPosition = replacementSlot;
    //        replacement.squadPosition = oldSlot;

    //        if (replacement.isAlive)
    //            replacement.Mover.MoveToPosition(GridPositionToWorld(replacement.squadPosition.x, replacement.squadPosition.y));
    //    }
    //    else
    //        return;

    //    ReplaceDeadUnit(deadUnit);
    //}
    #endregion
}
