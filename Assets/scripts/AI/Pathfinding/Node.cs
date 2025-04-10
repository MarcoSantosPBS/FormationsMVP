using UnityEngine;

public class Node
{
    public float terrainCost = 1f;
    public float gCost;
    public float hCost;
    public Node parent;
    public bool isWalkable;
    public Vector2Int position;

    public float fCost => gCost + hCost;

    public Node(Vector2Int position, bool isWalkable)
    {
        this.position = position;
        this.isWalkable = isWalkable;
        gCost = 0;
        hCost = 0;
        parent = null;
    }
}
