using UnityEngine;

public class EnemySteering : MonoBehaviour
{
    [HideInInspector] public EnemyController controller;
    private Rigidbody rb;
    private WaypointSystem waypointSystem;

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
        waypointSystem = GetComponent<WaypointSystem>();
        obstacleAvoidance = GetComponent<ObstacleAvoidance>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        if (obstacleAvoidance == null)
            obstacleAvoidance = gameObject.AddComponent<ObstacleAvoidance>();

        if (waypointSystem == null)
            Debug.LogError("No se encontró componente WaypointSystem");

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
        if (movingToPosition) return;

        Vector3 target;
        float speed;

        if (escaping)
        {
            target = waypointSystem.GetEscapeTargetPosition();
            speed = controller.runSpeed;

            if (waypointSystem.HasReachedEscapeTarget(transform.position) &&
                !waypointSystem.MoveToNextEscapePoint())
            {
                escaping = false;
                waypointSystem.ResetToStart();
            }
        }
        else
        {
            target = waypointSystem.GetCurrentTargetPosition();
            speed = controller.walkSpeed;
        }

        currentTargetPosition = target;
        currentMaxSpeed = speed;
        targetTransform.position = target;

        Vector3 steering = seekBehavior.MoveDirection();
        Vector3 avoid = obstacleAvoidance.Avoid();
        Vector3 combined = steering + avoid * obstacleAvoidanceWeight;

        ApplySteering(combined, speed);
    }

    private void ApplySteering(Vector3 force, float maxSpeed)
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    public void ReturnToStart()
    {
        if (waypointSystem != null)
        {
            waypointSystem.ResetToStart();
            escaping = false;
        }
        else
        {
            Debug.LogError("No se pudo encontrar WaypointSystem al intentar regresar al inicio.");
        }
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