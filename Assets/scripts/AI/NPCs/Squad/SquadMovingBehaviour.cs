using UnityEngine;
using System.Collections.Generic;

public class SquadMovingBehaviour : SquadBehaviour
{
    private bool _shouldKeepMoving;

    private void Awake()
    {
        _shouldKeepMoving = true;
    }

    protected override void Update()
    {
        IsBehindAlliedSquad();

        if (_isActive && _shouldKeepMoving)
        {
            MoveForward();
            KeepFormation();
        }
    }

    private void IsBehindAlliedSquad()
    {
        var alliesInRange = UnitCollider.Instance.CheckSquadCollision(controller, false, true);

        if (alliesInRange.Count > 0) 
        {
            if (HasAnyAllyInFront(alliesInRange))
            {
                _shouldKeepMoving = false;
                StopFormation();
                return;
            }
            else
            {
                _shouldKeepMoving = true;
            }
        }
        else
        {
            _shouldKeepMoving = true;
        }

        
    }

    private bool HasAnyAllyInFront(List<SquadController> neighbors)
    {
        foreach (SquadController neighbor in neighbors)
        {
            Vector3 dirToNeighbor = (neighbor.transform.position - controller.transform.position).normalized;
            float dotProduct = Vector3.Dot(controller.transform.forward, dirToNeighbor);

            if (dotProduct > 0)
            {
                return true;
            }
        }

        return false;
    }

}
