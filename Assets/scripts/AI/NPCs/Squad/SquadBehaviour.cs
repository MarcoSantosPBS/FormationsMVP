using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SquadBehaviour : MonoBehaviour
{
    [SerializeField] protected SquadController controller;

    protected int _columns;
    protected int _lines;
    protected float _unitSpacing;
    protected List<Unit> _units;
    protected Transform _controllerTransform;
    protected bool _isActive;

    protected virtual void Start()
    {
        _columns = controller.Columns;
        _lines = controller.Lines;
        _units = controller.Units;
        _unitSpacing = controller.UnitSpacing;
        _controllerTransform = controller.transform;
    }

    protected virtual void Update() { }

    public virtual void Activate()
    {
        _isActive = true;
    }

    public virtual void Deactivate()
    {
        _isActive = false;
    }

    protected void KeepFormation()
    {
        for (int line = 0; line < _lines; line++)
        {
            for (int column = 0; column < _columns; column++)
            {
                Unit unit = controller.UnitsGrid[column, line];

                //if (!controller.UnitsGrid[column, line].IsAlive) { continue; }
                //if (!controller.UnitsGrid[column, line].isActiveAndEnabled) { continue; }

                Vector3 destination = controller.GridPositionToWorld(column, line);
                float dist = Vector3.Distance(unit.transform.position, destination);
                if (dist < 0.1f) { continue; }
                unit.Mover.MoveToPosition(destination);
            }
        }
    }

    protected virtual void MoveForward()
    {
        controller.transform.position += Time.deltaTime * controller.Speed * controller.transform.forward;
    }

    protected void StopFormation()
    {
        foreach (Unit unit in _units)
        {
            if (unit.IsAlive)
                unit.Mover.Stop();
        }
    }

}
