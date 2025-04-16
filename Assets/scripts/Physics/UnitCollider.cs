using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitCollider : MonoBehaviour
{
    public static UnitCollider Instance;
    private Quadtree<Unit> quadtree;
    private Rect mapArea = new Rect(-50, -50, 150, 150);
    public List<Unit> units = new List<Unit>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
        quadtree = new Quadtree<Unit>(4, mapArea);
    }

    private void FixedUpdate()
    {
        quadtree = new Quadtree<Unit>(4, mapArea);

        foreach (var unit in units)
        {
            Rect unitAABB = GetUnitAABB(unit);
            quadtree.Insert(unitAABB, unit);
        }
    }

    public bool CheckCollision(List<Unit> units, Action OnCollision, bool ignoreAlliedCollision)
    {
        bool hasAnyUnitCollided = false;

        foreach (Unit unit in units)
        {
            Rect searchArea = GetUnitAABB(unit);

            var neighbors = quadtree.Search(searchArea);

            foreach (Unit neighbor in neighbors)
            {
                if (neighbor == unit) continue;
                if (ignoreAlliedCollision && neighbor.Squad == unit.Squad) continue;

                Vector3 delta = neighbor.transform.position - unit.transform.position;
                float dist = delta.magnitude;
                float radiusSum = unit.radius + neighbor.radius;

                if (dist < radiusSum & dist > 0.001f)
                {
                    hasAnyUnitCollided = true;
                    OnCollision();
                }
            }
        }

        return hasAnyUnitCollided;
    }

    public void GetTarget(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            Rect searchArea = GetUnitAABB(unit);
            List<Unit> neighbors = quadtree.Search(searchArea);
            Unit closestEnemy = null;
            float distanceToClosest = 0f;

            foreach (Unit neighbor in neighbors)
            {
                if (neighbor == unit) continue;
                if (neighbor.Squad == unit.Squad) continue;

                if (closestEnemy == null) 
                {
                    closestEnemy = neighbor;
                    distanceToClosest = Vector3.Distance(unit.transform.position, neighbor.transform.position);
                    continue;
                }

                float newDistance = Vector3.Distance(unit.transform.position, neighbor.transform.position);

                if (newDistance < distanceToClosest)
                {
                    closestEnemy = neighbor;
                    distanceToClosest = newDistance;
                }
            }

            unit.SetTargetUnit(closestEnemy);
        }
    }

    private void ApplyPush(Unit unit, Unit neighbor, Vector3 delta, float dist, float radiusSum)
    {
        Vector3 mtv = delta.normalized * (radiusSum - dist);

        float m1 = unit.mass;
        float m2 = neighbor.mass;
        float somaMassas = m1 + m2;

        Vector3 pushUnit = -mtv * (m2 / somaMassas);
        Vector3 pushNeighbor = mtv * (m1 / somaMassas);

        unit.ApplyPush(pushUnit);
        neighbor.ApplyPush(pushNeighbor);
    }

    private Rect GetUnitAABB(Unit unit)
    {
        Vector3 unitPos = unit.transform.position;
        return new Rect(unitPos.x - unit.radius, unitPos.z - unit.radius, unit.radius * 2f, unit.radius * 2f);
    }

    private void OnDrawGizmos()
    {
        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        Gizmos.color = Color.yellow;

        foreach (var unit in units)
        {
            Rect aabb = GetUnitAABB(unit);
            DrawRectGizmo(aabb);
        }
    }

    private void DrawRectGizmo(Rect rect)
    {
        Vector3 bottomLeft = new Vector3(rect.xMin, 0f, rect.yMin);
        Vector3 bottomRight = new Vector3(rect.xMax, 0f, rect.yMin);
        Vector3 topRight = new Vector3(rect.xMax, 0f, rect.yMax);
        Vector3 topLeft = new Vector3(rect.xMin, 0f, rect.yMax);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}
