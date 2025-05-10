using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SquadBehaviour : MonoBehaviour
{
    [SerializeField] protected SquadController controller;

    protected int columns;
    protected int lines;
    protected float unitSpacing;
    protected List<Unit> units;
    protected Transform controllerTransform;
    protected bool isActive;

    protected virtual void Start()
    {
        columns = controller.Columns;
        lines = controller.Lines;
        units = controller.Units;
        unitSpacing = controller.UnitSpacing;
        controllerTransform = controller.transform;
    }

    protected virtual void Update()
    {
        if (isActive)
        {
            UpdateUnitsPositions();
        }
    }

    public virtual void Activate()
    {
        isActive = true;
    }

    public virtual void Deactivate()
    {
        isActive = false;
    }

    public virtual void UpdateUnitsPositions()
    {
        for (int line = 0; line < lines; line++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = line * columns + column;
                if (index >= units.Count) return;
                if (!units[index].isActiveAndEnabled) { continue; }
                if (units[index].combatUnit.targetUnit != null) { continue; }

                Vector3 offset = CalculateOffset(column, line);
                Vector3 destination = controllerTransform.position + offset;

                Unit unit = units[index];
                unit.Mover.MoveToPosition(destination);
                unit.transform.rotation = Quaternion.LookRotation(controllerTransform.forward);
            }
        }
    }

    protected Vector3 CalculateOffset(int column, int line) => controller.CalculateOffset(column, line);
}
