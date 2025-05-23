using UnityEngine;

public class AIBattleCommander : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private SquadScriptableObject[] _availableSquads;
    private LineSpawner[] _lineSpawners;

    private void Start()
    {
        _availableSquads = GameManager.Instance.GetAvailableSquads(Factions.Greek);
        _lineSpawners = GameManager.Instance.GetLineSpawners();
    }
}
