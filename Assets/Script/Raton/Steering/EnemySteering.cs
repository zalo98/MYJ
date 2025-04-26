using UnityEngine;
using UnityEngine.AI;

public class EnemySteering : MonoBehaviour
{
    // Referencias
    [HideInInspector] public EnemyController controller;
    private Rigidbody rb;
    private WaypointSystem waypointSystem;

    // Comportamientos de steering
    private Seek seekBehavior;
    private Flee fleeBehavior;
    private Evade evadeBehavior;
    private ObstacleAvoidance obstacleAvoidance;

    // Propiedades para los comportamientos
    [HideInInspector] public Vector3 currentVelocity;
    [HideInInspector] public Vector3 currentTargetPosition;
    [HideInInspector] public float currentMaxSpeed;

    [HideInInspector] public Vector3 lastTargetPosition;

    [Header("Steering Parameters")]
    public float maxSteeringForce = 10f;

    [Header("Obstacle Avoidance")]
    public float obstacleAvoidanceWeight = 1.5f;

    // Estado de movimiento
    private bool escaping = false;

    public void Initialize()
    {
        Debug.Log("EnemySteering: Inicializando");

        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody>();
        waypointSystem = GetComponent<WaypointSystem>();

        // Obtener comportamientos de steering
        seekBehavior = GetComponent<Seek>();
        fleeBehavior = GetComponent<Flee>();
        evadeBehavior = GetComponent<Evade>();
        obstacleAvoidance = GetComponent<ObstacleAvoidance>();

        if (seekBehavior == null) seekBehavior = gameObject.AddComponent<Seek>();
        if (fleeBehavior == null) fleeBehavior = gameObject.AddComponent<Flee>();
        if (evadeBehavior == null) evadeBehavior = gameObject.AddComponent<Evade>();
        if (obstacleAvoidance == null) obstacleAvoidance = gameObject.AddComponent<ObstacleAvoidance>();

        lastTargetPosition = Vector3.zero;

        // Configurar rigidbody si es necesario
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        // Verificar que exista WaypointSystem
        if (waypointSystem == null)
            Debug.LogError("No se encontró componente WaypointSystem");
    }

    void ConfigureRigidbody()
    {
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        currentVelocity = rb.linearVelocity;
    }

    // Método para seguir la ruta
    public void FollowPath()
    {
        if (escaping)
        {
            // Obtener el siguiente punto de la ruta de escape
            Vector3 target = waypointSystem.GetEscapeTargetPosition();
            currentTargetPosition = target;
            currentMaxSpeed = controller.runSpeed;

            // Dirección base hacia el punto objetivo
            Vector3 dirToTarget = (target - transform.position).normalized * controller.runSpeed;

            // Obtener fuerza de evasión
            Vector3 avoidForce = obstacleAvoidance.Avoid();

            // Combinar fuerzas
            Vector3 combinedForce = dirToTarget + avoidForce * obstacleAvoidanceWeight;

            ApplySteering(combinedForce, controller.runSpeed);

            // Verificar si ha llegado al punto de destino actual
            if (waypointSystem.HasReachedCurrentTarget(transform.position))
            {
                // Avanzar al siguiente punto en la ruta de escape
                if (waypointSystem.MoveToNextEscapePoint())
                {
                    // Si hemos terminado de escapar (llegado al inicio)
                    escaping = false;
                    waypointSystem.ResetToStart();
                }
            }
        }
        else
        {
            // Obtener el siguiente punto de la ruta normal
            Vector3 target = waypointSystem.GetCurrentTargetPosition();
            currentTargetPosition = target;
            currentMaxSpeed = controller.walkSpeed;

            // Dirección base hacia el punto objetivo
            Vector3 dirToTarget = (target - transform.position).normalized * controller.walkSpeed;

            // Obtener fuerza de evasión
            Vector3 avoidForce = obstacleAvoidance.Avoid();

            // Combinar fuerzas
            Vector3 combinedForce = dirToTarget + avoidForce * obstacleAvoidanceWeight;

            ApplySteering(combinedForce, controller.walkSpeed);

            // Verificar si ha llegado al punto de destino actual
            if (waypointSystem.HasReachedCurrentTarget(transform.position))
            {
                waypointSystem.MoveToNextTarget();
            }
        }
    }

    // Método para huir
    public void ReturnToStart()
    {
        if (!escaping)
        {
            escaping = true;
            waypointSystem.StartEscapeRoute();
        }

        // El resto de la lógica ya está en FollowPath()
        FollowPath();
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

    public bool HasReachedCurrentTarget()
    {
        return waypointSystem.HasReachedCurrentTarget(transform.position);
    }

    public void MoveToNextTarget()
    {
        waypointSystem.MoveToNextTarget();
    }

    public void ResetToStart()
    {
        waypointSystem.ResetToStart();
    }

    // Para campos como obstacleDetectionRadius
    public float obstacleDetectionRadius
    {
        get { return obstacleAvoidance ? obstacleAvoidance.detectionRange : 3f; }
    }
}
