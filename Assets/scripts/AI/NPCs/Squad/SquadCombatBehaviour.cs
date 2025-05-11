using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
    }

    public override void Activate()
    {
        currentTargetPairs = new Dictionary<Unit, Unit>();
        isActive = true;
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    private Unit GetUnitInFrontOf(Unit unit)
    {
        int line = unit.squadPosition.y;
        int column = unit.squadPosition.x;
        return units.FirstOrDefault(u => u.squadPosition.x == column && u.squadPosition.y == line - 1);
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
