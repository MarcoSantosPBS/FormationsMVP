using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class SquadCombatBehaviour : SquadBehaviour
{
    private SquadController mainOponent;
    private Dictionary<Unit, Unit> currentTargetPairs;

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
        currentTargetPairs = new Dictionary<Unit, Unit>();
        isActive = true;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        SetPivotToFrontline();
    }

    public void Fight()
    {
        if (mainOponent == null) { return; }
        SetPivotToFrontline();

        foreach (Unit unit in units)
        {
            if (unit.GetTargetUnit() != null) { continue; }
            if (unit.squadPosition.y != 0) { continue; }

            Unit closestEnemy = null;
            float closestEnemyDistance = -1;

            foreach (Unit enemyUnit in mainOponent.Units)
            {
                if (!enemyUnit.isAlive || !enemyUnit.isActiveAndEnabled) { continue; }
                if (IsBlockedByCloserEnemy(unit, enemyUnit)) { continue; }

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
                if (currentTargetPairs.TryGetValue(closestEnemy, out Unit allied))
                {
                    float oldDistance = CalculateDistance(allied, closestEnemy);
                    if (closestEnemyDistance < oldDistance)
                    {
                        allied.SetTargetUnit(null);
                        currentTargetPairs[closestEnemy] = unit;
                        FindNewTarget(allied, new List<Unit>());
                    }
                    else
                    {
                        FindNewTarget(unit, new List<Unit>() { closestEnemy });
                        continue;
                    }
                }

                if (!currentTargetPairs.ContainsKey(closestEnemy))
                {
                    currentTargetPairs.Add(closestEnemy, unit);
                }

                unit.SetTargetUnit(closestEnemy);
            }
        }
    }

    private void SetPivotToFrontline()
    {
        Unit pivot = controller.GetFrontlinePivot();

        if (pivot != null)
        {
            transform.position = pivot.transform.position;
        }
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
            if (IsBlockedByCloserEnemy(unit, enemyUnit)) { continue; }

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
            if (currentTargetPairs.TryGetValue(closestEnemy, out Unit allied))
            {
                if (!allied.isAlive)
                {
                    currentTargetPairs[closestEnemy] = unit;
                    unit.SetTargetUnit(closestEnemy);
                    return;
                }

                if (areAllEnemiesTargeted)
                {
                    unit.SetTargetUnit(closestEnemy);
                    return;
                }

                float oldDistance = CalculateDistance(allied, closestEnemy);
                if (closestEnemyDistance < oldDistance)
                {
                    allied.SetTargetUnit(null);
                    currentTargetPairs[closestEnemy] = unit;
                    FindNewTarget(allied, new List<Unit>() { closestEnemy });
                }
                else
                {
                    closedList.Add(closestEnemy);
                    FindNewTarget(unit, closedList);
                    return;
                }
            }

            if (!currentTargetPairs.ContainsKey(closestEnemy))
            {
                currentTargetPairs.Add(closestEnemy, unit);
            }

            unit.SetTargetUnit(closestEnemy);
        }

    }

    public override void UpdateUnitsPositions()
    {
        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = line * columns + column;
                if (index >= units.Count) return;
                if (!units[index].isActiveAndEnabled) { continue; }

                Unit unit = units[index];
                if (unit.combatUnit.targetUnit != null) { continue; }

                Unit inFront = GetUnitInFrontOf(unit);

                if (inFront == null) continue;
                Vector3 behindOffset = -inFront.transform.forward * controller.UnitSpacing;
                Vector3 desiredPosition = inFront.transform.position + behindOffset;

                unit.Mover.MoveToPosition(desiredPosition);
                unit.transform.rotation = Quaternion.LookRotation(controllerTransform.forward);
            }
        }
    }

    private Unit GetUnitInFrontOf(Unit unit)
    {
        int line = unit.squadPosition.y;
        int column = unit.squadPosition.x;
        return units.FirstOrDefault(u => u.squadPosition.x == column && u.squadPosition.y == line - 1);
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

    private float CalculateDistance(int column, Vector3 enemyUnit)
    {
        Vector3 pivot = controller.GetCentroid();
        Vector3 dirToEnemy = enemyUnit - pivot;
        Vector3 forward = (mainOponent.GetCentroid() - controller.GetCentroid()).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        float localX = Vector3.Dot(dirToEnemy, right);
        float localZ = Vector3.Dot(dirToEnemy, forward);

        float expectedColumn = (column - (columns - 1) / 2.0f) * unitSpacing;
        float lateralError = Mathf.Abs(localX - expectedColumn);
        float cost = lateralError + Mathf.Abs(localZ);

        return cost;
    }

    public bool AreAllEnemiesBeingTargeted(List<Unit> closedList)
    {
        List<Unit> aliveEnemyUnits = mainOponent.Units.Where(x => x.isAlive).ToList();

        return !aliveEnemyUnits.Except(closedList).Any() && aliveEnemyUnits.Count == closedList.Count;
    }

    private bool IsBlockedByCloserEnemy(Unit source, Unit target)
    {
        bool isBlockedLaterally = false;
        bool isBlockedFrontally = false;
        Vector3 sourceCentroid = source.Squad.GetCentroid();

        Vector3 targetPos = target.transform.position;
        Vector3 dirToEnemy = (target.transform.position - source.transform.position).normalized;
        Vector3 dirCentroidToEnemy = (target.transform.position - sourceCentroid).normalized;
        Vector3 localRight = Vector3.Cross(Vector3.up, dirCentroidToEnemy);

        float distanceToTarget = CalculateDistance(source.squadPosition.x, targetPos);
        float sourceToAlvoRight = Vector3.Dot(dirToEnemy, source.transform.right);

        foreach (var other in mainOponent.Units)
        {
            if (!other.isAlive || other == target) continue;

            Vector3 otherPos = other.transform.position;
            Vector3 dirTargetToNeighbor = (otherPos - targetPos).normalized;

            float alvoToOtherRight = Vector3.Dot(-dirTargetToNeighbor, localRight);
            float distanceToOther = CalculateDistance(source.squadPosition.x, otherPos);
            float alvoToOTherForward = Vector3.Dot(dirTargetToNeighbor, -dirCentroidToEnemy);

            if (distanceToOther > distanceToTarget) continue;

            float lateralOffset = Vector3.Cross(dirTargetToNeighbor, -dirCentroidToEnemy.normalized).magnitude * dirTargetToNeighbor.magnitude;
            float frontalOffset = Vector3.Cross(dirTargetToNeighbor, localRight).magnitude * dirTargetToNeighbor.magnitude;

            float targetToOther = Vector3.Dot(dirTargetToNeighbor, localRight);
            bool hasSomeoneOnCommingSide = sourceToAlvoRight * alvoToOtherRight >= 0f;

            if (hasSomeoneOnCommingSide)
            {
                if ((Mathf.Abs(alvoToOtherRight) - frontalOffset) > 0.2f)
                {
                    isBlockedLaterally = true;
                }
            }

            if ((alvoToOTherForward - lateralOffset) > 0.50)
            {
                isBlockedFrontally = true;
            }

            bool isBlocking = isBlockedLaterally && isBlockedFrontally;

            if (isBlocking)
            {
                return true;
            }
        }

        return false;
    }

    public void AllignFormationWithEnemy(SquadController attackingSquad, Transform pivo)
    {
        mainOponent = attackingSquad;

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

    //private float GetRelativeX(Unit enemy)
    //{
    //    Vector3 pivot = controller.GetCentroid();
    //    Vector3 dirToEnemy = enemy.transform.position - pivot;
    //    Vector3 forward = (mainOponent.GetCentroid() - controller.GetCentroid()).normalized;
    //    Vector3 right = Vector3.Cross(Vector3.up, forward);

    //    return Vector3.Dot(dirToEnemy, right);
    //}

    //private float GetRelativeZ(Unit enemy)
    //{
    //    Vector3 pivot = controller.GetCentroid();
    //    Vector3 dirToEnemy = enemy.transform.position - pivot;
    //    Vector3 forward = (mainOponent.GetCentroid() - controller.GetCentroid()).normalized;

    //    return Vector3.Dot(dirToEnemy, forward);
    //}
    #endregion
}
