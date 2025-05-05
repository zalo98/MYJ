using UnityEngine;

public class WaypointSystem : MonoBehaviour
{
    [Header("Configuración de Ruta")]
    public Transform startPoint;
    public Transform endPoint;
    public Transform[] waypoints;

    [Header("Configuración de Navegación")]
    public float arrivalRadius = 0.5f;

    private int currentWaypointIndex = 0;
    private int waypointDirection = 1;
    private bool reachedEndPoint = false;
    
    private bool returningByPath = false;
    private int escapeWaypointIndex = 0;

    void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("Waypoints no asignados en WaypointSystem");
        }
    }
    
    public Vector3 GetCurrentTargetPosition()
    {
        if (!reachedEndPoint)
        {
            if (currentWaypointIndex < waypoints.Length)
                return waypoints[currentWaypointIndex].position;
            else
                return endPoint.position;
        }
        else
        {
            if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Length)
                return waypoints[currentWaypointIndex].position;
            else
                return startPoint.position;
        }
    }
    
    public Vector3 GetEscapeTargetPosition()
    {
        if (escapeWaypointIndex >= 0 && escapeWaypointIndex < waypoints.Length)
            return waypoints[escapeWaypointIndex].position;
        else
            return startPoint.position;
    }
    
    public bool HasReachedCurrentTarget(Vector3 position)
    {
        Vector3 target = GetCurrentTargetPosition();
        float distance = Vector3.Distance(position, target);
        return distance <= arrivalRadius;
    }

    public void MoveToNextTarget()
    {
        if (!reachedEndPoint)
        {
            if (currentWaypointIndex < waypoints.Length - 1)
            {
                currentWaypointIndex++;
            }
            else
            {
                reachedEndPoint = true;
            }
        }
        else
        {
            if (currentWaypointIndex > 0)
            {
                currentWaypointIndex--;
            }
            else
            {
                reachedEndPoint = false;
            }
        }
    }
    
    public void StartEscapeRoute()
    {
        returningByPath = true;
        escapeWaypointIndex = DetermineClosestPreviousWaypoint();
    }

    public bool MoveToNextEscapePoint()
    {
        escapeWaypointIndex--;
        
        if (escapeWaypointIndex < 0)
        {
            float distanceToStart = Vector3.Distance(transform.position, startPoint.position);
            if (distanceToStart <= arrivalRadius)
            {
                returningByPath = false;
                Debug.Log("Escape completado");
                return true;
            }
        }

        return false;
    }
    
    public void ResetToStart()
    {
        reachedEndPoint = false;
        currentWaypointIndex = 0;
        waypointDirection = 1;
        returningByPath = false;
    }
    
    private int DetermineClosestPreviousWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return -1;
        }

        if (currentWaypointIndex >= waypoints.Length)
        {
            return waypoints.Length - 1;
        }

        if (reachedEndPoint)
        {
            return currentWaypointIndex;
        }
        
        return Mathf.Max(0, currentWaypointIndex - 1);
    }
    
    public bool HasReachedEscapeTarget(Vector3 position)
    {
        Vector3 target = GetEscapeTargetPosition();
        float distance = Vector3.Distance(position, target);
        return distance <= arrivalRadius;
    }
    
    void OnDrawGizmosSelected()
    {
        if (startPoint != null && endPoint != null && waypoints != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPoint.position, 0.5f);

            Gizmos.color = Color.blue;

            if (waypoints.Length > 0 && waypoints[0] != null)
                Gizmos.DrawLine(startPoint.position, waypoints[0].position);

            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }

            if (waypoints.Length > 0 && waypoints[waypoints.Length - 1] != null)
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, endPoint.position);

            foreach (var waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawSphere(waypoint.position, 0.3f);
                }
            }
            
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(GetCurrentTargetPosition(), arrivalRadius);
        }
    }
}
