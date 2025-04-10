using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;

    public static GridManager Instance;
    public Pathfinding pathfinder;

    private void Awake()
    {
        Instance = this;
        pathfinder = new Pathfinding(width, height);
    }

    public Vector2Int WorldToGrid(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int y = Mathf.FloorToInt(position.z / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int position)
    {
        float x = position.x * cellSize + cellSize / 2;
        float z = position.y * cellSize + cellSize / 2;

        return new Vector3(x, 0, z);
    }
}
