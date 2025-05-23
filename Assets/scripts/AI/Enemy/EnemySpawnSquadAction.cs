using System;
using UnityEngine;

public class EnemySpawnSquadAction : EnemyAction
{
    public override int CalculateActionScore()
    {
        return 100;
    }

    public override void TakeAction()
    {
        foreach (LineSpawner line in _lineSpawners)
        {
            if (line.GetPlayerSquadsInLine().Count > 0 && line.GetEnemySquadsInLine().Count < 1)
            {
                SquadScriptableObject bestUnit = GetBestUnitToSpawn(line);
                _gameManager.InstantiateSquad(line.GetEnemySpawner(), bestUnit, _gameManager.GetEnemyFaction());
            }
        }
    }

    private SquadScriptableObject GetBestUnitToSpawn(LineSpawner line)
    {
        return _availableSquads[0];
    }
}
