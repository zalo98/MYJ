using UnityEngine;

public class EnemySteering : MonoBehaviour
{
    [HideInInspector] public EnemyController controller;
    private Rigidbody rb;
    public MouseMovement mouseMovement;
    private EnemyVision enemyVision;

    public Seek seekBehavior;
    private Flee fleeBehavior;
    private ObstacleAvoidance obstacleAvoidance;
    private Transform targetTransform;

    [HideInInspector] public Vector3 currentVelocity;
    [HideInInspector] public Vector3 currentTargetPosition;
    [HideInInspector] public float currentMaxSpeed;

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
        mouseMovement = GetComponent<MouseMovement>();
        obstacleAvoidance = GetComponent<ObstacleAvoidance>();
        enemyVision = GetComponent<EnemyVision>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        if (obstacleAvoidance == null)
            obstacleAvoidance = gameObject.AddComponent<ObstacleAvoidance>();

        if (mouseMovement == null)
            Debug.LogError("No se encontró componente MouseMovement");

        GameObject targetObj = new GameObject("TargetPoint");
        targetTransform = targetObj.transform;
        targetTransform.parent = transform;

        seekBehavior = new Seek(rb, targetTransform, controller.walkSpeed);
        fleeBehavior = new Flee(rb, controller.PlayerTransform, controller.runSpeed);
    }

    private void ConfigureRigidbody()
    {
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        currentVelocity = rb.linearVelocity;

        if (movingToPosition)
        {
            UpdateMoveToPosition();
        }

        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity);
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, 5f * Time.deltaTime);
            rb.MoveRotation(smoothedRotation);
        }
    }

    public void FollowPath()
    {
        // Actualizar detección de enemigos
        if (enemyVision != null)
            enemyVision.UpdateDetection();

        // PRIMERO: Verificar si detecta al player antes de cualquier movimiento
        if (!escaping && IsPlayerDetected())
        {
            Debug.Log("♟️ Player detectado! Iniciando escape táctico hacia punto A");
            StartEscapeMode();
        }

        // Actualizar el path si es necesario
        mouseMovement.UpdatePath();

        if (escaping)
        {
            // MODO ESCAPE TÁCTICO - Usando pathfinding que evita player pero va hacia A
            Vector3 target = mouseMovement.GetCurrentTargetPosition();
            currentTargetPosition = target;
            currentMaxSpeed = controller.runSpeed;

            // 1. Dirección principal hacia el nodo táctico calculado
            Vector3 dirToTarget = (target - transform.position).normalized * controller.runSpeed;

            // 2. Evasión de obstáculos físicos (paredes, etc)
            Vector3 obstacleAvoidForce = obstacleAvoidance.Avoid();

            // 3. NO usar AvoidPlayer aquí - el pathfinding táctico ya evita al player
            // El flee está "integrado" en la selección de nodos del pathfinding

            // 4. Combinar fuerzas - priorizar el pathfinding táctico
            Vector3 combinedForce;

            if (obstacleAvoidForce.magnitude > 0.1f)
            {
                // Si hay obstáculo físico inmediato, evitarlo pero mantener dirección general
                combinedForce = (dirToTarget * 1.8f) + (obstacleAvoidForce * 2.2f);
            }
            else
            {
                // Camino libre - seguir el pathfinding táctico puro
                combinedForce = dirToTarget * 2f;
            }

            ApplySteering(combinedForce, controller.runSpeed);

            // Debug visual del pathfinding táctico
            Debug.DrawRay(transform.position, dirToTarget.normalized * 3f, Color.cyan, 0.1f);
            Debug.DrawRay(transform.position, obstacleAvoidForce.normalized * 2f, Color.red, 0.1f);

            // Verificar progreso del escape táctico
            if (mouseMovement.HasReachedCurrentTarget(transform.position))
            {
                mouseMovement.MoveToNextTarget();

                // Si llegó al punto A, verificar si terminar escape
                if (Vector3.Distance(transform.position, mouseMovement.startPoint.position) <= mouseMovement.arrivalRadius)
                {
                    if (!IsPlayerDetected() || IsInSafeZone())
                    {
                        CompleteEscape();
                    }
                    else
                    {
                        // Quedarse en punto A hasta que sea seguro
                        Debug.Log("🏠 En punto A pero player aún visible - esperando...");
                    }
                }
            }

            // Verificar si el player se fue y está en zona segura
            if (!IsPlayerDetected() && IsInSafeZone())
            {
                Debug.Log("✅ Player ya no detectado y en zona segura - terminando escape");
                CompleteEscape();
            }
        }
        else
        {
            // MODO PATRULLAJE NORMAL (sin cambios)
            Vector3 target = mouseMovement.GetCurrentTargetPosition();
            currentTargetPosition = target;
            currentMaxSpeed = controller.walkSpeed;

            Vector3 dirToTarget = (target - transform.position).normalized * controller.walkSpeed;
            Vector3 avoidForce = obstacleAvoidance.Avoid();
            Vector3 combinedForce = dirToTarget + avoidForce * obstacleAvoidanceWeight;

            ApplySteering(combinedForce, controller.walkSpeed);

            if (mouseMovement.HasReachedCurrentTarget(transform.position))
            {
                mouseMovement.MoveToNextTarget();
            }
        }
    }

    bool IsPlayerDetected()
    {
        if (enemyVision == null) return false;
        return enemyVision.HasDirectDetection || enemyVision.HasPeripheralDetection;
    }

    bool IsInSafeZone()
    {
        float safeDistance = 4f; // Zona segura alrededor del punto A
        return Vector3.Distance(transform.position, mouseMovement.startPoint.position) <= safeDistance;
    }

    public void CompleteEscape()
    {
        escaping = false;
        mouseMovement.ResetToStart();

        Debug.Log("🏠 Escape completado - resumiendo patrullaje normal");

        // Opcional: Cambiar animación
        var animController = GetComponent<EnemyAnimationController>();
        if (animController != null)
            animController.SetRunning(false);
    }

    public void StartEscapeMode()
    {
        if (escaping) return; // Ya está escapando

        escaping = true;
        mouseMovement.StartEscape();

        Debug.Log("🏃‍♂️ ESCAPE TÁCTICO INICIADO - Dirigiéndose al punto A");

        // Opcional: Cambiar animación
        var animController = GetComponent<EnemyAnimationController>();
        if (animController != null)
            animController.SetRunning(true);
    }

    private void ApplySteering(Vector3 force, float maxSpeed)
    {
        // Limitar la fuerza máxima
        force = Vector3.ClampMagnitude(force, maxSteeringForce);

        // Aplicar fuerza
        rb.AddForce(force, ForceMode.Acceleration);

        // Limitar velocidad máxima
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    public void ReturnToStart()
    {
        if (!escaping)
        {
            escaping = true;
            mouseMovement.StartEscape();
        }

        // El resto de la lógica ya está en FollowPath()
        FollowPath();
    }

    public Flee FleeBehavior => fleeBehavior;

    public void MoveToPosition(Vector3 target, float speed)
    {
        currentTargetPosition = target;
        currentMaxSpeed = speed;
        targetTransform.position = target;
        
        Vector3 moveDirection = seekBehavior.MoveDirection();
        moveDirection.y = 0f;
        Vector3 avoidance = obstacleAvoidance.Avoid();
        
        if (avoidance.sqrMagnitude > 0.1f)
        {
            rb.AddForce(avoidance * speed, ForceMode.Acceleration);
        }
        
        rb.AddForce(moveDirection.normalized * speed, ForceMode.Acceleration);
        
        if (rb.linearVelocity.magnitude > speed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }
    
    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothedRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, 360f * Time.deltaTime);
        rb.MoveRotation(smoothedRotation);
    }

    private void UpdateMoveToPosition()
    {
        Vector3 steering = seekBehavior.MoveDirection();
        Vector3 avoid = obstacleAvoidance.Avoid();
        Vector3 combined = steering + avoid * obstacleAvoidanceWeight;

        ApplySteering(combined, currentMaxSpeed);

        if (Vector3.Distance(transform.position, currentTargetPosition) < 1f)
        {
            movingToPosition = false;
        }
    }
}