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

    private bool IsBlockedByCloserEnemy(Unit source, Unit target)
    {
        Vector3 sourcePos = source.transform.position;
        Vector3 targetPos = target.transform.position;

        Vector3 dirToEnemy = (target.transform.position - source.transform.position).normalized;
        float side = Vector3.Dot(source.transform.right, dirToEnemy);

        Vector3 directionToTarget = (targetPos - sourcePos).normalized;
        float distanceToTarget = CalculateDistance(source.squadPosition.x, targetPos);

        foreach (var other in mainOponent.Units)
        {
            if (!other.isAlive || other == target) continue;

            Vector3 otherPos = other.transform.position;
            Vector3 directionToOther = (otherPos - sourcePos).normalized;
            float distanceToOther = CalculateDistance(source.squadPosition.x, otherPos);

            if (distanceToOther > distanceToTarget) continue;

            float lateralOffset = Vector3.Cross(directionToOther, directionToTarget).y;

            bool isBlockingPath = (side < 0 && lateralOffset < 0) || (side > 0 && lateralOffset > 0);

            if (isBlockingPath)
            {
                return true;
            }
        }

        return false;
    }

    private float GetRelativeX(Unit enemy)
    {
        Vector3 pivot = controller.GetCentroid();
        Vector3 dirToEnemy = enemy.transform.position - pivot;
        Vector3 forward = (mainOponent.GetCentroid() - controller.GetCentroid()).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        return Vector3.Dot(dirToEnemy, right);
    }

    private float GetRelativeZ(Unit enemy)
    {
        Vector3 pivot = controller.GetCentroid();
        Vector3 dirToEnemy = enemy.transform.position - pivot;
        Vector3 forward = (mainOponent.GetCentroid() - controller.GetCentroid()).normalized;

        return Vector3.Dot(dirToEnemy, forward);
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

    private void OnDrawGizmos()
    {
        if (mainOponent == null) { return; }

        Unit unidade1 = controller.Units[columns * 0 + 0];
        Unit unidade2 = controller.Units[columns * 0 + 6];

        Unit inimigo1 = mainOponent.Units[mainOponent.Columns * 1 + 1];
        Unit inimigo2 = mainOponent.Units[mainOponent.Columns * 1 + 2];
        Unit inimigo3 = mainOponent.Units[mainOponent.Columns * 1 + 0];

        var dir1ToTarget = (inimigo1.transform.position - unidade1.transform.position).normalized;
        var dir1ToOther = (inimigo2.transform.position - unidade1.transform.position).normalized;

        var dir2ToTarget = (inimigo1.transform.position - unidade2.transform.position).normalized;
        var dir2ToOther = (inimigo3.transform.position - unidade2.transform.position).normalized;

        Vector3 sla = Vector3.Cross(dir1ToOther, dir1ToTarget);
        Vector3 sla2 = Vector3.Cross(dir2ToOther, dir2ToTarget);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(unidade1.transform.position, unidade1.transform.position + sla * 20f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(unidade1.transform.position, unidade1.transform.position + dir1ToTarget * 3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(unidade1.transform.position, unidade1.transform.position + dir1ToOther * 3f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(unidade2.transform.position, unidade2.transform.position + sla2 * 20f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(unidade2.transform.position, unidade2.transform.position + dir2ToTarget * 3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(unidade2.transform.position, unidade2.transform.position + dir2ToOther * 3f);
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
