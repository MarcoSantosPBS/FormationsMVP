using System.Collections.Generic;
using UnityEngine;

public class AIBattleCommander : MonoBehaviour
{
    private EnemyAction[] actions;

    private void Awake()
    {
        actions = GetComponents<EnemyAction>();
    }

    private void Update()
    {
        foreach (EnemyAction action in actions)
        {
            action.TakeAction();
        }
    }
}
