using System;
using UnityEngine;

public class teste : MonoBehaviour
{
    public SquadController squad;


    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Unit unit = squad.DebugKill();
            unit.TakeDamage(10000, null);
        }
    }

}
