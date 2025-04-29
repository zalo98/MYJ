using UnityEngine;

public class EnemySteering : MonoBehaviour
{
    [HideInInspector] public EnemyController controller;
    private Rigidbody rb;
    private WaypointSystem waypointSystem;

    private ISteering seekBehavior;
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

        GameObject targetObj = new GameObject("TargetPoint");
        targetTransform = targetObj.transform;
        targetObj.transform.parent = transform;

        seekBehavior = new Seek(rb, targetTransform, controller.walkSpeed);
        fleeBehavior = new Flee(rb, controller.PlayerTransform, controller.runSpeed);

        if (obstacleAvoidance == null)
            obstacleAvoidance = gameObject.AddComponent<ObstacleAvoidance>();

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
            targetTransform.position = target;

            Vector3 steeringForce = seekBehavior.MoveDirection();
            Vector3 avoidForce = obstacleAvoidance.Avoid();
            Vector3 combinedForce = steeringForce + avoidForce * obstacleAvoidanceWeight;

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
            targetTransform.position = target;

            Vector3 steeringForce = seekBehavior.MoveDirection();
            Vector3 avoidForce = obstacleAvoidance.Avoid();
            Vector3 combinedForce = steeringForce + avoidForce * obstacleAvoidanceWeight;

            ApplySteering(combinedForce, controller.walkSpeed);
        }
    }

    private void ApplySteering(Vector3 steeringForce, float maxSpeed)
    {
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxSteeringForce);

        rb.AddForce(steeringForce);
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