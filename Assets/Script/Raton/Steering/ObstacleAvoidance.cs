using UnityEngine;
using UnityEngine.UIElements;

public class ObstacleAvoidance : MonoBehaviour
{
    [SerializeField] float detectionRange;
    [SerializeField] float avoidForce;
    [SerializeField] LayerMask obstacleMask;
    public Vector3 Avoid()
    {
        var colliders = Physics.OverlapSphere(transform.position, detectionRange, obstacleMask);
        float minDist = detectionRange + 1;
        Collider closestCol = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            //colliders[i].ClosestPoint(transform.position);
            //float currentDist = Vector3.Distance(transform.position, colliders[i].transform.position);
            float currentDist = Vector3.Distance(transform.position, colliders[i].ClosestPoint(transform.position));
            if (currentDist < minDist)
            {
                closestCol = colliders[i];
                minDist = currentDist;
            }
        }
        if (closestCol == null) return Vector3.zero;
        Vector3 dirToAvoid = transform.position - closestCol.ClosestPoint(transform.position);
        Vector3 avoidDir = new Vector3(dirToAvoid.x, 0, dirToAvoid.z).normalized * avoidForce;
        avoidDir *= Mathf.Lerp(1, 0, Vector3.Distance(transform.position, closestCol.ClosestPoint(transform.position)) / detectionRange);
        return avoidDir;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
