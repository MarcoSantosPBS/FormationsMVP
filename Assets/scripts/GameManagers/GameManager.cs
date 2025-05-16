using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LayerMask LineMask;
    [SerializeField] private LineSpawner DebugSpawner;
    [SerializeField] private SquadScriptableObject AllySO;
    [SerializeField] private SquadScriptableObject EnemySO;
    [SerializeField] private SquadController SquadPrefab;


    private void Start()
    {
        InstantiateSquad(DebugSpawner.GetAllySpawner(), SquadFriendlyType.Allied, AllySO);
        InstantiateSquad(DebugSpawner.GetEnemySpawner(), SquadFriendlyType.Enemy, EnemySO);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LineSpawner line = MouseWorld.Instance.GetTInMousePosition<LineSpawner>(LineMask);

            if (line == null) 
                return;

            InstantiateSquad(line.GetAllySpawner(), SquadFriendlyType.Allied, AllySO);
        }
    }

    private void InstantiateSquad(Transform spawnTransform, SquadFriendlyType type, SquadScriptableObject squadSO)
    {
        Quaternion rotation = Quaternion.LookRotation(spawnTransform.forward);

        Instantiate(SquadPrefab, spawnTransform.position, rotation).InitSquad(squadSO, type);
    }

}
