using UnityEngine;

public class LineSpawner : MonoBehaviour
{
    [SerializeField] public Transform allySpawner;
    [SerializeField] public Transform enemySpawner;

    public Vector3 GetAllySpawner() => allySpawner.position;
    public Vector3 GetEnemySpawner() => enemySpawner.position;
}
