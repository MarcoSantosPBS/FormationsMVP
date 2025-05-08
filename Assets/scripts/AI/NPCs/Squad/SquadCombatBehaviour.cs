using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SquadCombatBehaviour : SquadBehaviour
{
    private SquadController mainOponent;
    private Dictionary<Unit, Unit> currentTargetCounts;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        Fight();
    }

    public override void Activate()
    {
        Dictionary<Unit, Unit> currentTargetCounts = new Dictionary<Unit, Unit>();
        isActive = false;
    }

    public void Fight()
    {
        if (mainOponent == null) { return; }

        currentTargetCounts = new Dictionary<Unit, Unit>();

        foreach (Unit unit in units)
        {
            if (unit.squadPosition.y != 0) { continue; }

            Unit closestEnemy = null;
            float closestEnemyDistance = -1;

            foreach (Unit enemyUnit in mainOponent.Units)
            {
                bool teste = IsBlockedByAnotherEnemy(unit, enemyUnit);
                if (!enemyUnit.isAlive || !enemyUnit.isActiveAndEnabled) { continue; }
                //if (IsBlockedByAnotherEnemy(unit, enemyUnit)) { continue; }

                var dist = CalculateDistance(unit, enemyUnit);

                if (closestEnemyDistance == -1f && closestEnemy == null)
                {
                    closestEnemy = enemyUnit;
                    closestEnemyDistance = dist;
                    continue;
                }

                if (dist < closestEnemyDistance)
                {
                    closestEnemy = enemyUnit;
                    closestEnemyDistance = dist;
                }
            }

            if (closestEnemy != null)
            {
                if (currentTargetCounts.TryGetValue(closestEnemy, out Unit allied))
                {
                    float oldDistance = CalculateDistance(allied, closestEnemy);
                    if (closestEnemyDistance < oldDistance)
                    {
                        allied.SetTargetUnit(null);
                        currentTargetCounts[closestEnemy] = unit;
                        FindNewTarget(allied, new List<Unit>());
                    }
                    else
                    {
                        FindNewTarget(unit, new List<Unit>() { closestEnemy });
                        continue;
                    }
                }

                if (!currentTargetCounts.ContainsKey(closestEnemy))
                {
                    currentTargetCounts.Add(closestEnemy, unit);
                }

                unit.SetTargetUnit(closestEnemy);
            }
        }
    }

    private float CalculateDistance(Unit alliedUnit, Unit enemyUnit)
    {
        Vector3 pivot = controller.GetCentroid();
        Vector3 dirToEnemy = enemyUnit.transform.position - pivot;
        Vector3 forward = (mainOponent.GetCentroid() - controller.GetCentroid()).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        float localX = Vector3.Dot(dirToEnemy, right);
        float localZ = Vector3.Dot(dirToEnemy, forward);

        float expectedColumn = (alliedUnit.squadPosition.x - (columns - 1) / 2.0f) * unitSpacing;
        float lateralError = Mathf.Abs(localX - expectedColumn);
        float cost = lateralError + Mathf.Abs(localZ);

        return cost;
    }

    public bool AreAllEnemiesBeingTargeted(List<Unit> closedList)
    {
        List<Unit> aliveEnemyUnits = mainOponent.Units.Where(x => x.isAlive).ToList();

        return !aliveEnemyUnits.Except(closedList).Any() && aliveEnemyUnits.Count == closedList.Count;
    }

    private void FindNewTarget(Unit unit, List<Unit> closedList)
    {
        Unit closestEnemy = null;
        float closestEnemyDistance = -1;
        bool areAllEnemiesTargeted = AreAllEnemiesBeingTargeted(closedList);

        foreach (Unit enemyUnit in mainOponent.Units)
        {
            if (!enemyUnit.isAlive || !enemyUnit.isActiveAndEnabled) { continue; }
            if (closedList.Contains(enemyUnit) && !areAllEnemiesTargeted) { continue; }
            //if (IsBlockedByAnotherEnemy(unit, enemyUnit, mainOponent.Units)) { continue; }

            var dist = CalculateDistance(unit, enemyUnit);

            if (closestEnemyDistance == -1f && closestEnemy == null)
            {
                closestEnemy = enemyUnit;
                closestEnemyDistance = dist;
                continue;
            }

            if (dist < closestEnemyDistance)
            {
                closestEnemy = enemyUnit;
                closestEnemyDistance = dist;
            }
        }

        if (closestEnemy != null)
        {
            if (currentTargetCounts.TryGetValue(closestEnemy, out Unit allied))
            {
                if (areAllEnemiesTargeted)
                {
                    unit.SetTargetUnit(closestEnemy);
                    return;
                }

                float oldDistance = CalculateDistance(allied, closestEnemy);
                if (closestEnemyDistance < oldDistance)
                {
                    allied.SetTargetUnit(null);
                    currentTargetCounts[closestEnemy] = unit;
                    FindNewTarget(allied, new List<Unit>() { closestEnemy });
                }
                else
                {
                    closedList.Add(closestEnemy);
                    FindNewTarget(unit, closedList);
                    return;
                }
            }

            if (!currentTargetCounts.ContainsKey(closestEnemy))
            {
                currentTargetCounts.Add(closestEnemy, unit);
            }

            unit.SetTargetUnit(closestEnemy);
        }

    }

    private bool IsBlockedByAnotherEnemy(Unit source, Unit target)
    {
        var neighbors = UnitCollider.Instance.GetUnitsCloseTo(target, false, true);
        Vector3 dirToEnemy = (target.transform.position - source.transform.position).normalized;
        float side = Vector3.Dot(source.transform.right, dirToEnemy);

        // está a direita?
        if (side > 0)
        {
            return HasUnitAtSide(target.transform.right, target, neighbors);
        }
        else
        {
            return HasUnitAtSide(-target.transform.right, target, neighbors);
        }
    }

    private bool HasUnitAtSide(Vector3 side, Unit target, List<Unit> neighbors)
    {
        Vector3 sidePosition = target.transform.position + side * target.Squad.UnitSpacing;

        foreach (Unit neighbor in neighbors)
        {
            if (Vector3.Distance(sidePosition, neighbor.transform.position) < 0.5f) return true;
        }

        return false;
    }

    public void AllignFormationWithEnemy(SquadController attackingSquad, Transform pivo)
    {
        mainOponent = attackingSquad;


        foreach (Unit enemy in mainOponent.Units)
        {
            Vector3 dirToEnemy = (enemy.transform.position - controller.GetCentroid()).normalized;
            Vector3 forward = attackingSquad.GetCentroid() - controller.GetCentroid();
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            float localX = Vector3.Dot(dirToEnemy, right);
            float localZ = Vector3.Dot(dirToEnemy, forward);

            enemy.name = $"({localX}, {localZ})";
        }
        return;
        Vector3 enemySquadDirection = (attackingSquad.GetCentroid() - controller.GetCentroid()).normalized;
        Vector3 ortogonalVector = Vector3.Cross(Vector3.up, enemySquadDirection);

        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = line * columns + column;
                if (index >= units.Count) return;
                if (!units[index].isActiveAndEnabled) { continue; }
                Unit unit = units[index];
                unit.squadPosition = new Vector2Int(column, line);

                Vector3 newPosition = pivo.position - line * unitSpacing * enemySquadDirection
                                        + (column - columns / 2) * unitSpacing * ortogonalVector;

                unit.Mover.MoveToPosition(newPosition);
                unit.transform.rotation = Quaternion.LookRotation(enemySquadDirection);
            }
        }
    }

    #region backup
    //if (!hasAdjustedLxC)
    //{
    //    lines = attackingSquad.Lines;
    //    columns = attackingSquad.Columns;
    //    hasAdjustedLxC = true;
    //}
    #endregion
}
