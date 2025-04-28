using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleAvoidance : MonoBehaviour
{
    [SerializeField] public float detectionRange;
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
        //.NoY()
        Vector3 avoidDir = ((transform.position - closestCol.ClosestPoint(transform.position)).normalized * avoidForce);
        avoidDir *= Mathf.Lerp(1, 0, Vector3.Distance(transform.position, closestCol.ClosestPoint(transform.position)) / detectionRange);
        return avoidDir;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
