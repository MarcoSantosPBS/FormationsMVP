using System.Collections.Generic;
using UnityEngine;

public class LineSpawner : MonoBehaviour
{
    [SerializeField] public Transform allySpawner;
    [SerializeField] public Transform enemySpawner;

    private List<SquadController> _playerSquads;
    private List<SquadController> _enemySquads;

    private void Awake()
    {
        _playerSquads = new List<SquadController>();
        _enemySquads = new List<SquadController>();
    }

    public void AddSquadToList(SquadController squadController, Factions faction)
    {
        if (faction == GameManager.Instance.GetPlayerFaction())
            _playerSquads.Add(squadController);
        else
            _enemySquads.Add(squadController);

        squadController.OnDeath += SquadController_OnDeath;
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

    public Transform GetAllySpawner() => allySpawner.transform;
    public Transform GetEnemySpawner() => enemySpawner.transform;
    public List<SquadController> GetPlayerSquadsInLine() => _playerSquads;
    public List<SquadController> GetEnemySquadsInLine() => _enemySquads;
}
