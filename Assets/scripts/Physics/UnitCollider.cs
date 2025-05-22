using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitCollider : MonoBehaviour
{
    public static UnitCollider Instance;
    private Quadtree<Unit> quadtree;
    private Quadtree<SquadController> quadtreeSquad;
    private Rect mapArea = new Rect(-50, -50, 150, 150);
    public List<Unit> units = new List<Unit>();
    public List<SquadController> squads = new List<SquadController>();

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
        quadtreeSquad = new Quadtree<SquadController>(4, mapArea);

        foreach (var unit in units)
        {
            Rect unitAABB = GetUnitAABB(unit);
            quadtree.Insert(unitAABB, unit);
        }

        foreach (var squad in squads)
        {
            Rect squadAABB = squad.GetSquadAABB();
            quadtreeSquad.Insert(squadAABB, squad);
        }
    }

    public bool CheckCollision(List<Unit> units, Action<Unit, Unit> OnCollision, bool ignoreAlliedCollision)
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
                float radiusSum = unit.Radius + neighbor.Radius;

                if (dist < radiusSum & dist > 0.001f)
                {
                    hasAnyUnitCollided = true;
                    OnCollision(neighbor, unit);
                }
            }
        }

        return hasAnyUnitCollided;
    }

    public List<SquadController> CheckSquadCollision(SquadController squadController, bool ignoreAlliedCollision, bool ignoreEnemyCollision = false)
    {
        Rect searchArea = squadController.GetSquadAABB();
        var neighbors = quadtreeSquad.Search(searchArea);
        List<SquadController> squadsInRange = new List<SquadController>();

        foreach (SquadController neighbor in neighbors)
        {
            if (neighbor == squadController) continue;
            if (ignoreAlliedCollision && neighbor.Type == squadController.Type) continue;
            if (ignoreEnemyCollision && neighbor.Type != squadController.Type) continue;

            squadsInRange.Add(neighbor);
        }

        return squadsInRange;
    }

    private void ApplyPush(Unit unit, Unit neighbor, Vector3 delta, float dist, float radiusSum)
    {
        Vector3 mtv = delta.normalized * (radiusSum - dist);

        float m1 = unit.Mass;
        float m2 = neighbor.Mass;
        float somaMassas = m1 + m2;

        Vector3 pushUnit = -mtv * (m2 / somaMassas);
        Vector3 pushNeighbor = mtv * (m1 / somaMassas);

        unit.ApplyPush(pushUnit);
        neighbor.ApplyPush(pushNeighbor);
    }

    private Rect GetUnitAABB(Unit unit)
    {
        Vector3 unitPos = unit.transform.position;
        return new Rect(unitPos.x - unit.Radius, unitPos.z - unit.Radius, unit.Radius * 2f, unit.Radius * 2f);
    }

    private void OnDrawGizmos()
    {
        //Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        SquadController[] squads = FindObjectsByType<SquadController>(FindObjectsSortMode.None);

        Gizmos.color = Color.yellow;

        //foreach (var unit in units)
        //{
        //    Rect aabb = GetUnitAABB(unit);
        //    DrawRectGizmo(aabb);
        //    DrawRectGizmo(unit.GetNeighborhoodRange());
        //}

        foreach (var squad in squads)
        {
            DrawSquadGizmo(squad);
        }
    }

    private void DrawSquadGizmo(SquadController squad)
    {
        if (squad == null) return;
        if (squad.Units == null) return;

        Rect aabb = squad.GetSquadAABB();

        Vector3 r0 = new Vector3(aabb.xMin, squad.transform.position.y + 0.1f, aabb.yMin);
        Vector3 r1 = new Vector3(aabb.xMax, squad.transform.position.y + 0.1f, aabb.yMin);
        Vector3 r2 = new Vector3(aabb.xMax, squad.transform.position.y + 0.1f, aabb.yMax);
        Vector3 r3 = new Vector3(aabb.xMin, squad.transform.position.y + 0.1f, aabb.yMax);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(r0, r1);
        Gizmos.DrawLine(r1, r2);
        Gizmos.DrawLine(r2, r3);
        Gizmos.DrawLine(r3, r0);
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

    #region backup
    //public void GetTarget(List<Unit> units)
    //{
    //    foreach (Unit unit in units)
    //    {
    //        Rect searchArea = GetUnitAABB(unit);
    //        List<Unit> neighbors = quadtree.Search(searchArea);
    //        Unit closestEnemy = null;
    //        float distanceToClosest = 0f;

    //        foreach (Unit neighbor in neighbors)
    //        {
    //            if (neighbor == unit) continue;
    //            if (neighbor.Squad == unit.Squad) continue;

    //            if (closestEnemy == null)
    //            {
    //                closestEnemy = neighbor;
    //                distanceToClosest = Vector3.Distance(unit.transform.position, neighbor.transform.position);
    //                continue;
    //            }

    //            float newDistance = Vector3.Distance(unit.transform.position, neighbor.transform.position);

    //            if (newDistance < distanceToClosest)
    //            {
    //                closestEnemy = neighbor;
    //                distanceToClosest = newDistance;
    //            }
    //        }

    //        unit.SetTargetUnit(closestEnemy);
    //    }
    //}
    #endregion
}
