using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SquadCombatBehaviour : SquadBehaviour
{
    protected override void Update()
    {
        base.Update();
        KeepFormation();
    }

    public override void Activate()
    {
        base.Activate();
        foreach (Unit unit in _units)
        {
            unit.Mover.Stop();
        }

        controller.SetCentroidToFormationCenter();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    #region backup
    #endregion
}
