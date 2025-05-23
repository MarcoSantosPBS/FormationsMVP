using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LayerMask LineMask;
    [SerializeField] private LineSpawner DebugSpawner;
    [SerializeField] private SquadScriptableObject AllySO;
    [SerializeField] private SquadScriptableObject EnemySO;
    [SerializeField] private SquadController SquadPrefab;

    private FactionManager[] _factionManagers;
    private BaseCollider[] _baseColliders;

    private void Start()
    {
        _baseColliders = FindObjectsByType<BaseCollider>(FindObjectsSortMode.None);
        _factionManagers = FindObjectsByType<FactionManager>(FindObjectsSortMode.None);

        SubscribeToEvents();
        InstantiateSquad(DebugSpawner.GetAllySpawner(), AllySO);
        InstantiateSquad(DebugSpawner.GetEnemySpawner(), EnemySO);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LineSpawner line = MouseWorld.Instance.GetTInMousePosition<LineSpawner>(LineMask);

            if (line == null)
                return;

            InstantiateSquad(line.GetAllySpawner(), AllySO);
        }
    }

    private void InstantiateSquad(Transform spawnTransform, SquadScriptableObject squadSO)
    {
        Quaternion rotation = Quaternion.LookRotation(spawnTransform.forward);

        Instantiate(SquadPrefab, spawnTransform.position, rotation).InitSquad(squadSO);
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

}
