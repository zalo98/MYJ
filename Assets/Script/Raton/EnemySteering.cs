using UnityEngine;

public class EnemySteering : MonoBehaviour
{
    // Referencias
    [HideInInspector] public EnemyController controller;
    private Rigidbody rb;
    private WaypointSystem waypointSystem;

    // Comportamientos de steering (cambiados a la nueva interfaz)
    private ISteering seekBehavior;
    private ISteering fleeBehavior;
    private ObstacleAvoidance obstacleAvoidance;

    // Transform temporal para el comportamiento Seek
    private Transform targetTransform;

    // Propiedades para los comportamientos
    [HideInInspector] public Vector3 currentVelocity;
    [HideInInspector] public Vector3 currentTargetPosition;
    [HideInInspector] public float currentMaxSpeed;

    [Header("Steering Parameters")]
    public float maxSteeringForce = 10f;

    [Header("Obstacle Avoidance")]
    public float obstacleAvoidanceWeight = 1.5f;

    // Estado de movimiento
    private bool escaping = false;

    public void Initialize()
    {
        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody>();
        waypointSystem = GetComponent<WaypointSystem>();
        obstacleAvoidance = GetComponent<ObstacleAvoidance>();

        // Crear transform objetivo temporal para Seek
        GameObject targetObj = new GameObject("TargetPoint");
        targetTransform = targetObj.transform;
        targetObj.transform.parent = transform;

        // Inicializar nuevos comportamientos de steering
        seekBehavior = new Seek(rb, targetTransform, controller.walkSpeed);
        fleeBehavior = new Flee(rb, controller.PlayerTransform, controller.runSpeed);

        // Verificar que exista ObstacleAvoidance
        if (obstacleAvoidance == null)
            obstacleAvoidance = gameObject.AddComponent<ObstacleAvoidance>();

        // Configurar rigidbody si es necesario
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        // Verificar que exista WaypointSystem
        if (waypointSystem == null)
            Debug.LogError("No se encontr� componente WaypointSystem");
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

    // M�todo para seguir la ruta
    public void FollowPath()
    {
        if (escaping)
        {
            // Obtener el siguiente punto de la ruta de escape
            Vector3 target = waypointSystem.GetEscapeTargetPosition();
            currentTargetPosition = target;
            currentMaxSpeed = controller.runSpeed;

            // Actualizar posici�n del objetivo para Seek
            targetTransform.position = target;

            // Usar fleeBehavior para alejarse del jugador, pero dirigirse hacia el punto de escape
            Vector3 steeringForce = seekBehavior.MoveDirection();

            // Obtener fuerza de evasi�n
            Vector3 avoidForce = obstacleAvoidance.Avoid();

            // Combinar fuerzas
            Vector3 combinedForce = steeringForce + avoidForce * obstacleAvoidanceWeight;

            ApplySteering(combinedForce, controller.runSpeed);

            // Verificar si ha llegado al punto de destino de ESCAPE actual
            if (waypointSystem.HasReachedEscapeTarget(transform.position))
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

            // Actualizar posici�n del objetivo para Seek
            targetTransform.position = target;

            // Usar seekBehavior para seguir el camino
            Vector3 steeringForce = seekBehavior.MoveDirection();

            // Obtener fuerza de evasi�n
            Vector3 avoidForce = obstacleAvoidance.Avoid();

            // Combinar fuerzas
            Vector3 combinedForce = steeringForce + avoidForce * obstacleAvoidanceWeight;

            ApplySteering(combinedForce, controller.walkSpeed);

            // Verificar si ha llegado al punto de destino actual
            if (waypointSystem.HasReachedCurrentTarget(transform.position))
            {
                waypointSystem.MoveToNextTarget();
            }
        }
    }

    // M�todo para huir
    public void ReturnToStart()
    {
        if (!escaping)
        {
            escaping = true;
            waypointSystem.StartEscapeRoute();
        }

        // El resto de la l�gica ya est� en FollowPath()
        FollowPath();
    }

    // Aplicar el steering resultante al Rigidbody
    void ApplySteering(Vector3 steering, float maxSpeed)
    {
        // Limitar la fuerza m�xima
        steering = Vector3.ClampMagnitude(steering, maxSteeringForce);

        // Aplicar fuerza
        rb.AddForce(steering, ForceMode.Acceleration);

        // Limitar velocidad m�xima
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        // Orientar al enemigo en la direcci�n del movimiento
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

    // Para obstacleDetectionRadius
    public float obstacleDetectionRadius
    {
        get { return obstacleAvoidance ? obstacleAvoidance.detectionRange : 3f; }
    }
}
