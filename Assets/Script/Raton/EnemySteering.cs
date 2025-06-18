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
        if (enemyVision != null)
            enemyVision.UpdateDetection();

        if (!escaping && IsPlayerDetected())
        {
            Debug.Log("♟️ Player detectado! Iniciando escape táctico hacia punto A");
            StartEscapeMode();
        }

        mouseMovement.UpdatePath();

        if (escaping)
        {
            Vector3 target = mouseMovement.GetCurrentTargetPosition();
            currentTargetPosition = target;
            currentMaxSpeed = controller.runSpeed;
            
            Vector3 dirToTarget = (target - transform.position).normalized * controller.runSpeed;
            
            Vector3 obstacleAvoidForce = obstacleAvoidance.Avoid();
            
            Vector3 combinedForce;

            if (obstacleAvoidForce.magnitude > 0.1f)
            {
                combinedForce = (dirToTarget * 1.8f) + (obstacleAvoidForce * 2.2f);
            }
            else
            {
                combinedForce = dirToTarget * 2f;
            }

            ApplySteering(combinedForce, controller.runSpeed);
            
            Debug.DrawRay(transform.position, dirToTarget.normalized * 3f, Color.cyan, 0.1f);
            Debug.DrawRay(transform.position, obstacleAvoidForce.normalized * 2f, Color.red, 0.1f);
            
            if (mouseMovement.HasReachedCurrentTarget(transform.position))
            {
                mouseMovement.MoveToNextTarget();
                
                if (Vector3.Distance(transform.position, mouseMovement.startPoint.position) <= mouseMovement.arrivalRadius)
                {
                    if (!IsPlayerDetected() || IsInSafeZone())
                    {
                        CompleteEscape();
                    }
                    else
                    {
                        Debug.Log("🏠 En punto A pero player aún visible - esperando...");
                    }
                }
            }
            
            if (!IsPlayerDetected() && IsInSafeZone())
            {
                Debug.Log("✅ Player ya no detectado y en zona segura - terminando escape");
                CompleteEscape();
            }
        }
        else
        {
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
        float safeDistance = 4f;
        return Vector3.Distance(transform.position, mouseMovement.startPoint.position) <= safeDistance;
    }

    public void CompleteEscape()
    {
        escaping = false;
        mouseMovement.ResetToStart();

        Debug.Log("🏠 Escape completado - resumiendo patrullaje normal");
        
        var animController = GetComponent<EnemyAnimationController>();
        if (animController != null)
            animController.SetRunning(false);
    }

    public void StartEscapeMode()
    {
        if (escaping) return;

        escaping = true;
        mouseMovement.StartEscape();

        Debug.Log("🏃‍♂️ ESCAPE TÁCTICO INICIADO - Dirigiéndose al punto A");
        
        var animController = GetComponent<EnemyAnimationController>();
        if (animController != null)
            animController.SetRunning(true);
    }

    private void ApplySteering(Vector3 force, float maxSpeed)
    {
        force = Vector3.ClampMagnitude(force, maxSteeringForce);
        
        rb.AddForce(force, ForceMode.Acceleration);
        
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