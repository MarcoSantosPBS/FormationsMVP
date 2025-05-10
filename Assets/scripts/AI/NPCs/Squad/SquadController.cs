using System;
using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    [SerializeField] GameObject unitPrefab;
    [SerializeField] public SquadFriendlyType type;
    [field: SerializeField] public float UnitSpacing { get; private set; }
    [field: SerializeField] public int Columns { get; private set; }
    [field: SerializeField] public int Lines { get; private set; }
    public List<Unit> Units { get; private set; }
    public Transform Pivot { get; private set; }
    public bool goToCombateState;
    public bool leaveCombatState;

    private bool isEngaged;
    private SquadCombatBehaviour combatBehaviour;
    private SquadIdleBehaviour idleBehaviour;

    private void Awake()
    {
        Units = new List<Unit>();
        Pivot = transform;
        combatBehaviour = GetComponent<SquadCombatBehaviour>();
        idleBehaviour = GetComponent<SquadIdleBehaviour>();
    }

    private void Start()
    {
        UnitCollider.Instance.squads.Add(this);
        GenerateUnits();
        isEngaged = false;
        goToCombateState = false;
        idleBehaviour.Activate();
    }

    private void Update()
    {
        var squadsInRange = UnitCollider.Instance.CheckSquadCollision(this, true);
        bool hasCollided = UnitCollider.Instance.CheckCollision(Units, HandleCollision, true);

        if (goToCombateState)
        {
            if (squadsInRange.Count > 0 && !isEngaged) 
            {
                idleBehaviour.Deactivate();
                combatBehaviour.AllignFormationWithEnemy(squadsInRange[0], transform);
                combatBehaviour.Activate();
            }
        }

        if (leaveCombatState)
        {
            combatBehaviour.Deactivate();
            idleBehaviour.Activate();
            leaveCombatState = false;
        }
    

        //if (!hasCollided) Desengage();
    }

    private void HandleCollision(Unit enemyUnit, Unit alliedUnit)
    {
        if (isEngaged) { return; }

        idleBehaviour.Deactivate();
        combatBehaviour.Activate();
        combatBehaviour.AllignFormationWithEnemy(enemyUnit.Squad, alliedUnit.transform);

        isEngaged = true;
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

    public void Desengage()
    {
        if (!isEngaged) return;

        foreach (Unit unit in Units)
        {
            unit.SetTargetUnit(null);
        }

        isEngaged = false;
        combatBehaviour.Deactivate();
        idleBehaviour.Activate();
        Debug.Log("Desengajou");
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

    #region backup

    //columns = Mathf.CeilToInt(Mathf.Sqrt(numberOfUnits));
    //lines = Mathf.CeilToInt((float)numberOfUnits / columns);

    //public void RotateToEnemySquad(GameObject enemySquad)
    //{
    //    Vector3 direction = enemySquad.transform.position - transform.position;
    //    Quaternion rotation = Quaternion.LookRotation(direction);
    //    transform.rotation = rotation;
    //}

    #endregion
}
