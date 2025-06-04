using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("Configuración de Ruta Fija")]
    public Transform startPoint; // Punto A
    public Transform endPoint;   // Punto B
    public Transform[] waypoints; // Puntos intermedios (como antes)

    [Header("Configuración de Navegación")]
    public float arrivalRadius = 0.5f;
    public LayerMask obstacleMask = -1; // Solo para pathfinding de escape
    public float anticipationDistance = 1.0f; // Distancia para anticipar llegada a endpoints

    [Header("Configuración de Escape")]
    public float recalculateInterval = 0.5f; // Recálculo de escape más frecuente

    // Referencias del sistema (solo para escape)
    private PFManager pathfindingManager;
    private PFNodeGrid nodeGrid;

    // Sistema de waypoints fijos (patrullaje normal)
    private int currentWaypointIndex = 0;
    private int waypointDirection = 1; // 1 adelante, -1 atrás
    private bool reachedEndPoint = false;

    // Sistema de escape con A*
    private List<PFNodes> escapePath;
    private int escapePathIndex = 0;
    private bool isEscaping = false;

    // Control de recálculo de escape
    private float lastRecalculateTime;
    private Vector3 lastPlayerPosition;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        // Solo obtener referencias de pathfinding si las necesitamos para escape
        pathfindingManager = PFManager.Instance;
        nodeGrid = FindObjectOfType<PFNodeGrid>();

        // No es crítico si no las encuentra (solo afecta al escape)
        if (pathfindingManager == null)
            Debug.LogWarning("No se encontró PFManager - el escape no funcionará");

        if (nodeGrid == null)
            Debug.LogWarning("No se encontró PFNodeGrid - el escape no funcionará");

        // Verificar waypoints
        if (startPoint == null || endPoint == null)
            Debug.LogError("Puntos de inicio y final no asignados en MouseMovement");

        // Posicionar al enemigo en el punto inicial
        if (startPoint != null)
            transform.position = startPoint.position;

        // Inicializar sistema de waypoints
        currentWaypointIndex = 0;
        waypointDirection = 1;
        reachedEndPoint = false;

        Debug.Log("MouseMovement inicializado - Modo waypoints para patrullaje, A* para escape");
    }

    // Obtener la posición del objetivo actual
    public Vector3 GetCurrentTargetPosition()
    {
        if (isEscaping)
        {
            // MODO ESCAPE: Usar A* pathfinding o escape directo
            if (escapePath != null && escapePath.Count > 0)
            {
                // Usando A* pathfinding
                if (escapePathIndex < escapePath.Count)
                    return escapePath[escapePathIndex].transform.position;
                else
                    return startPoint.position; // Fallback al punto inicial
            }
            else
            {
                // Escape directo cuando A* falla - ir directo al inicio
                return startPoint.position;
            }
        }
        else
        {
            // MODO PATRULLAJE: Usar waypoints fijos
            return GetCurrentWaypointPosition();
        }
    }

    // Obtener posición del waypoint actual (sistema fijo)
    Vector3 GetCurrentWaypointPosition()
    {
        if (!reachedEndPoint) // Ida (A ? B)
        {
            if (currentWaypointIndex < waypoints.Length)
            {
                return waypoints[currentWaypointIndex].position;
            }
            else
            {
                // Va hacia el endPoint
                return endPoint.position;
            }
        }
        else // Vuelta (B ? A)
        {
            if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Length)
            {
                return waypoints[currentWaypointIndex].position;
            }
            else
            {
                // Va hacia el startPoint
                return startPoint.position;
            }
        }
    }

    // Verificar si ha llegado al punto de destino actual
    public bool HasReachedCurrentTarget(Vector3 position)
    {
        Vector3 target = GetCurrentTargetPosition();
        float distance = Vector3.Distance(position, target);

        // Usar un radio más pequeño para endpoints para transiciones más rápidas
        float effectiveRadius = arrivalRadius;

        if (!isEscaping) // Solo en patrullaje
        {
            if ((!reachedEndPoint && target == endPoint.position) ||
                (reachedEndPoint && target == startPoint.position))
            {
                effectiveRadius = arrivalRadius * 0.7f; // 30% más pequeño para endpoints
            }
        }

        return distance <= effectiveRadius;
    }

    // Método adicional para anticipar llegada a endpoints
    public bool ShouldPrepareForTurn(Vector3 position, Vector3 velocity)
    {
        if (isEscaping) return false; // Solo para patrullaje

        Vector3 target = GetCurrentTargetPosition();

        // Solo anticipar en endpoints
        bool isAtEndpoint = (!reachedEndPoint && target == endPoint.position) ||
                           (reachedEndPoint && target == startPoint.position);

        if (!isAtEndpoint) return false;

        float distanceToTarget = Vector3.Distance(position, target);
        float currentSpeed = velocity.magnitude;

        // Anticipar basado en velocidad actual
        float anticipationDist = Mathf.Clamp(currentSpeed * 0.5f, anticipationDistance * 0.5f, anticipationDistance);

        return distanceToTarget <= anticipationDist;
    }

    // Avanzar al siguiente punto en la ruta
    public void MoveToNextTarget()
    {
        if (isEscaping)
        {
            // MODO ESCAPE: Avanzar en el path A*
            escapePathIndex++;

            // Si llegó al final de la ruta de escape
            if (escapePathIndex >= escapePath.Count)
            {
                // Verificar si está cerca del punto inicial
                if (Vector3.Distance(transform.position, startPoint.position) <= arrivalRadius)
                {
                    CompleteEscape();
                }
            }
        }
        else
        {
            // MODO PATRULLAJE: Sistema de waypoints fijo
            MoveToNextWaypoint();
        }
    }

    // Avanzar al siguiente waypoint (sistema fijo)
    void MoveToNextWaypoint()
    {
        Debug.Log($"MoveToNextWaypoint llamado - reachedEndPoint: {reachedEndPoint}, currentWaypointIndex: {currentWaypointIndex}, waypoints.Length: {waypoints.Length}");

        if (!reachedEndPoint) // Modo ida (A ? B)
        {
            currentWaypointIndex++;
            Debug.Log($"Modo IDA - Nuevo índice: {currentWaypointIndex}");

            // Si acabamos de pasar el último waypoint, ahora va hacia endPoint
            if (currentWaypointIndex > waypoints.Length)
            {
                // Llegó al endPoint, cambiar a modo vuelta
                reachedEndPoint = true;
                currentWaypointIndex = waypoints.Length - 1; // Empezar desde el último waypoint
                waypointDirection = -1;
                Debug.Log("?? Llegó al punto B, iniciando regreso - índice: " + currentWaypointIndex);
            }
        }
        else // Modo vuelta (B ? A)
        {
            currentWaypointIndex--;
            Debug.Log($"Modo VUELTA - Nuevo índice: {currentWaypointIndex}");

            // Si ya pasó el primer waypoint, ahora va hacia startPoint
            if (currentWaypointIndex < -1)
            {
                // Llegó al startPoint, cambiar a modo ida
                reachedEndPoint = false;
                currentWaypointIndex = 0; // Empezar desde el primer waypoint
                waypointDirection = 1;
                Debug.Log("?? Llegó al punto A, iniciando nueva ida - índice: " + currentWaypointIndex);
            }
        }

        Debug.Log($"Próximo objetivo: {GetCurrentWaypointPosition()}");
    }

    // Iniciar escape con A* pathfinding
    public void StartEscape()
    {
        if (isEscaping) return; // Ya está escapando

        Debug.Log("Iniciando escape con A* pathfinding");
        isEscaping = true;
        CalculateEscapePath();
    }

    // Calcular path de escape evitando al player
    void CalculateEscapePath()
    {
        // Verificar que tenemos los componentes necesarios
        if (pathfindingManager == null || nodeGrid == null)
        {
            Debug.LogWarning("No se puede calcular escape path - falta PFManager o PFNodeGrid. Usando escape directo.");
            // Fallback: ir directo al punto inicial sin pathfinding
            CreateDirectEscapePath();
            return;
        }

        Vector3 currentPos = transform.position;
        PFNodes currentNode = GetClosestNode(currentPos);
        PFNodes startNode = GetClosestNode(startPoint.position);

        if (currentNode != null && startNode != null)
        {
            // Marcar temporalmente nodos cerca del player como bloqueados
            List<PFNodes> temporaryBlockedNodes = BlockNodesNearPlayer();

            try
            {
                // Calcular path evitando al player
                escapePath = PathFinding.AstarPS(currentNode, startNode, obstacleMask);

                if (escapePath == null || escapePath.Count == 0)
                {
                    Debug.LogWarning("A* no pudo encontrar path válido, usando escape directo");
                    CreateDirectEscapePath();
                }
                else
                {
                    escapePathIndex = 0;
                    Debug.Log($"Ruta de escape A* calculada con {escapePath.Count} nodos");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error calculando A* path: {e.Message}. Usando escape directo.");
                CreateDirectEscapePath();
            }
            finally
            {
                // Restaurar nodos bloqueados temporalmente
                RestoreTemporaryBlockedNodes(temporaryBlockedNodes);
            }

            // Guardar posición del player para detectar cambios
            Transform player = GetPlayerTransform();
            if (player != null)
                lastPlayerPosition = player.position;
        }
        else
        {
            Debug.LogWarning("No se pudieron encontrar nodos válidos para A*, usando escape directo");
            CreateDirectEscapePath();
        }
    }

    // Crear un path de escape directo cuando A* falla
    void CreateDirectEscapePath()
    {
        // Crear un path simple directo al punto inicial
        // El ObstacleAvoidance en EnemySteering se encargará de evitar paredes
        escapePath = null; // Indicar que no hay path A*
        escapePathIndex = 0;
        Debug.Log("Usando escape directo al punto inicial - ObstacleAvoidance manejará las paredes");
    }

    // Completar escape y volver al patrullaje
    void CompleteEscape()
    {
        Debug.Log("Escape completado - volviendo a patrullaje");
        isEscaping = false;
        escapePath = null;
        ResetToStart();
    }

    // Reiniciar al estado inicial de patrullaje
    public void ResetToStart()
    {
        isEscaping = false;
        reachedEndPoint = false;
        currentWaypointIndex = 0;
        waypointDirection = 1;
        escapePath = null;
    }

    // Actualizar path de escape si es necesario
    public void UpdatePath()
    {
        // Solo recalcular durante el escape
        if (!isEscaping) return;

        // Solo recalcular cada cierto intervalo
        if (Time.time - lastRecalculateTime < recalculateInterval)
            return;

        lastRecalculateTime = Time.time;

        // Verificar si el player se ha movido significativamente
        Transform player = GetPlayerTransform();
        if (player != null)
        {
            float playerMovementDistance = Vector3.Distance(player.position, lastPlayerPosition);

            // Si el player se movió más de 2 unidades, recalcular escape
            if (playerMovementDistance > 2f)
            {
                Debug.Log("Player se movió durante escape, recalculando path");
                CalculateEscapePath();
            }
        }
    }

    // Obtener el nodo más cercano a una posición
    PFNodes GetClosestNode(Vector3 worldPosition)
    {
        if (nodeGrid == null || nodeGrid.nodeGrid == null) return null;

        return nodeGrid.nodeGrid
            .Where(node => node != null && !node.Blocked)
            .OrderBy(node => Vector3.Distance(node.transform.position, worldPosition))
            .FirstOrDefault();
    }

    // Bloquear temporalmente nodos cerca del player
    List<PFNodes> BlockNodesNearPlayer()
    {
        List<PFNodes> blockedNodes = new List<PFNodes>();
        Transform player = GetPlayerTransform();

        if (player == null || nodeGrid == null) return blockedNodes;

        float playerAvoidanceRadius = 3f; // Radio alrededor del player a evitar

        foreach (var node in nodeGrid.nodeGrid)
        {
            if (node == null || node.Blocked) continue;

            float distanceToPlayer = Vector3.Distance(node.transform.position, player.position);

            if (distanceToPlayer <= playerAvoidanceRadius)
            {
                // Usar reflexión para acceder al campo privado 'blocked'
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

    // Restaurar nodos que fueron bloqueados temporalmente
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

    // Obtener referencia al player
    Transform GetPlayerTransform()
    {
        // Método 1: Si tienes una referencia directa
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null && enemyController.PlayerTransform != null)
            return enemyController.PlayerTransform;

        // Método 2: Buscar por tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    // Métodos de compatibilidad para escape
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

        // Retorna true si ha completado el escape
        return escapePathIndex >= escapePath.Count &&
               Vector3.Distance(transform.position, startPoint.position) <= arrivalRadius;
    }

    // Para visualización en el editor
    void OnDrawGizmosSelected()
    {
        // Dibujar puntos de inicio y fin
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

        // Dibujar ruta de waypoints fijos
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

        // Dibujar escape path (A*)
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

        // Dibujar radio de llegada
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(GetCurrentTargetPosition(), arrivalRadius);
    }
}
