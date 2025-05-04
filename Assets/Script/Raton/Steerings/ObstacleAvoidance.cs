using UnityEngine;

public class ObstacleAvoidance : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float avoidForce = 5f;
    [SerializeField] private LayerMask obstacleMask;

    public Vector3 Avoid()
    {
        var colliders = Physics.OverlapSphere(transform.position, detectionRange, obstacleMask);

        float minDist = detectionRange + 1;
        Collider closestCol = null;

        for (int i = 0; i < colliders.Length; i++)
        {
            float currentDist = Vector3.Distance(transform.position, colliders[i].ClosestPoint(transform.position));
            if (currentDist < minDist)
            {
                closestCol = colliders[i];
                minDist = currentDist;
            }
        }

        if (closestCol == null) return Vector3.zero;

        Vector3 avoidDir = (transform.position - closestCol.ClosestPoint(transform.position)).normalized;
        avoidDir.y = 0;
        avoidDir *= avoidForce;
        avoidDir *= Mathf.Lerp(1, 0, minDist / detectionRange);

        return avoidDir;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}