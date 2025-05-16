using UnityEngine;

public enum FlankDirection
{
    None,
    Left,
    Right
}

public class FlankDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 6f;
    [Range(0f, 180f)]
    public float flankAngle = 90f; // total arc angle per flank side

    public LayerMask enemyLayer;

    [Header("Enemy Formation Reference")]
    public Transform enemyFormationCenter;
    public Vector3 enemyFormationForward; // Must be set externally

    // Outputs
    public bool IsFlanking { get; private set; }
    public FlankDirection FlankDir { get; private set; }
    public Transform DetectedFlankTarget { get; private set; }

    private Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public void DetectFlank()
    {
        IsFlanking = false;
        FlankDir = FlankDirection.None;
        DetectedFlankTarget = null;

        Vector3 unitPos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 leftBound = Quaternion.AngleAxis(-flankAngle / 2f, Vector3.up) * forward;
        Vector3 rightBound = Quaternion.AngleAxis(flankAngle / 2f, Vector3.up) * forward;

        Collider[] enemies = Physics.OverlapSphere(unitPos, detectionRadius, enemyLayer);

        foreach (var enemyCol in enemies)
        {
            Vector3 toEnemy = (enemyCol.transform.position - unitPos).normalized;
           
            float angle = Vector3.Angle(forward, toEnemy);
            if (!IsInSideSector(toEnemy, forward, flankAngle))
                continue;

            Vector3 toUsFromEnemy = (transform.position - enemyFormationCenter.position).normalized;
            float enemyDot = Vector3.Dot(enemyFormationForward.normalized, toUsFromEnemy);

            if (Mathf.Abs(enemyDot) < 0.5f) continue; 
            
            Vector3 enemyRight = Vector3.Cross(Vector3.up, enemyFormationForward.normalized);
            float sideDot = Vector3.Dot(enemyRight, toUsFromEnemy);

            FlankDir = (sideDot > 0f) ? FlankDirection.Right : FlankDirection.Left;
            DetectedFlankTarget = enemyCol.transform;
            IsFlanking = true;
            break;
        }
    }

    bool IsInSideSector(Vector3 toEnemy, Vector3 forward, float angle)
    {
        Vector3 left = Quaternion.AngleAxis(-angle / 2f, Vector3.up) * forward;
        Vector3 right = Quaternion.AngleAxis(angle / 2f, Vector3.up) * forward;

        // Use cross products to check if toEnemy lies between left and right bounds
        float crossLeft = Vector3.Cross(left, toEnemy).y;
        float crossRight = Vector3.Cross(toEnemy, right).y;

        return crossLeft >= 0 && crossRight >= 0;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;

        Vector3 left = Quaternion.AngleAxis(-flankAngle / 2f, Vector3.up) * forward;
        Vector3 right = Quaternion.AngleAxis(flankAngle / 2f, Vector3.up) * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pos, detectionRadius);
        Gizmos.DrawLine(pos, pos + left * detectionRadius);
        Gizmos.DrawLine(pos, pos + right * detectionRadius);

        if (IsFlanking && DetectedFlankTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, DetectedFlankTarget.position);
        }
    }
}
