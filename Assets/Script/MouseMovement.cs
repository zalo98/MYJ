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

    [Header("Portal Escape System")]
    public bool usePortalEscape = true;
    private Portal targetPortal;
    private bool escapingToPortal = false;

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
            if (escapingToPortal && targetPortal != null)
            {
                // MODO ESCAPE A PORTAL
                if (escapePath != null && escapePath.Count > 0)
                {
                    // Usando A* hacia portal
                    if (escapePathIndex < escapePath.Count)
                    {
                        Vector3 target = escapePath[escapePathIndex].transform.position;
                        return target;
                    }
                    else
                    {
                        // Llegó al final del path, ir directo al portal
                        return targetPortal.transform.position;
                    }
                }
                else
                {
                    // Ir directo al portal sin A*
                    return targetPortal.transform.position;
                }
            }
            else
            {
                // Sistema de escape anterior (fallback)
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
        }
        else
        {
            // MODO PATRULLAJE: Usar waypoints fijos
            Vector3 target = GetCurrentWaypointPosition();
            return target;
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

    // Avanzar al siguiente punto en la ruta
    public void MoveToNextTarget()
    {
        if (isEscaping)
        {
            if (escapingToPortal && targetPortal != null)
            {
                // ESCAPE A PORTAL
                if (escapePath != null && escapePath.Count > 0)
                {
                    escapePathIndex++;

                    // Si llegó al final del path A*, verificar si está cerca del portal
                    if (escapePathIndex >= escapePath.Count)
                    {
                        float distanceToPortal = Vector3.Distance(transform.position, targetPortal.transform.position);
                        if (distanceToPortal <= targetPortal.activationRadius * 1.5f)
                        {
                            Debug.Log("?? Cerca del portal, el trigger se encargará del teleport");
                            // El portal mismo manejará el teleport via OnTriggerEnter
                        }
                    }
                }
                else
                {
                    // Movimiento directo al portal, verificar distancia
                    float distanceToPortal = Vector3.Distance(transform.position, targetPortal.transform.position);
                    if (distanceToPortal <= targetPortal.activationRadius * 1.5f)
                    {
                        Debug.Log("?? Cerca del portal, esperando trigger");
                    }
                }
            }
            else
            {
                // Sistema anterior (fallback)
                escapePathIndex++;
                if (escapePathIndex >= escapePath.Count)
                {
                    if (Vector3.Distance(transform.position, startPoint.position) <= arrivalRadius)
                    {
                        CompleteEscape();
                    }
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

        if (!reachedEndPoint) // Modo ida (A ? B)
        {
            currentWaypointIndex++;

            // Si acabamos de pasar el último waypoint, ahora va hacia endPoint
            if (currentWaypointIndex > waypoints.Length)
            {
                // Llegó al endPoint, cambiar a modo vuelta
                reachedEndPoint = true;
                currentWaypointIndex = waypoints.Length - 1; // Empezar desde el último waypoint
                waypointDirection = -1;
            }
        }
        else // Modo vuelta (B ? A)
        {
            currentWaypointIndex--;

            // Si ya pasó el primer waypoint, ahora va hacia startPoint
            if (currentWaypointIndex < -1)
            {
                // Llegó al startPoint, cambiar a modo ida
                reachedEndPoint = false;
                currentWaypointIndex = 0; // Empezar desde el primer waypoint
                waypointDirection = 1;
            }
        }
    }

    // Iniciar escape hacia portal
    public void StartEscape()
    {
        if (isEscaping) return;

        isEscaping = true;

        if (usePortalEscape)
        {
            StartPortalEscape();
        }
        else
        {
            // Fallback al sistema anterior
            CalculateEscapePath();
        }
    }

    // Iniciar escape hacia portal
    void StartPortalEscape()
    {
        Vector3 currentPos = transform.position;
        Vector3 playerPos = GetPlayerPosition();

        // Encontrar el mejor portal
        targetPortal = PortalManager.Instance?.FindBestPortal(currentPos, playerPos);

        if (targetPortal == null)
        {
            CreateDirectEscapePath();
            return;
        }

        // Calcular path A* hacia el portal
        CalculatePathToPortal();
    }

    // Calcular path A* hacia el portal
    void CalculatePathToPortal()
    {
        if (pathfindingManager == null || nodeGrid == null)
        {
            escapingToPortal = true;
            escapePath = null;
            return;
        }

        Vector3 currentPos = transform.position;
        Vector3 portalPos = targetPortal.transform.position;

        PFNodes startNode = GetClosestNode(currentPos);
        PFNodes portalNode = GetClosestNode(portalPos);

        if (startNode != null && portalNode != null)
        {

            // Usar A* estándar (no necesitamos heurística táctica compleja)
            escapePath = PathFinding.AstarPS(startNode, portalNode, obstacleMask);
            escapePathIndex = 0;
            escapingToPortal = true;

            if (escapePath != null && escapePath.Count > 0)
            {
                Debug.Log($"? Path A* al portal calculado con {escapePath.Count} nodos");

                // Debug: mostrar el path
                for (int i = 0; i < escapePath.Count; i++)
                {
                    Debug.Log($"  Nodo {i}: {escapePath[i].transform.position}");
                }
            }
            else
            {
                Debug.LogWarning("?? A* falló, yendo directo al portal");
                escapePath = null;
            }
        }
        else
        {
            Debug.LogWarning("? No se encontraron nodos válidos, yendo directo al portal");
            escapePath = null;
            escapingToPortal = true;
        }
    }

    // Obtener posición del player
    Vector3 GetPlayerPosition()
    {
        Transform player = GetPlayerTransform();
        return player != null ? player.position : transform.position;
    }

    // Método llamado por el portal cuando teleporta
    public void CompleteEscapeViaPortal()
    {
        isEscaping = false;
        escapingToPortal = false;
        targetPortal = null;
        escapePath = null;
        ResetToStart();
    }

    // Calcular path de escape evitando al player (fallback)
    void CalculateEscapePath()
    {
        // Verificar que tenemos los componentes necesarios
        if (pathfindingManager == null || nodeGrid == null)
        {
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
            //List<PFNodes> temporaryBlockedNodes = BlockNodesNearPlayer();

            try
            {
                // Calcular path evitando al player
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
    }

    // Completar escape y volver al patrullaje
    void CompleteEscape()
    {
        isEscaping = false;
        escapePath = null;
        ResetToStart();
    }

    // Reiniciar al estado inicial de patrullaje
    public void ResetToStart()
    {
        isEscaping = false;
        escapingToPortal = false;
        targetPortal = null;
        reachedEndPoint = false;
        currentWaypointIndex = 0;
        waypointDirection = 1;
        escapePath = null;
    }

    // Actualizar path de escape si es necesario
    public void UpdatePath()
    {
        if (!isEscaping) return;

        // Solo recalcular cada cierto intervalo
        if (Time.time - lastRecalculateTime < recalculateInterval)
            return;

        lastRecalculateTime = Time.time;

        if (escapingToPortal)
        {
            // Verificar si el portal sigue siendo válido
            if (targetPortal == null || !targetPortal.IsAvailable)
            {
                StartPortalEscape();
                return;
            }

            // Verificar si el player se movió y hay un portal mejor
            Vector3 playerPos = GetPlayerPosition();
            Portal betterPortal = PortalManager.Instance?.FindBestPortal(transform.position, playerPos);

            if (betterPortal != null && betterPortal != targetPortal)
            {CalculatePathToPortal();
            }
        }
        else
        {
            // Sistema anterior de escape
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

        // Dibujar escape path A* hacia portal
        if (isEscaping && escapingToPortal && escapePath != null && escapePath.Count > 1)
        {
            Gizmos.color = Color.cyan; // Color diferente para path hacia portal

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

        // Dibujar linea al portal objetivo
        if (isEscaping && escapingToPortal && targetPortal != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetPortal.transform.position);
            Gizmos.DrawWireSphere(targetPortal.transform.position, 1f);
        }

        // Dibujar radio de llegada
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(GetCurrentTargetPosition(), arrivalRadius);
    }
}