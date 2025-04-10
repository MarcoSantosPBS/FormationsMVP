using System.Collections.Generic;
using UnityEngine;

public class PathDebugger : MonoBehaviour
{
    public Pathfinding pathfinding;
    public Vector2Int startPoint;
    public Vector2Int endPoint;
    public float nodeSize = 1f;

    private List<Vector2Int> path;


    private void Start()
    {
        pathfinding = new Pathfinding(100, 100);
        pathfinding.grid[5, 5].isWalkable = false;
        pathfinding.grid[5, 6].isWalkable = false;
        pathfinding.grid[5, 7].isWalkable = false;
        pathfinding.grid[1, 0].isWalkable = false;
        pathfinding.grid[2, 0].isWalkable = false;
        pathfinding.grid[3, 0].isWalkable = false;
    }

    void OnDrawGizmos()
    {
        if (pathfinding == null || pathfinding.grid == null)
            return;

        int width = pathfinding.gridSizeX;
        int height = pathfinding.gridSizeY;

        Node[,] grid = pathfinding.grid;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = grid[x, y];

                // Escolhe a cor do gizmo
                if (!node.isWalkable)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.white;

                Vector3 pos = new Vector3(node.position.x, 0, node.position.y);
                Gizmos.DrawWireCube(pos, Vector3.one * nodeSize * 0.9f);
            }
        }

        // Desenha o caminho final em verde
        if (path != null)
        {
            Gizmos.color = Color.green;
            foreach (Vector2 point in path)
            {
                Vector3 pos = new Vector3(point.x, 0, point.y);
                Gizmos.DrawCube(pos, Vector3.one * nodeSize * 0.6f);
            }
        }

        // Ponto inicial: azul
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(new Vector3(startPoint.x, 0, startPoint.y), Vector3.one * nodeSize);

        // Ponto final: amarelo
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(new Vector3(endPoint.x, 0, endPoint.y), Vector3.one * nodeSize);
    }

    void Update()
    {
        if (pathfinding != null)
            path = pathfinding.FindPath(startPoint, endPoint);
    }
}
