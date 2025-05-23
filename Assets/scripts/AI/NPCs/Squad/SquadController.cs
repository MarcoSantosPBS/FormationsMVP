using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; private set; }

    [SerializeField] public Factions Faction;

    public float UnitSpacing { get; private set; }
    public int Columns { get; private set; }
    public int Lines { get; private set; }
    public List<Unit> Units { get; private set; }
    public Unit[,] UnitsGrid { get; private set; }
    public bool _isEngaged { get; set; }

    private SquadCombatBehaviour _combatBehaviour;
    private SquadMovingBehaviour _idleBehaviour;
    private GameObject _unitPrefab;
    private bool _mustAllignToCenter;

    private void Awake()
    {
        Units = new List<Unit>();
        _combatBehaviour = GetComponent<SquadCombatBehaviour>();
        _idleBehaviour = GetComponent<SquadMovingBehaviour>();
    }

    private void Start()
    {
        UnitCollider.Instance.squads.Add(this);
        _isEngaged = false;
        _idleBehaviour.Activate();
    }

    private void Update()
    {
        if (_mustAllignToCenter)
        {
            TryToReformToCenter();
        }

        var squadsInRange = UnitCollider.Instance.CheckSquadCollision(this, true);

        if (squadsInRange.Count == 0) Desengage();
    }

    private void OnDestroy()
    {
        foreach (var unit in Units)
        {
            if (unit != null)
            {
                UnitCollider.Instance.units.Remove(unit);
                Destroy(unit.gameObject);
            }
        }

        UnitCollider.Instance.squads.Remove(this);
    }

    public void InitSquad(SquadScriptableObject squadSO, Factions faction)
    {
        _unitPrefab = squadSO.unitPrefab;
        Faction = faction;
        UnitSpacing = squadSO.UnitSpacing;
        Columns = squadSO.Columns;
        Lines = squadSO.Lines;
        UnitsGrid = new Unit[Columns, Lines];

        GenerateUnits();
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

    private void GenerateUnits()
    {
        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Vector3 startPosition = GridPositionToWorld(column, line);
                Vector2Int positionInGrid = new Vector2Int(column, line);

                var unitGO = Instantiate(_unitPrefab, startPosition, Quaternion.LookRotation(transform.forward));

                Unit unit = unitGO.GetComponent<Unit>();
                unit.Squad = this;
                unit.squadPosition = positionInGrid;
                Units.Add(unit);
                unit.combatUnit.Health.OnDeath += health_OnDeath;

                if (Faction == Factions.Greek)
                {
                    unit.name = $"(Inimigo: {column}, {line})";
                }
                else
                    unit.name = $"({column}, {line})";

                UnitsGrid[column, line] = unit;
            }
        }
    }

    public void CentralizeFormation()
    {
        List<Unit> aliveUnits = new List<Unit>();
        List<Vector2Int> sortedSlots = GetSortedSlotPositions();

        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Lines; y++)
            {
                Unit unit = UnitsGrid[x, y];
                if (unit != null && unit.IsAlive)
                {
                    aliveUnits.Add(unit);
                }
            }
        }

        int N = aliveUnits.Count;
        double[,] costMatrix = new double[N, N];

        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Vector3 pos = GridPositionToWorld(sortedSlots[j].x, sortedSlots[j].y);
                Vector3 unitPos = GridPositionToWorld(aliveUnits[i].squadPosition.x, aliveUnits[i].squadPosition.y);

                costMatrix[i, j] = Vector3.SqrMagnitude(unitPos - pos);
            }
        }

        AuctionAlgorithm solver = new AuctionAlgorithm(costMatrix);
        int[] assignment = solver.Solve();


        for (int i = 0; i < N; i++)
        {
            Vector2Int slot = sortedSlots[assignment[i]];
            SwapUnitsPosition(UnitsGrid[slot.x, slot.y], aliveUnits[i]);
        }

        _mustAllignToCenter = false;
    }

    List<Vector2Int> GetSortedSlotPositions()
    {
        List<Vector2Int> slots = new List<Vector2Int>();

        int centerX = Columns / 2;
        int firstLine = Lines - 1;

        for (int x = 0; x < Columns; x++)
        {
            slots.Add(new Vector2Int(x, Lines - 1));
        }

        slots.Sort((a, b) =>
        {
            float distA = Vector2.Distance(a, new Vector2(centerX, firstLine));
            float distB = Vector2.Distance(b, new Vector2(centerX, firstLine));
            return distA.CompareTo(distB);
        });

        return slots;
    }

    public Vector3 GridPositionToWorld(int column, int line)
    {
        float offsetX = (Columns % 2 == 0) ? UnitSpacing / 2f : 0f;

        float x = (column - Columns / 2) * UnitSpacing + offsetX;
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

                int penaltyY = (unitPosition.y >= deadUnitPosition.y) ? 15 : 0;
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

        SwapUnitsPosition(deadUnit, closestUnit);

        return ReplaceDeadUnit(deadUnit);
    }

    private void SwapUnitsPosition(Unit deadUnit, Unit closestUnit)
    {
        Vector2Int oldPos = deadUnit.squadPosition;

        UnitsGrid[deadUnit.squadPosition.x, deadUnit.squadPosition.y] = closestUnit;
        UnitsGrid[closestUnit.squadPosition.x, closestUnit.squadPosition.y] = deadUnit;

        deadUnit.squadPosition = closestUnit.squadPosition;
        closestUnit.squadPosition = oldPos;

        closestUnit.Mover.MoveToPosition(GridPositionToWorld(closestUnit.squadPosition.x, closestUnit.squadPosition.y));
    }

    private bool IsSquadDefeated()
    {
        foreach (Unit unit in Units)
        {
            if (unit.IsAlive) { return false; }
        }

        return true;
    }

    private void health_OnDeath(Unit unit)
    {
        _mustAllignToCenter = true;
        UnitCollider.Instance.units.Remove(unit);
        unit.IsAlive = false;
        unit.gameObject.SetActive(false);
        ReplaceDeadUnit(unit);
    }

    public void TryToReformToCenter()
    {
        if (IsAllBackLinesDefeated())
        {
            foreach (Unit unit in Units)
            {
                if (unit.IsAlive && unit.Mover.IsMoving())
                {
                    return;
                }
            }

            CentralizeFormation();
        }
        else
        {
            _mustAllignToCenter = false;
        }
    }

    private bool IsAllBackLinesDefeated()
    {
        for (int column = 0; column < Columns; column++)
        {
            for (int line = 0; line < Lines; line++)
            {
                if (line == Lines - 1) continue;

                if (UnitsGrid[column, line].IsAlive)
                {
                    return false;
                }
            }
        }

        return true;
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

    public void DebugKill()
    {
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Lines; y++)
            {
                Unit unit = UnitsGrid[x, y];

                if (y != Lines - 1)
                {
                    unit.DebugKillUnit();
                    continue;
                }

                if (x == 1 || x == 3)
                {
                    unit.DebugKillUnit();
                    continue;
                }
            }
        }
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
    #endregion
}
