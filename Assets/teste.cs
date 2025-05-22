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
            squad.DebugKill();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            squad.CentralizeFormation();
        }
    }

}
