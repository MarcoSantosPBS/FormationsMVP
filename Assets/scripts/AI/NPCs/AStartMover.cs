using System.Collections.Generic;
using UnityEngine;

public class AStartMover : MonoBehaviour, IUnitMover
{
    [SerializeField] float speed;
    [SerializeField] float stoppingDistance = 0.5f;

    private List<Vector3> path;
    private Vector3 currentPosition;
    private Vector3 targetPosition;
    private Vector3 destination;
    private int index;

    private void Update()
    {
        if (path == null || path.Count == 0) return;

        if (Vector3.Distance(currentPosition, destination) < stoppingDistance)
        {
            path = null;
        }

        Vector3 delta = targetPosition - currentPosition;
        float dist = delta.magnitude;

        if (dist < stoppingDistance)
        {
            index++;
            currentPosition = targetPosition;
            targetPosition = path[index];
        }

        transform.position = Vector3.Lerp(currentPosition, targetPosition, speed * Time.deltaTime);
    }

    public void MoveToPosition(Vector3 destination)
    {
        path = GridManager.Instance.pathfinder.GetPathVector3(transform.position, destination);
        this.destination = destination;
        index = 0;
        currentPosition = transform.position;

        if (path == null || path.Count == 0)
            return;

        targetPosition = path[index];
    }
}
