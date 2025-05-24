using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SquadCombatBehaviour : SquadBehaviour
{
    private SquadController _enemySquad;

    protected override void Update()
    {
        if (!_isActive) return;

        base.Update();
        KeepFormation();

        if (controller.IsRanged)
        {
            GetRangedTargets();
        }

    }

    public override void Activate()
    {
        base.Activate();
        StopFormation();
        controller.SetCentroidToFormationCenter();
    }

    private void GetRangedTargets()
    {
        if (_enemySquad == null)
        {
            controller.Desengage();
        }

        foreach (Unit unit in controller.Units)
        {
            if (unit.combatUnit.HasTargetUnit()) { continue; }
            if (_enemySquad == null) { continue; }

            int targetindex = Random.Range(0, _enemySquad.AliveUnits.Count);
            Unit target = _enemySquad.AliveUnits[targetindex];

            unit.combatUnit.SetTargetUnit(target.combatUnit);
        }
    }

    public void SetEnemySquad(SquadController enemySquad)
    {
        _enemySquad = enemySquad;
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }


}
