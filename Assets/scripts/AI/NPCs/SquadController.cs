using System;
using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    [SerializeField] private int numberOfUnits;
    [SerializeField] private float unitSpacing;
    [SerializeField] GameObject unitPrefab;

    public List<Unit> units = new List<Unit>();

    private int columns;
    private int lines;
    private Squad squad;

    private void Awake()
    {
        squad = GetComponent<Squad>();
        RecalculateFormation();

        columns = Mathf.CeilToInt(Mathf.Sqrt(numberOfUnits));
        lines = Mathf.CeilToInt((float)numberOfUnits / columns);
    }

    public void RecalculateFormation()
    {
        int totalUnits = units.Count;

        if (totalUnits == 0) return;

        columns = Mathf.CeilToInt(Mathf.Sqrt(totalUnits));
        lines = Mathf.CeilToInt((float)totalUnits / columns);
    }

    public void RotateToEnemySquad(GameObject enemySquad)
    {
        Vector3 direction = enemySquad.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }

    public void GenerateUnits()
    {
        units.Clear();

        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector3 offset = CalculateOffset(column, line);
                Vector3 startPosition = transform.position + offset;

                var unitGO = Instantiate(unitPrefab, startPosition, Quaternion.identity);
                Unit unit = unitGO.GetComponent<Unit>();
                unit.Squad = squad;
                if (squad.type == SquadFriendlyType.Enemy)
                {
                    unit.name = $"Inimigo {line * columns + column}";
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

                Vector3 offset = CalculateOffset(column, line);
                Vector3 destination = transform.position + offset;

                Unit unit = units[index];
                unit.Mover.MoveToPosition(destination);
            }
        }
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
        RecalculateFormation();
        UpdateUnitsPositions();
    }

    private Vector3 CalculateOffset(int column, int line)
    {
        var xOffset = (column - (columns - 1) /2f) * unitSpacing;
        var zOffset = (line - (lines - 1) / 2f) * unitSpacing;
        Vector3 offset = new Vector3(xOffset, 0, zOffset);

        return transform.rotation * offset;
    }

    public Unit GetRandomUnit()
    {
        return units[new System.Random().Next(units.Count - 1)];
    }
}
