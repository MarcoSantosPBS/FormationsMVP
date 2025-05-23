using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LayerMask LineMask;
    [SerializeField] private LineSpawner DebugSpawner;
    [SerializeField] private SquadScriptableObject AllySO;
    [SerializeField] private SquadScriptableObject EnemySO;
    [SerializeField] private SquadController SquadPrefab;
    [SerializeField] private Factions PlayerFaction;
    [SerializeField] private Factions EnemyFaction;

    private FactionManager[] _factionManagers;
    private BaseCollider[] _baseColliders;
    private LineSpawner[] _lineSpawners;
    private SquadScriptableObject _playerSelectedSquadSO;
    
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
        _baseColliders = FindObjectsByType<BaseCollider>(FindObjectsSortMode.None);
        _factionManagers = FindObjectsByType<FactionManager>(FindObjectsSortMode.None);
        _lineSpawners = FindObjectsByType<LineSpawner>(FindObjectsSortMode.None);
    }


    private void Start()
    {
        SubscribeToEvents();
        //InstantiateSquad(DebugSpawner.GetAllySpawner(), AllySO, PlayerFaction);
        //InstantiateSquad(DebugSpawner.GetEnemySpawner(), EnemySO, EnemyFaction);
    }

    private void Update()
    {
        if (MouseWorld.Instance.IsMouseOverUI()) { return; }

        if (Input.GetMouseButtonDown(0))
        {
            LineSpawner line = MouseWorld.Instance.GetTInMousePosition<LineSpawner>(LineMask);

            if (line == null || _playerSelectedSquadSO == null)
                return;

            InstantiateSquad(line, line.GetAllySpawner(), _playerSelectedSquadSO, PlayerFaction);
        }
    }

    public void InstantiateSquad(LineSpawner lineSpawner, Transform spawnTransform, SquadScriptableObject squadSO, Factions faction)
    {
        Quaternion rotation = Quaternion.LookRotation(spawnTransform.forward);

        SquadController squad = Instantiate(SquadPrefab, spawnTransform.position, rotation);
        squad.InitSquad(squadSO, faction);
        lineSpawner.AddSquadToList(squad, faction);
    }

    public SquadScriptableObject[] GetAvailableSquads(Factions faction)
    {
        FactionManager manager =  _factionManagers.FirstOrDefault(x => x.Faction == faction);
        return manager.GetAvailableSquads();
    }

    private void SubscribeToEvents()
    {
        foreach (BaseCollider baseCollider in _baseColliders)
        {
            baseCollider.OnCollisionDetected += BaseCollider_OnCollisionDetected;
        }
    }

    private void BaseCollider_OnCollisionDetected(Factions faction)
    {
        FactionManager factionManager = _factionManagers.FirstOrDefault(x => x.Faction == faction);
        factionManager.TakeBaseDamage(2);
    }

    public void SetPlayerSelectedSquad(SquadScriptableObject _squadSO) => _playerSelectedSquadSO = _squadSO;
    public LineSpawner[] GetLineSpawners() => _lineSpawners;
    public Factions GetPlayerFaction() => PlayerFaction;
    public Factions GetEnemyFaction() => EnemyFaction;

}
