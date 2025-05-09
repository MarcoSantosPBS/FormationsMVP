using System;
using UnityEngine;

public class teste : MonoBehaviour
{
    public Transform debugAlvo;
    public Transform debugSource;
    public Transform debugNeighbor;
    public Transform debugVizinho2;
    public Transform debugPivo;

    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Vector3 dirToAlvo = (debugAlvo.position - debugPivo.position).normalized;
        Vector3 dirToNeighbor = (debugNeighbor.position - debugPivo.position).normalized;
        Vector3 dirTargetToNeighbor = (debugNeighbor.position - debugAlvo.position).normalized;

        float sourceToAlvoRight = Vector3.Dot(dirToAlvo, debugSource.right);
        float alvoToOtherRight = Vector3.Dot(-dirTargetToNeighbor, Vector3.right);
        float alvoToOTherForward = Vector3.Dot(dirTargetToNeighbor, -dirToAlvo);

        float lateralOffset = Vector3.Cross(dirTargetToNeighbor, -dirToAlvo.normalized).magnitude * dirToNeighbor.magnitude;
        float frontalOffset = Vector3.Cross(dirTargetToNeighbor, Vector3.right).magnitude * dirToNeighbor.magnitude;

        bool hasSomeoneOnCommingSide = sourceToAlvoRight * alvoToOtherRight >= 0f;
        bool isBlockingLaterally;

        if (hasSomeoneOnCommingSide)
        {
            isBlockingLaterally = (Mathf.Abs(alvoToOtherRight) - frontalOffset) > 0.2f;
        }
        else
        {
            isBlockingLaterally = false;
        }

        bool isBlockedFrontally = (alvoToOTherForward - lateralOffset) > 0.50;

        Debug.Log($"{alvoToOtherRight}");
        //Debug.Log($"Está bloqueando lateralmente: {isBlockingLaterally}");
        Debug.Log($"Está bloqueando frontalmente: {isBlockedFrontally}");
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(debugPivo.position, debugAlvo.position);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(debugAlvo.position, debugNeighbor.position);
    }
}
