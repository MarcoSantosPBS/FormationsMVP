using UnityEngine;
using System.Collections.Generic;

public class SquadMovingBehaviour : SquadBehaviour
{
    protected override void Update()
    {
        if (_isActive)
        {
            MoveForward();
            KeepFormation();
        }
    }

}
