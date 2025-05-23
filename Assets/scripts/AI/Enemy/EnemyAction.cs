using UnityEngine;

public abstract class EnemyAction : MonoBehaviour
{
    protected SquadScriptableObject[] _availableSquads;
    protected LineSpawner[] _lineSpawners;
    protected GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _availableSquads = _gameManager.GetAvailableSquads(Factions.Greek);
        _lineSpawners = _gameManager.GetLineSpawners();
    }

    public abstract int CalculateActionScore();
    public abstract void TakeAction();
    
}
