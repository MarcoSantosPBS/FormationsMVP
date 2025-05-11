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

}
