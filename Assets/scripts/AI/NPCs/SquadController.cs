using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SquadController : MonoBehaviour
{
    [SerializeField] private int numberOfUnits;
    [SerializeField] private float unitSpacing;
    [SerializeField] GameObject unitPrefab;
    [SerializeField] public int columns;
    [SerializeField] public int lines;

    public List<Unit> units = new List<Unit>();

    private Vector3 attackDirection = Vector3.zero;
    private Vector3 ortogonalVector = Vector3.zero;
    private Transform pivot;

    private Squad squad;

    private void Awake()
    {
        squad = GetComponent<Squad>();
        pivot = transform;
    }

    private void IterateUnits(Action<int, int> method)
    {
        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                
            }
        }
    }

    public void GenerateUnits()
    {
        units.Clear();

        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector3 offset = CalculateOffset(column, line);
                Vector3 startPosition = pivot.position + offset;

                var unitGO = Instantiate(unitPrefab, startPosition, Quaternion.identity);
                Unit unit = unitGO.GetComponent<Unit>();
                unit.Squad = squad;
                unit.squadPosition = new Vector2Int(column, line);

                if (squad.type == SquadFriendlyType.Enemy)
                {
                    unit.name = $"Inimigo {line * columns + column}";
                }
                else
                {
                    unit.name = $"({column},{line})";
                }
                units.Add(unit);
            }
        }
    }

    public void UpdateUnitsPositions()
    {
        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = line * columns + column;
                if (index >= units.Count) return;
                if (!units[index].isActiveAndEnabled) { continue; }

                Vector3 offset = CalculateOffset(column, line);
                Vector3 destination = pivot.position + offset;

                Unit unit = units[index];
                unit.Mover.MoveToPosition(destination);
            }
        }
    }

    public void UpdatePositionToCombat(Squad attackingSquad, Vector3 pivo)
    {
        attackDirection = (pivot.forward - attackingSquad.transform.forward).normalized;
        ortogonalVector = Vector3.Cross(Vector3.up, attackDirection);
        Vector3 enemySquadDirection = (attackingSquad.transform.position - pivot.position).normalized;

        lines = attackingSquad.GetLines();
        columns = attackingSquad.GetCollumns();

        pivot.forward = enemySquadDirection;
        pivot.position = pivo;

        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = line * columns + column;
                if (index >= units.Count) return;
                if (!units[index].isActiveAndEnabled) { continue; }
                Unit unit = units[index];
                unit.squadPosition = new Vector2Int(column, line);

                Vector3 newPosition = pivot.position - line * unitSpacing * attackDirection + (column - columns / 2) * unitSpacing * ortogonalVector;
                unit.Mover.MoveToPosition(newPosition);
                unit.transform.rotation = Quaternion.LookRotation(attackDirection);
            }
        }
    }

    public bool RecalculateFormation(Unit deadUnit)
    {
        Unit closestUnit = null;
        float currentDistance = 0;
        float Wx = 2;
        float Wy = 1;

        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = line * columns + column;
                if (index >= units.Count) continue;

                Unit unit = units[line * columns + column];

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

        units[GetUnitIndex(deadUnit)] = closestUnit;
        units[GetUnitIndex(closestUnit)] = deadUnit;

        closestUnit.squadPosition = deadUnit.squadPosition;
        deadUnit.squadPosition = oldPos;

        return RecalculateFormation(deadUnit);
    }

    public int GetUnitIndex(Unit unit)
    {
        Vector2Int position = unit.squadPosition;

        return position.y * columns + position.x;
    }

    public void RemoveUnit(Unit unit)
    {
        RecalculateFormation(unit);
    }

    private Vector3 CalculateOffset(int column, int line)
    {
        Vector3 offset = -line * unitSpacing * transform.forward + (column - columns / 2) * unitSpacing * transform.right;

        return offset;
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
