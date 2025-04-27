using UnityEngine;

public class WaypointSystem : MonoBehaviour
{
    [Header("Configuración de Ruta")]
    public Transform startPoint; // Punto A
    public Transform endPoint;   // Punto B
    public Transform[] waypoints; // Puntos intermedios

    [Header("Configuración de Navegación")]
    public float arrivalRadius = 0.5f;

    private int currentWaypointIndex = 0;
    private int waypointDirection = 1; // 1 adelante, -1 atrás
    private bool reachedEndPoint = false;

    // Variables para el escape
    private bool returningByPath = false;
    private int escapeWaypointIndex = 0;

    void Start()
    {
        // Verificar puntos de ruta
        if (startPoint == null || endPoint == null)
            Debug.LogError("Puntos de inicio y final no asignados en WaypointSystem");

        // Posicionar al enemigo en el punto inicial (si está en Start)
        if (startPoint != null)
            transform.position = startPoint.position;
    }

    // Obtener la posición del objetivo actual en modo normal
    public Vector3 GetCurrentTargetPosition()
    {
        if (!reachedEndPoint) // Ida (A → B)
        {
            if (currentWaypointIndex < waypoints.Length)
                return waypoints[currentWaypointIndex].position;
            else
                return endPoint.position;
        }
        else // Vuelta (B → A)
        {
            if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Length)
                return waypoints[currentWaypointIndex].position;
            else
                return startPoint.position;
        }
    }

    // Obtener la posición del objetivo actual en modo escape
    public Vector3 GetEscapeTargetPosition()
    {
        if (escapeWaypointIndex >= 0 && escapeWaypointIndex < waypoints.Length)
            return waypoints[escapeWaypointIndex].position;
        else
            return startPoint.position;
    }

    // Verificar si ha llegado al punto de destino actual
    public bool HasReachedCurrentTarget(Vector3 position)
    {
        Vector3 target = GetCurrentTargetPosition();
        float distance = Vector3.Distance(position, target);
        return distance <= arrivalRadius;
    }

    // Avanzar al siguiente punto de la ruta normal
    public void MoveToNextTarget()
    {
        // Si llegó al punto B
        if (!reachedEndPoint && currentWaypointIndex >= waypoints.Length)
        {
            reachedEndPoint = true;
            currentWaypointIndex = waypoints.Length - 1;
            waypointDirection = -1;
            return;
        }

        // Si llegó al punto A después de estar en B
        if (reachedEndPoint && currentWaypointIndex < 0)
        {
            reachedEndPoint = false;
            currentWaypointIndex = 0;
            waypointDirection = 1;
            return;
        }

        // Avance normal
        currentWaypointIndex += waypointDirection;
    }

    // Iniciar ruta de escape
    public void StartEscapeRoute()
    {
        returningByPath = true;
        escapeWaypointIndex = DetermineClosestPreviousWaypoint();
    }

    public bool MoveToNextEscapePoint()
    {
        escapeWaypointIndex--;

        // Si ya no hay más waypoints para el escape
        if (escapeWaypointIndex < 0)
        {
            // Si estamos lo suficientemente cerca del punto inicial
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

    // Reiniciar al estado inicial
    public void ResetToStart()
    {
        reachedEndPoint = false;
        currentWaypointIndex = 0;
        waypointDirection = 1;
        returningByPath = false;
    }

    // Determinar el waypoint anterior más cercano (para escape)
    private int DetermineClosestPreviousWaypoint()
    {
        // Si no hay waypoints, retornar -1 para ir directo al punto inicial
        if (waypoints == null || waypoints.Length == 0)
            return -1;

        // Si ya había pasado todos los waypoints y estaba dirigiéndose al punto final
        if (currentWaypointIndex >= waypoints.Length)
            return waypoints.Length - 1;

        // Si estaba de regreso (después de alcanzar el endpoint)
        if (reachedEndPoint)
            return currentWaypointIndex;

        // Si estaba camino hacia adelante, usar el índice actual - 1
        return Mathf.Max(0, currentWaypointIndex - 1);
    }

    // Verificar si ha llegado al punto de destino de escape actual
    public bool HasReachedEscapeTarget(Vector3 position)
    {
        Vector3 target = GetEscapeTargetPosition();
        float distance = Vector3.Distance(position, target);
        return distance <= arrivalRadius;
    }

    // Para visualización de la ruta en el editor
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

            // Dibujar radio de llegada
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(GetCurrentTargetPosition(), arrivalRadius);
        }
    }
}
