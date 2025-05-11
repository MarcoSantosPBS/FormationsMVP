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
    public Unit[,] UnitsGrid { get; private set; }

    private bool _isEngaged;
    private SquadCombatBehaviour _combatBehaviour;
    private SquadIdleBehaviour _idleBehaviour;

    private void Awake()
    {
        Units = new List<Unit>();
        UnitsGrid = new Unit[Columns, Lines];
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
            transform.rotation = Quaternion.LookRotation(destDirection);
            transform.position = destination;
            UpdatePosition();
        }

        if (squadsInRange.Count > 0 && !_isEngaged)
        {
            _idleBehaviour.Deactivate();
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

    private void GenerateUnits()
    {
        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Vector3 startPosition = GridPositionToWorld(column, line);

                var unitGO = Instantiate(unitPrefab, startPosition, Quaternion.identity);

                Unit unit = unitGO.GetComponent<Unit>();
                unit.Squad = this;
                unit.squadPosition = new Vector2Int(column, line);
                Units.Add(unit);

                unit.name = $"({column}, {line})";

                UnitsGrid[column, line] = unit;
            }
        }
    }

    protected void UpdatePosition()
    {
        int totalUnits = Units.Count; // sua lista de unidades
        int totalSlots = Lines * Columns;

        if (totalUnits != totalSlots)
        {
            Debug.LogError("Número de unidades não bate com o número de posições na formação.");
            return;
        }

        // Etapa 1: gerar lista de posições do grid
        List<Vector3> formationPositions = new List<Vector3>();
        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                formationPositions.Add(GridPositionToWorld(column, line));
            }
        }

        // Etapa 2: criar matriz de custo [unidade i] x [posição j]
        int N = totalUnits;
        double[,] costMatrix = new double[N, N];
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                costMatrix[i, j] = Vector3.SqrMagnitude(Units[i].transform.position - formationPositions[j]);
            }
        }

        AuctionAlgorithm solver = new AuctionAlgorithm(costMatrix);
        int[] assignment = solver.Solve();

        // Etapa 4: mover unidades para as posições designadas
        for (int i = 0; i < N; i++)
        {
            Vector3 destination = formationPositions[assignment[i]];
            Units[i].Mover.MoveToPosition(destination);
        }
    }

    private void GreedAssigment()
    {
        List<Vector2Int> formationSlots = CreateFormationSlotsList();
        List<Unit> closedList = new List<Unit>();

        foreach (Vector2Int slot in formationSlots)
        {
            Unit closestUnit = null;
            float minDist = float.MaxValue;

            foreach (Unit unit in Units)
            {
                if (closedList.Contains(unit)) { continue; }

                Vector3 slotWorldPosition = GridPositionToWorld(slot.x, slot.y);
                float dist = Vector3.SqrMagnitude(unit.transform.position - slotWorldPosition);

                if (dist < minDist)
                {
                    minDist = dist;
                    closestUnit = unit;
                }
            }

            UnitsGrid[slot.x, slot.y] = closestUnit;
            closedList.Add(closestUnit);
        }
    }

    private List<Vector2Int> CreateFormationSlotsList()
    {
        List<Vector2Int> formationSlots = new List<Vector2Int>();

        for (int line = 0; line < Lines; line++)
        {
            for (int column = 0; column < Columns; column++)
            {
                formationSlots.Add(new Vector2Int(column, line));
            }
        }

        return formationSlots;
    }

    private Vector3 GridPositionToWorld(int column, int line)
    {
        float x = (column - Columns / 2) * UnitSpacing;
        float z = (line - Lines / 2) * UnitSpacing;
        return transform.position + transform.rotation * new Vector3(x, 0, z);
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
