﻿using UnityEngine;

public interface IUnitMover
{
    public void MoveToPosition(Vector3 destination);
    public bool IsMoving();
    public void Stop();
}

