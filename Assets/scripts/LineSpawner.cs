using System.Collections.Generic;
using UnityEngine;

public class LineSpawner : MonoBehaviour
{
    [SerializeField] public Transform allySpawner;
    [SerializeField] public Transform enemySpawner;

    private List<SquadController> _playerSquads;
    private List<SquadController> _enemySquads;

    public Transform GetAllySpawner() => allySpawner.transform;
    public Transform GetEnemySpawner() => enemySpawner.transform;

    private void OnCollisionEnter(Collision collision)
    {
        if (TryGetComponent(out SquadController squad))
        {
            Debug.Log(squad.Faction);
        }
    }
}
