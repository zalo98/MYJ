using UnityEngine;

public class ObstacleAvoidance : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float avoidForce = 5f;
    [SerializeField] private LayerMask obstacleMask;

    public Vector3 Avoid()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, obstacleMask);
        Collider closest = null;
        float minDistance = detectionRange;

        foreach (var col in colliders)
        {
            float dist = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
            if (dist < minDistance)
            {
                closest = col;
                minDistance = dist;
            }
        }

        if (closest == null) return Vector3.zero;

        Vector3 dir = (transform.position - closest.ClosestPoint(transform.position)).normalized;
        dir.y = 0;

        float forceScale = Mathf.Lerp(1f, 0f, minDistance / detectionRange);
        return dir * avoidForce * forceScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}