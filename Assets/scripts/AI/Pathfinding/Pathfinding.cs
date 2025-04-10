using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    public Node[,] grid;
    public int gridSizeX;
    public int gridSizeY;

    List<Node> openList;
    List<Node> closedList;

    public Pathfinding(int width, int height)
    {
        gridSizeX = width;
        gridSizeY = height;

        grid = new Node[width, height];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                grid[x, y] = new Node(new Vector2Int(x, y), true);
            }
        }
    }

    public List<Vector3> GetPathVector3(Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();

        Vector2Int startVector2 = GridManager.Instance.WorldToGrid(start);
        Vector2Int endVector2 = GridManager.Instance.WorldToGrid(end);

        var pathVector2 = FindPath(startVector2, endVector2);

        foreach (var position in pathVector2)
        {
            path.Add(GridManager.Instance.GridToWorld(position));
        }

        return path;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Node startNode = grid[start.x, start.y];
        Node endNode = grid[end.x, end.y];

        openList = new List<Node>() { startNode };
        closedList = new List<Node>();

        while (openList.Count > 0)
        {
            Node currentNode = GetLowestFCost(openList);

            if (currentNode == endNode)
            {
                return RetracePath(startNode, currentNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            List<Node> neighbors = GetNeighbors(currentNode);

            foreach (Node neighbor in neighbors)
            {
                if (closedList.Contains(neighbor) || !neighbor.isWalkable) 
                    continue;

                float newGCost = currentNode.gCost + Vector2.Distance(currentNode.position, neighbor.position);

                if (newGCost < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = Vector2Int.Distance(neighbor.position, end);
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return new List<Vector2Int>();
    }

    private Node GetLowestFCost(List<Node> nodes)
    {
        Node lowestNode = nodes[0];

        foreach (Node node in nodes)
        {
            if (node.fCost < lowestNode.fCost)
            {
                lowestNode = node;
            }
        }

        return lowestNode;
    }

    private List<Node> GetNeighbors(Node currentNode)
    {
        List<Node> neighbors = new List<Node>();
        int x = currentNode.position.x;
        int y = currentNode.position.y;

        if (x - 1 >= 0) neighbors.Add(grid[x - 1, y]);
        if (x + 1 < gridSizeX) neighbors.Add(grid[x + 1, y]);
        if (y - 1 >= 0) neighbors.Add(grid[x, y - 1]);
        if (y + 1 < gridSizeY) neighbors.Add(grid[x, y + 1]);

        return neighbors;
    }

    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}
