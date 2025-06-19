using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("Configuraci�n de Ruta Fija")]
    public Transform startPoint;
    public Transform endPoint;
    public Transform[] waypoints;

    [Header("Configuraci�n de Navegaci�n")]
    public float arrivalRadius = 0.5f;
    public LayerMask obstacleMask = -1;
    public float anticipationDistance = 1.0f;

    [Header("Configuraci�n de Escape")]
    public float recalculateInterval = 0.5f;

    private PFManager pathfindingManager;
    private PFNodeGrid nodeGrid;

    private int currentWaypointIndex = 0;
    private int waypointDirection = 1;
    private bool reachedEndPoint = false;
    
    private List<PFNodes> escapePath;
    private int escapePathIndex = 0;
    private bool isEscaping = false;
    
    private float lastRecalculateTime;
    private Vector3 lastPlayerPosition;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        pathfindingManager = PFManager.Instance;
        nodeGrid = FindObjectOfType<PFNodeGrid>();

        if (pathfindingManager == null)
            Debug.LogWarning("No se encontró PFManager - el escape no funcionará");

        if (nodeGrid == null)
            Debug.LogWarning("No se encontró PFNodeGrid - el escape no funcionará");
        
        currentWaypointIndex = 0;
        waypointDirection = 1;
        reachedEndPoint = false;

        Debug.Log("MouseMovement inicializado - Modo waypoints para patrullaje, A* para escape");
    }

    public Vector3 GetCurrentTargetPosition()
    {
        if (isEscaping)
        {
            if (escapePath != null && escapePath.Count > 0)
            {
                if (escapePathIndex < escapePath.Count)
                    return escapePath[escapePathIndex].transform.position;
                else
                    return startPoint.position;
            }
            else
            {
                return startPoint.position;
            }
        }
        else
        {
            return GetCurrentWaypointPosition();
        }
    }

    Vector3 GetCurrentWaypointPosition()
    {
        if (!reachedEndPoint)
        {
            if (currentWaypointIndex < waypoints.Length)
            {
                return waypoints[currentWaypointIndex].position;
            }
            else
            {
                return endPoint.position;
            }
        }
        else
        {
            if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Length)
            {
                return waypoints[currentWaypointIndex].position;
            }
            else
            {
                return startPoint.position;
            }
        }
    }
    
    public bool HasReachedCurrentTarget(Vector3 position)
    {
        Vector3 target = GetCurrentTargetPosition();
        float distance = Vector3.Distance(position, target);

        float effectiveRadius = arrivalRadius;

        if (!isEscaping)
        {
            if ((!reachedEndPoint && target == endPoint.position) ||
                (reachedEndPoint && target == startPoint.position))
            {
                effectiveRadius = arrivalRadius * 0.7f;
            }
        }

        return distance <= effectiveRadius;
    }
    
    public bool ShouldPrepareForTurn(Vector3 position, Vector3 velocity)
    {
        if (isEscaping) return false;

        Vector3 target = GetCurrentTargetPosition();

        bool isAtEndpoint = (!reachedEndPoint && target == endPoint.position) ||
                           (reachedEndPoint && target == startPoint.position);

        if (!isAtEndpoint) return false;

        float distanceToTarget = Vector3.Distance(position, target);
        float currentSpeed = velocity.magnitude;
        float anticipationDist = Mathf.Clamp(currentSpeed * 0.5f, anticipationDistance * 0.5f, anticipationDistance);
        return distanceToTarget <= anticipationDist;
    }

    public void MoveToNextTarget()
    {
        if (isEscaping)
        {
            escapePathIndex++;

            if (escapePathIndex >= escapePath.Count)
            {
                if (Vector3.Distance(transform.position, startPoint.position) <= arrivalRadius)
                {
                    CompleteEscape();
                }
            }
        }
        else
        {
            MoveToNextWaypoint();
        }
    }

    void MoveToNextWaypoint()
    {
        if (!reachedEndPoint)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex > waypoints.Length)
            {
                reachedEndPoint = true;
                currentWaypointIndex = waypoints.Length - 1;
                waypointDirection = -1;
            }
        }
        else
        {
            currentWaypointIndex--;

            if (currentWaypointIndex < -1)
            {
                reachedEndPoint = false;
                currentWaypointIndex = 0;
                waypointDirection = 1;
            }
        }
    }

    public void StartEscape()
    {
        if (isEscaping) return;
        isEscaping = true;
        CalculateEscapePath();
    }

    void CalculateEscapePath()
    {
        if (pathfindingManager == null || nodeGrid == null)
        {
            CreateDirectEscapePath();
            return;
        }

        Vector3 currentPos = transform.position;
        PFNodes currentNode = GetClosestNode(currentPos);
        PFNodes startNode = GetClosestNode(startPoint.position);

        if (currentNode != null && startNode != null)
        {
            List<PFNodes> temporaryBlockedNodes = BlockNodesNearPlayer();

            try
            {
                escapePath = PathFinding.AstarPS(currentNode, startNode, obstacleMask);

                if (escapePath == null || escapePath.Count == 0)
                {
                    CreateDirectEscapePath();
                }
                else
                {
                    escapePathIndex = 0;
                }
            }
            catch (System.Exception e)
            {
                CreateDirectEscapePath();
            }
            finally
            {
                RestoreTemporaryBlockedNodes(temporaryBlockedNodes);
            }

            Transform player = GetPlayerTransform();
            if (player != null)
                lastPlayerPosition = player.position;
        }
        else
        {
            CreateDirectEscapePath();
        }
    }

    void CreateDirectEscapePath()
    {
        escapePath = null;
        escapePathIndex = 0;
    }

    void CompleteEscape()
    {
        isEscaping = false;
        escapePath = null;
        ResetToStart();
    }

    public void ResetToStart()
    {
        isEscaping = false;
        reachedEndPoint = false;
        currentWaypointIndex = 0;
        waypointDirection = 1;
        escapePath = null;
    }

    public void UpdatePath()
    {
        if (!isEscaping) return;

        if (Time.time - lastRecalculateTime < recalculateInterval)
            return;

        lastRecalculateTime = Time.time;

        Transform player = GetPlayerTransform();
        if (player != null)
        {
            float playerMovementDistance = Vector3.Distance(player.position, lastPlayerPosition);

            if (playerMovementDistance > 2f)
            {
                CalculateEscapePath();
            }
        }
    }

    PFNodes GetClosestNode(Vector3 worldPosition)
    {
        if (nodeGrid == null || nodeGrid.nodeGrid == null) return null;

        return nodeGrid.nodeGrid
            .Where(node => node != null && !node.Blocked)
            .OrderBy(node => Vector3.Distance(node.transform.position, worldPosition))
            .FirstOrDefault();
    }

    List<PFNodes> BlockNodesNearPlayer()
    {
        List<PFNodes> blockedNodes = new List<PFNodes>();
        Transform player = GetPlayerTransform();

        if (player == null || nodeGrid == null) return blockedNodes;

        float playerAvoidanceRadius = 3f;

        foreach (var node in nodeGrid.nodeGrid)
        {
            if (node == null || node.Blocked) continue;

            float distanceToPlayer = Vector3.Distance(node.transform.position, player.position);

            if (distanceToPlayer <= playerAvoidanceRadius)
            {
                var blockedField = typeof(PFNodes).GetField("blocked",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (blockedField != null)
                {
                    blockedField.SetValue(node, true);
                    blockedNodes.Add(node);
                }
            }
        }

        return blockedNodes;
    }

    void RestoreTemporaryBlockedNodes(List<PFNodes> nodesToRestore)
    {
        var blockedField = typeof(PFNodes).GetField("blocked",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (blockedField != null)
        {
            foreach (var node in nodesToRestore)
            {
                if (node != null)
                    blockedField.SetValue(node, false);
            }
        }
    }

    Transform GetPlayerTransform()
    {
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null && enemyController.PlayerTransform != null)
            return enemyController.PlayerTransform;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    public bool HasReachedEscapeTarget(Vector3 position)
    {
        if (!isEscaping || escapePath == null || escapePathIndex >= escapePath.Count)
            return false;

        Vector3 target = escapePath[escapePathIndex].transform.position;
        return Vector3.Distance(position, target) <= arrivalRadius;
    }

    public bool MoveToNextEscapePoint()
    {
        escapePathIndex++;

        return escapePathIndex >= escapePath.Count &&
               Vector3.Distance(transform.position, startPoint.position) <= arrivalRadius;
    }

    void OnDrawGizmosSelected()
    {
        if (startPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 0.5f);
        }

        if (endPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPoint.position, 0.5f);
        }

        if (startPoint != null && endPoint != null && waypoints != null)
        {
            Gizmos.color = isEscaping ? Color.gray : Color.blue;

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
        }

        if (isEscaping && escapePath != null && escapePath.Count > 1)
        {
            Gizmos.color = Color.yellow;

            for (int i = escapePathIndex; i < escapePath.Count - 1; i++)
            {
                if (escapePath[i] != null && escapePath[i + 1] != null)
                {
                    Gizmos.DrawLine(
                        escapePath[i].transform.position,
                        escapePath[i + 1].transform.position
                    );
                }
            }
        }

        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(GetCurrentTargetPosition(), arrivalRadius);
    }
}
