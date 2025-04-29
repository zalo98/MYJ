using UnityEngine;
using UnityEngine.AI;

public class EnemySteering : MonoBehaviour
{
    [HideInInspector] public EnemyController controller;
    private Rigidbody rb;
    private WaypointSystem waypointSystem;

<<<<<<< Updated upstream:Assets/Script/Raton/Steering/EnemySteering.cs
    // Comportamientos de steering
    private Seek seekBehavior;
    private Flee fleeBehavior;
    private Evade evadeBehavior;
    private ObstacleAvoidance obstacleAvoidance;

    // Propiedades para los comportamientos
=======
    private ISteering seekBehavior;
    private Flee fleeBehavior;
    private ObstacleAvoidance obstacleAvoidance;

    private Transform targetTransform;

>>>>>>> Stashed changes:Assets/Script/Raton/EnemySteering.cs
    [HideInInspector] public Vector3 currentVelocity;
    [HideInInspector] public Vector3 currentTargetPosition;
    [HideInInspector] public float currentMaxSpeed;

    [HideInInspector] public Vector3 lastTargetPosition;

    [Header("Steering Parameters")]
    public float maxSteeringForce = 10f;

    [Header("Obstacle Avoidance")]
    public float obstacleAvoidanceWeight = 1.5f;

    private bool escaping = false;
    private bool movingToPosition = false;

    public void Initialize()
    {
        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody>();
        waypointSystem = GetComponent<WaypointSystem>();

        // Obtener comportamientos de steering
        seekBehavior = GetComponent<Seek>();
        fleeBehavior = GetComponent<Flee>();
        evadeBehavior = GetComponent<Evade>();
        obstacleAvoidance = GetComponent<ObstacleAvoidance>();

<<<<<<< Updated upstream:Assets/Script/Raton/Steering/EnemySteering.cs
        if (seekBehavior == null) seekBehavior = gameObject.AddComponent<Seek>();
        if (fleeBehavior == null) fleeBehavior = gameObject.AddComponent<Flee>();
        if (evadeBehavior == null) evadeBehavior = gameObject.AddComponent<Evade>();
        if (obstacleAvoidance == null) obstacleAvoidance = gameObject.AddComponent<ObstacleAvoidance>();

        lastTargetPosition = Vector3.zero;
=======
        GameObject targetObj = new GameObject("TargetPoint");
        targetTransform = targetObj.transform;
        targetObj.transform.parent = transform;

        seekBehavior = new Seek(rb, targetTransform, controller.walkSpeed);
        fleeBehavior = new Flee(rb, controller.PlayerTransform, controller.runSpeed);

        if (obstacleAvoidance == null)
            obstacleAvoidance = gameObject.AddComponent<ObstacleAvoidance>();
>>>>>>> Stashed changes:Assets/Script/Raton/EnemySteering.cs

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            ConfigureRigidbody();
        }

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

        if (movingToPosition)
        {
            UpdateMoveToPosition();
        }
    }

    public void FollowPath()
    {
        if (movingToPosition) return;

        if (escaping)
        {
            Vector3 target = waypointSystem.GetEscapeTargetPosition();
            currentTargetPosition = target;
            currentMaxSpeed = controller.runSpeed;
<<<<<<< Updated upstream:Assets/Script/Raton/Steering/EnemySteering.cs

            // Dirección base hacia el punto objetivo
            Vector3 dirToTarget = (target - transform.position).normalized * controller.runSpeed;

            // Obtener fuerza de evasión
            Vector3 avoidForce = obstacleAvoidance.Avoid();

            // Combinar fuerzas
            Vector3 combinedForce = dirToTarget + avoidForce * obstacleAvoidanceWeight;
=======
            targetTransform.position = target;

            Vector3 steeringForce = seekBehavior.MoveDirection();
            Vector3 avoidForce = obstacleAvoidance.Avoid();
            Vector3 combinedForce = steeringForce + avoidForce * obstacleAvoidanceWeight;
>>>>>>> Stashed changes:Assets/Script/Raton/EnemySteering.cs

            ApplySteering(combinedForce, controller.runSpeed);

            if (waypointSystem.HasReachedEscapeTarget(transform.position))
            {
                if (waypointSystem.MoveToNextEscapePoint())
                {
                    escaping = false;
                    waypointSystem.ResetToStart();
                }
            }
        }
        else
        {
            Vector3 target = waypointSystem.GetCurrentTargetPosition();
            currentTargetPosition = target;
            currentMaxSpeed = controller.walkSpeed;
<<<<<<< Updated upstream:Assets/Script/Raton/Steering/EnemySteering.cs

            // Dirección base hacia el punto objetivo
            Vector3 dirToTarget = (target - transform.position).normalized * controller.walkSpeed;

            // Obtener fuerza de evasión
            Vector3 avoidForce = obstacleAvoidance.Avoid();

            // Combinar fuerzas
            Vector3 combinedForce = dirToTarget + avoidForce * obstacleAvoidanceWeight;
=======
            targetTransform.position = target;

            Vector3 steeringForce = seekBehavior.MoveDirection();
            Vector3 avoidForce = obstacleAvoidance.Avoid();
            Vector3 combinedForce = steeringForce + avoidForce * obstacleAvoidanceWeight;
>>>>>>> Stashed changes:Assets/Script/Raton/EnemySteering.cs

            ApplySteering(combinedForce, controller.walkSpeed);

            // Verificar si ha llegado al punto de destino actual
            if (waypointSystem.HasReachedCurrentTarget(transform.position))
            {
                waypointSystem.MoveToNextTarget();
            }
        }
    }

<<<<<<< Updated upstream:Assets/Script/Raton/Steering/EnemySteering.cs
    // Método para huir
=======
    private void ApplySteering(Vector3 steeringForce, float maxSpeed)
    {
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxSteeringForce);

        rb.AddForce(steeringForce);
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

>>>>>>> Stashed changes:Assets/Script/Raton/EnemySteering.cs
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

<<<<<<< Updated upstream:Assets/Script/Raton/Steering/EnemySteering.cs
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
=======
    public Flee FleeBehavior => fleeBehavior;
    
    public void MoveToPosition(Vector3 position, float speed)
    {
        movingToPosition = true;
        currentTargetPosition = position;
        currentMaxSpeed = speed;
        targetTransform.position = position;
    }
    
    private void UpdateMoveToPosition()
    {
        Vector3 steeringForce = seekBehavior.MoveDirection();
        Vector3 avoidForce = obstacleAvoidance.Avoid();
        Vector3 combinedForce = steeringForce + avoidForce * obstacleAvoidanceWeight;

        ApplySteering(combinedForce, currentMaxSpeed);
        
        if (Vector3.Distance(transform.position, currentTargetPosition) < 1f)
        {
            movingToPosition = false;
        }
    }
}
>>>>>>> Stashed changes:Assets/Script/Raton/EnemySteering.cs
