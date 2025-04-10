using UnityEngine;
using UnityEngine.AI;

public class NavMeshMover : MonoBehaviour, IUnitMover
{
    [SerializeField] private NavMeshAgent agent;

    private void Start()
    {
        agent.updateRotation = false;
    }

    private void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.05f)
        {
            agent.ResetPath();
        }
    }

    public void MoveToPosition(Vector3 destination)
    {
        agent.SetDestination(destination);
    }
}
