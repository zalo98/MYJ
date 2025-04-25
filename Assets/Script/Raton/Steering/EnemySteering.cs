using UnityEngine;
using UnityEngine.AI;

public class EnemySteering : MonoBehaviour
{
    // Referencias
    [HideInInspector] public EnemyController controller;
    private Rigidbody rb;

    // Comportamientos de steering
    private Seek seekBehavior;
    private Flee fleeBehavior;
    private Pursuit pursuitBehavior;
    private Evade evadeBehavior;
    private ObstacleAvoidance obstacleBehavior;

    // Propiedades para los comportamientos
    [HideInInspector] public Vector3 currentVelocity;
    [HideInInspector] public Vector3 currentTargetPosition;
    [HideInInspector] public float currentMaxSpeed;
    [HideInInspector] public Vector3 lastTargetPosition;

    [Header("Configuración de Ruta")]
    public Transform startPoint; // Punto A
    public Transform endPoint;   // Punto B
    public Transform[] waypoints; // Puntos intermedios
    private int currentWaypointIndex = 0;
    private int waypointDirection = 1; // 1 adelante, -1 atrás

    [Header("Steering Parameters")]
    public float maxSteeringForce = 10f;
    public float arrivalRadius = 0.5f;

    [Header("Obstacle Avoidance")]
    public float obstacleDetectionRadius = 3f;
    public float obstacleAvoidanceWeight = 1.5f;
    public LayerMask obstacleLayer;

    // Estados de movimiento
    private bool goingForward = true; // True: A → B, False: escapa → A
    private bool reachedEndPoint = false; // Si ya llegó a B y vuelve

    public void Initialize()
    {
        Debug.Log("EnemySteering: Inicializando");

        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody>();

        // Obtener comportamientos de steering
        seekBehavior = GetComponent<Seek>();
        fleeBehavior = GetComponent<Flee>();
        pursuitBehavior = GetComponent<Pursuit>();
        evadeBehavior = GetComponent<Evade>();
        obstacleBehavior = GetComponent<ObstacleAvoidance>();

        if (seekBehavior == null) seekBehavior = gameObject.AddComponent<Seek>();
        if (fleeBehavior == null) fleeBehavior = gameObject.AddComponent<Flee>();
        if (pursuitBehavior == null) pursuitBehavior = gameObject.AddComponent<Pursuit>();
        if (evadeBehavior == null) evadeBehavior = gameObject.AddComponent<Evade>();
        if (obstacleBehavior == null) obstacleBehavior = gameObject.AddComponent<ObstacleAvoidance>();

        // Configurar rigidbody si es necesario
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        // Verificar puntos de ruta
        if (startPoint == null || endPoint == null)
            Debug.LogError("Puntos de inicio y final no asignados en EnemySteering");

        // Posicionar al enemigo en el punto inicial
        if (startPoint != null)
            transform.position = startPoint.position;

        lastTargetPosition = Vector3.zero;
    }

    void ConfigureRigidbody()
    {
        // Configuración común para Rigidbody de personajes
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Evita que rote físicamente
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        Debug.Log("EnemySteering: FixedUpdate ejecutándose");
        currentVelocity = rb.linearVelocity;
    }

    // Método para seguir la ruta
    public void FollowPath()
    {
        Vector3 target;

        if (!goingForward) // Escape
        {
            target = startPoint.position;
            currentTargetPosition = target;
            currentMaxSpeed = controller.runSpeed;

            // Combinar comportamientos (con evasión de obstáculos)
            Vector3 seekForce = seekBehavior.CalculateSteering(this);
            Vector3 avoidForce = obstacleBehavior.CalculateSteering(this);

            Vector3 combinedForce = seekForce + avoidForce * obstacleAvoidanceWeight;
            ApplySteering(combinedForce, controller.runSpeed);
        }
        else if (!reachedEndPoint) // Ida (A → B)
        {
            if (currentWaypointIndex < waypoints.Length)
                target = waypoints[currentWaypointIndex].position;
            else
                target = endPoint.position;

            // Si llegó al punto B
            if (HasReachedDestination(endPoint.position))
            {
                reachedEndPoint = true;
                currentWaypointIndex = waypoints.Length - 1;
                waypointDirection = -1;
                return;
            }

            currentTargetPosition = target;
            currentMaxSpeed = controller.walkSpeed;

            // Agregar evasión de obstáculos también en la fase de patrulla
            Vector3 seekForce = seekBehavior.CalculateSteering(this);
            Vector3 avoidForce = obstacleBehavior.CalculateSteering(this);

            // Usar un peso posiblemente menor para la evasión durante patrulla
            Vector3 combinedForce = seekForce + avoidForce * (obstacleAvoidanceWeight * 0.7f);
            ApplySteering(combinedForce, controller.walkSpeed);
        }
        else // Vuelta (B → A)
        {
            if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Length)
                target = waypoints[currentWaypointIndex].position;
            else
                target = startPoint.position;

            // Si llegó al punto A
            if (HasReachedDestination(startPoint.position))
            {
                reachedEndPoint = false;
                currentWaypointIndex = 0;
                waypointDirection = 1;
                return;
            }

            currentTargetPosition = target;
            currentMaxSpeed = controller.walkSpeed;

            // Agregar evasión de obstáculos también en la fase de regreso
            Vector3 seekForce = seekBehavior.CalculateSteering(this);
            Vector3 avoidForce = obstacleBehavior.CalculateSteering(this);

            // Usar un peso posiblemente menor para la evasión durante el regreso
            Vector3 combinedForce = seekForce + avoidForce * (obstacleAvoidanceWeight * 0.7f);
            ApplySteering(combinedForce, controller.walkSpeed);
        }
    }

    // Método para huir
    public void ReturnToStart()
    {
        goingForward = false;

        // Usar comportamientos para huir y evitar obstáculos
        Vector3 fleeForce = evadeBehavior.CalculateSteering(this);
        Vector3 seekForce = seekBehavior.CalculateSteering(this);
        Vector3 avoidForce = obstacleBehavior.CalculateSteering(this);

        // Combinar fuerzas (60% seek al punto A, 20% evade del jugador, 20% obstacle avoidance)
        Vector3 combinedForce = (seekForce * 0.6f) + (fleeForce * 0.2f) + (avoidForce * 0.2f);

        ApplySteering(combinedForce, controller.runSpeed);
    }

    // Verificar si se ha llegado a un punto específico
    private bool HasReachedDestination(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        return distance <= arrivalRadius;
    }

    // Verificar si se ha llegado al destino actual
    public bool HasReachedCurrentTarget()
    {
        Vector3 currentTarget;

        if (!goingForward)
        {
            currentTarget = startPoint.position;
        }
        else if (!reachedEndPoint)
        {
            if (currentWaypointIndex < waypoints.Length)
                currentTarget = waypoints[currentWaypointIndex].position;
            else
                currentTarget = endPoint.position;
        }
        else
        {
            if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Length)
                currentTarget = waypoints[currentWaypointIndex].position;
            else
                currentTarget = startPoint.position;
        }

        return HasReachedDestination(currentTarget);
    }

    // Avanzar al siguiente punto de la ruta
    public void MoveToNextTarget()
    {
        currentWaypointIndex += waypointDirection;

        // Si está retrocediendo y llega al inicio de los waypoints
        if (reachedEndPoint && currentWaypointIndex < 0)
        {
            currentWaypointIndex = -1; // Indicar que debe ir al punto A
        }
        // Si está avanzando y llega al final de los waypoints
        else if (!reachedEndPoint && currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = waypoints.Length; // Indicar que debe ir al punto B
        }
    }

    // Reiniciar al estado inicial
    public void ResetToStart()
    {
        goingForward = true;
        reachedEndPoint = false;
        currentWaypointIndex = 0;
        waypointDirection = 1;
    }

    // Aplicar el steering resultante al Rigidbody
    void ApplySteering(Vector3 steering, float maxSpeed)
    {
        // Limitar la fuerza máxima
        steering = Vector3.ClampMagnitude(steering, maxSteeringForce);

        // Aplicar fuerza
        rb.AddForce(steering, ForceMode.Acceleration);

        // Limitar velocidad máxima
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        // Orientar al enemigo en la dirección del movimiento
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Vector3 lookDirection = rb.linearVelocity;
            lookDirection.y = 0;

            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    controller.rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    // Métodos para visualización de gizmos
    void OnDrawGizmosSelected()
    {
        // Dibujar ruta
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
        }

        // Dibujar radio de detección de obstáculos
        Gizmos.color = new Color(1, 0.5f, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, obstacleDetectionRadius);
    }
}
