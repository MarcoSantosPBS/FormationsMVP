using UnityEngine;

public class LineSpawner : MonoBehaviour
{
    [SerializeField] public Transform allySpawner;
    [SerializeField] public Transform enemySpawner;

    public Transform GetAllySpawner() => allySpawner.transform;
    public Transform GetEnemySpawner() => enemySpawner.transform;
}
