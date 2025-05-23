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

    private void Awake()
    {
        _playerSquads = new List<SquadController>();
        _enemySquads = new List<SquadController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SquadController squad))
        {
            Factions faction = squad.Faction;

            if (faction == GameManager.Instance.GetPlayerFaction())
                _playerSquads.Add(squad);
            else
                _enemySquads.Add(squad);

            squad.OnDeath += SquadController_OnDeath;
        }
    }

    private void SquadController_OnDeath(SquadController squadController, Factions faction)
    {
        if (faction == GameManager.Instance.GetPlayerFaction())
        {
            Debug.Log("Squad do player morreu");
            if (_playerSquads.Contains(squadController))
                _playerSquads.Remove(squadController);
        }
        else
        {
            Debug.Log("Squad do inimigo morreu");
            if (_enemySquads.Contains(squadController))
                _enemySquads.Remove(squadController);
        }
    }

    
}
