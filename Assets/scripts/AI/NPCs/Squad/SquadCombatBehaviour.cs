using System.Collections.Generic;
using Unity.VisualScripting;
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
                if (!enemyUnit.isAlive || !enemyUnit.isActiveAndEnabled) { continue; }

                var dist = Vector3.Distance(unit.transform.position, enemyUnit.transform.position);

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
                    float oldDistance = Vector3.Distance(allied.transform.position, closestEnemy.transform.position);
                    if (closestEnemyDistance < oldDistance)
                    {
                        allied.SetTargetUnit(null);
                        currentTargetCounts[closestEnemy] = unit;
                        FindNewTarget(allied);
                    }
                    else
                    {
                        FindNewTarget(unit);
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

    //private float CalculateDistance()
    //{

    //}

    private void FindNewTarget(Unit unit)
    {
        Unit closestEnemy = null;
        float closestEnemyDistance = -1;

        foreach (Unit enemyUnit in mainOponent.Units)
        {
            if (!enemyUnit.isAlive || !enemyUnit.isActiveAndEnabled) { continue; }
            if (currentTargetCounts.ContainsKey(enemyUnit)) { continue; }

            var dist = Vector3.Distance(unit.transform.position, enemyUnit.transform.position);

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
            if (!currentTargetCounts.ContainsKey(closestEnemy))
            {
                currentTargetCounts.Add(closestEnemy, unit);
            }

            unit.SetTargetUnit(closestEnemy);
        }

    }

    public void AllignFormationWithEnemy(SquadController attackingSquad, Transform pivo)
    {
        mainOponent = attackingSquad;
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
