using System;
using UnityEngine;
using static UnityEngine.UI.Image;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    public GameObject debugObject;

    public static GridManager Instance;
    public Pathfinding pathfinder;
    

    private void Awake()
    {
        Instance = this;
        pathfinder = new Pathfinding(width, height);
        //DebugGrid();
    }

    private void OnDestroy()
    {
        Instance = null;   
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

    private void DebugGrid()
    {
        for (int x = 0; x < pathfinder.gridSizeX; x++)
        {
            for (int y = 0; y < pathfinder.gridSizeY; y++)
            {
                var origin = GridToWorld(new Vector2Int(x, y));
                Instantiate(debugObject, origin, Quaternion.identity);
            }
        }
    }
}
