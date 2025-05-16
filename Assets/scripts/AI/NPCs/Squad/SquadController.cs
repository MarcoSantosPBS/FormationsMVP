using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class SquadController : MonoBehaviour
{
    [SerializeField] GameObject unitPrefab;
    [SerializeField] public SquadFriendlyType type;

    [field: SerializeField] public bool KeepFormationInCombat { get; private set; }
    [field: SerializeField] public float UnitSpacing { get; private set; }
    [field: SerializeField] public int Columns { get; private set; }
    [field: SerializeField] public int Lines { get; private set; }
    [SerializeField] public SquadController enemySquad;

    public List<Unit> Units { get; private set; }
    public Transform Pivot { get; private set; }
    public Unit[,] UnitsGrid { get; private set; }
    [field: SerializeField] public bool _isEngaged { get; set; }

    private SquadCombatBehaviour _combatBehaviour;
    private SquadIdleBehaviour _idleBehaviour;
    private Dictionary<Vector2Int, Unit> _positionsInGrid;

    private void Awake()
    {
        Units = new List<Unit>();
        UnitsGrid = new Unit[Columns, Lines];
        _positionsInGrid = new Dictionary<Vector2Int, Unit>();
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
        MoveInFormation();

        if (Input.GetMouseButtonDown(1) && type == SquadFriendlyType.Allied)
        {
            Vector3 destination = MouseWorld.Instance.GetMousePosition();
            Vector3 destDirection = (destination - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(destDirection);
            transform.position = destination;
            UpdatePosition();
        }

        if (squadsInRange.Count > 0 && !_isEngaged)
        {
            _idleBehaviour.Deactivate();
            _combatBehaviour.Activate();
            _combatBehaviour.SetMainOponent(squadsInRange[0]);
            _isEngaged = true;
        }

        if (squadsInRange.Count == 0 && _isEngaged)
        {
            _combatBehaviour.Deactivate();
            _idleBehaviour.Activate();
            _isEngaged = false;
        }
    }

    private void GenerateUnits()
    {
        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Vector3 startPosition = GridPositionToWorld(column, line);
                Vector2Int positionInGrid = new Vector2Int(column, line);

                var unitGO = Instantiate(unitPrefab, startPosition, Quaternion.identity);

                Unit unit = unitGO.GetComponent<Unit>();
                unit.Squad = this;
                unit.squadPosition = positionInGrid;
                Units.Add(unit);
                _positionsInGrid.Add(positionInGrid, unit);
                if (type == SquadFriendlyType.Enemy)
                {
                    unit.name = $"(Inimigo: {column}, {line})";
                }
                else
                    unit.name = $"({column}, {line})";

                UnitsGrid[column, line] = unit;
            }
        }
    }

    protected void UpdatePosition()
    {
        int totalUnits = Units.Count;
        int totalSlots = Lines * Columns;
        double[,] costMatrix;

        if (totalUnits != totalSlots)
        {
            Debug.LogError("Número de unidades não bate com o número de posições na formação.");
            return;
        }

        List<Vector3> formationPositions = new List<Vector3>();
        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                formationPositions.Add(GridPositionToWorld(column, line));
            }
        }

        costMatrix = GetCostMatrix(totalUnits, formationPositions);

        AuctionAlgorithm solver = new AuctionAlgorithm(costMatrix);
        int[] assignment = solver.Solve();

        for (int i = 0; i < totalUnits; i++)
        {
            Vector3 destination = formationPositions[assignment[i]];
            if (Units[i].IsAlive)
                Units[i].Mover.MoveToPosition(destination);
        }
    }

    protected void MoveInFormation()
    {
        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Unit unit = UnitsGrid[column, line];

                if (!UnitsGrid[column, line].IsAlive) { continue; }
                if (!UnitsGrid[column, line].isActiveAndEnabled) { continue; }

                Vector3 destination = GridPositionToWorld(column, line);
                unit.Mover.MoveToPosition(destination);
            }
        }
    }

    private double[,] GetCostMatrix(int totalUnits, List<Vector3> formationPositions)
    {
        double[,] costMatrix = new double[totalUnits, totalUnits];
        for (int i = 0; i < totalUnits; i++)
        {
            for (int j = 0; j < totalUnits; j++)
            {
                double cost = Units[i].IsAlive ? Vector3.SqrMagnitude(Units[i].transform.position - formationPositions[j]) : 1e9;
                costMatrix[i, j] = cost;
            }
        }

        return costMatrix;
    }

    private Vector3 GridPositionToWorld(int column, int line)
    {
        float x = (column - Columns / 2) * UnitSpacing;
        float z = (line - Lines / 2) * UnitSpacing;
        return transform.position + transform.rotation * new Vector3(x, 0, z);
    }

    public bool ReplaceDeadUnit(Unit deadUnit)
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

                Unit unit = UnitsGrid[column, line];

                if (unit == deadUnit) { continue; }
                if (!unit.IsAlive) { continue; }

                Vector2Int unitPosition = unit.squadPosition;
                Vector2Int deadUnitPosition = deadUnit.squadPosition;

                int penaltyY = (unitPosition.y >= deadUnitPosition.y) ? 5 : 0;
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

        if (closestUnit.squadPosition.y == deadUnit.squadPosition.y || closestUnit.squadPosition.y > deadUnit.squadPosition.y)
        {
            return true;
        }

        Vector2Int oldPos = deadUnit.squadPosition;

        UnitsGrid[deadUnit.squadPosition.x, deadUnit.squadPosition.y] = closestUnit;
        UnitsGrid[closestUnit.squadPosition.x, closestUnit.squadPosition.y] = deadUnit;

        deadUnit.squadPosition = closestUnit.squadPosition;
        closestUnit.squadPosition = oldPos;

        return ReplaceDeadUnit(deadUnit);
    }

    public Unit DebugKill()
    {
        return UnitsGrid[1, 2];
    }

    public Rect GetSquadAABB()
    {
        Transform frontLinePivotPosition = transform;

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

    //protected void UpdatePosition()
    //{
    //    GreedAssigment();

    //    for (int line = 0; line < Lines; line++)
    //    {
    //        for (int column = 0; column < Columns; column++)
    //        {
    //            Vector3 destination = GridPositionToWorld(column, line);
    //            Unit unit = UnitsGrid[column, line];
    //            unit.Mover.MoveToPosition(destination);
    //        }
    //    }
    //}

    //private void GreedAssigment()
    //{
    //    List<Vector2Int> formationSlots = CreateFormationSlotsList();
    //    List<Unit> closedList = new List<Unit>();

    //    foreach (Vector2Int slot in formationSlots)
    //    {
    //        Unit closestUnit = null;
    //        float minDist = float.MaxValue;

    //        foreach (Unit unit in Units)
    //        {
    //            if (closedList.Contains(unit)) { continue; }

    //            Vector3 slotWorldPosition = GridPositionToWorld(slot.x, slot.y);
    //            float dist = Vector3.SqrMagnitude(unit.transform.position - slotWorldPosition);

    //            if (dist < minDist)
    //            {
    //                minDist = dist;
    //                closestUnit = unit;
    //            }
    //        }

    //        UnitsGrid[slot.x, slot.y] = closestUnit;
    //        closedList.Add(closestUnit);
    //    }
    //}

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
