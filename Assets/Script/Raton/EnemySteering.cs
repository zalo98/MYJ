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

            // Actualizar posición del objetivo para Seek
            targetTransform.position = target;

            // Usar fleeBehavior para alejarse del jugador, pero dirigirse hacia el punto de escape
            Vector3 steeringForce = seekBehavior.MoveDirection();

            // Obtener fuerza de evasión
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

            // Actualizar posición del objetivo para Seek
            targetTransform.position = target;

            // Usar seekBehavior para seguir el camino
            Vector3 steeringForce = seekBehavior.MoveDirection();

            // Obtener fuerza de evasión
            Vector3 avoidForce = obstacleAvoidance.Avoid();

            // Combinar fuerzas
            Vector3 combinedForce = steeringForce + avoidForce * obstacleAvoidanceWeight;

            ApplySteering(combinedForce, controller.walkSpeed);
        }
    }

    // Aplicar el steering calculado
    private void ApplySteering(Vector3 steeringForce, float maxSpeed)
    {
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxSteeringForce);

        rb.AddForce(steeringForce);
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    // Método para regresar al punto inicial de la ruta
    public void ReturnToStart()
    {
        if (waypointSystem != null)
        {
            waypointSystem.ResetToStart();
            escaping = false;  // Deja de escapar y vuelve a patrullar
        }
        else
        {
            Debug.LogError("No se pudo encontrar WaypointSystem al intentar regresar al inicio.");
        }
    }
}