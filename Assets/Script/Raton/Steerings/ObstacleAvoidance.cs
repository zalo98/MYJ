using UnityEngine;

public class ObstacleAvoidance : MonoBehaviour
{
    [Header("Obstacle Avoidance Settings")]
    [SerializeField] float detectionRange = 2f;
    [SerializeField] float avoidForce = 5f;
    [SerializeField] LayerMask obstacleMask;

    public Vector3 Avoid()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, obstacleMask);

        if (colliders.Length == 0)
            return Vector3.zero;

        float closestDistance = detectionRange + 1f;
        Collider closestCollider = null;

        foreach (var collider in colliders)
        {
            float distance = Vector3.Distance(transform.position, collider.ClosestPoint(transform.position));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = collider;
            }
        }

        if (closestCollider == null)
            return Vector3.zero;

        Vector3 avoidDirection = (transform.position - closestCollider.ClosestPoint(transform.position)).normalized;
        avoidDirection.y = 0f;
        
        float avoidanceStrength = Mathf.Lerp(avoidForce, 0f, closestDistance / detectionRange);

        return avoidDirection * avoidanceStrength;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}