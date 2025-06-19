using UnityEngine;
using System.Reflection;

public class FlockingPatrolState : State
{
    private EnemyController controller;
    private MouseBoid mouseBoid;
    private MouseMovement mouseMovement;
    private FlockingManager flockingManager;
    private float obstacleAvoidanceWeight = 1.5f;
    private PlayerController playerController;

    private float patrolDelay = 0.5f;
    private float timeSpentAtWaypoint = 0f;
    private int waypointCounter = 0;
    private int waypointLooking;
    private bool waypointsInitialized = false;

    private static bool wayPointReached = false;
    private static float groupWaypointTimer = 0f;
    private static int currentGroupWaypointIndex = 0;
    private static bool isReturning = false;
    private static object wayPointLock = new object();

    public FlockingPatrolState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsWalking", true);
        controller.EnemyAnimator.SetBool("IsRunning", false);

        timeSpentAtWaypoint = 0f;
        waypointCounter = 0;
        waypointLooking = Mathf.RoundToInt(Random.Range(3f, 5f));

        mouseMovement = controller.GetComponent<MouseMovement>();
        if (mouseMovement == null)
        {
            mouseMovement = controller.gameObject.AddComponent<MouseMovement>();
        }

        mouseBoid = controller.GetComponent<MouseBoid>();
        if (mouseBoid == null)
        {
            mouseBoid = controller.gameObject.AddComponent<MouseBoid>();
        }
        mouseBoid.SetMaxSpeed(controller.walkSpeed);

        if (playerController == null && controller.PlayerTransform != null)
        {
            playerController = controller.PlayerTransform.GetComponent<PlayerController>();
        }

        TryInitializeWaypoints();
        SyncToGroupWaypoint();
    }

    private bool TryInitializeWaypoints()
    {
        flockingManager = FlockingManager.Instance;
        if (flockingManager == null)
        {
            Debug.Log("FlockingManager not available yet, will retry later");
            return false;
        }

        if (flockingManager.startPoint != null && flockingManager.endPoint != null)
        {
            mouseMovement.startPoint = flockingManager.startPoint;
            mouseMovement.endPoint = flockingManager.endPoint;

            if (flockingManager.waypoints != null && flockingManager.waypoints.Length > 0)
            {
                int validCount = 0;
                foreach (var wp in flockingManager.waypoints)
                {
                    if (wp != null) validCount++;
                }

                Transform[] cleanWaypoints = new Transform[validCount];
                int index = 0;
                foreach (var wp in flockingManager.waypoints)
                {
                    if (wp != null)
                    {
                        cleanWaypoints[index] = wp;
                        index++;
                    }
                }
                mouseMovement.waypoints = cleanWaypoints;
            }
            else
            {
                mouseMovement.waypoints = new Transform[0];
            }

            if (flockingManager.GetType().GetField("waypointArrivalRadius") != null)
            {
                mouseMovement.arrivalRadius = flockingManager.waypointArrivalRadius;
            }
            else
            {
                mouseMovement.arrivalRadius = 1.5f;
            }

            if (mouseBoid != null)
            {
                flockingManager.AddBoid(mouseBoid);
            }

            waypointsInitialized = true;
            return true;
        }

        return false;
    }

    public override void Execute()
    {
        if (!waypointsInitialized)
        {
            if (!TryInitializeWaypoints())
            {
                CreateTemporaryWaypoints();
            }
        }

        if (controller.enemyVision.HasDirectDetection)
        {
            BlueMouse blueMouse = controller as BlueMouse;
            if (blueMouse != null)
            {
                if (playerController != null && playerController.isAttacking)
                {
                    BlueMouse.BroadcastToAllMice(
                        StateEnum.BlueEscapeState,
                        blueMouse,
                        controller.PlayerTransform.position
                    );
                    fsm.Transition(StateEnum.BlueEscapeState);
                }
                else
                {
                    BlueMouse.BroadcastToAllMice(
                        StateEnum.FlockingAttackState,
                        blueMouse,
                        controller.PlayerTransform.position
                    );
                    fsm.Transition(StateEnum.FlockingAttackState);
                }
                return;
            }
        }

        try
        {
            if (mouseMovement.HasReachedCurrentTarget(controller.transform.position) && !wayPointReached)
            {
                lock (wayPointLock)
                {
                    if (!wayPointReached)
                    {
                        wayPointReached = true;
                        groupWaypointTimer = 0f;

                        Debug.Log($"Boid {controller.gameObject.name} reached waypoint - notifying flock");
                    }
                }
            }

            if (wayPointReached)
            {
                groupWaypointTimer += Time.deltaTime;

                if (groupWaypointTimer > patrolDelay)
                {
                    lock (wayPointLock)
                    {
                        if (!isReturning)
                        {
                            currentGroupWaypointIndex++;
                            if (currentGroupWaypointIndex >= mouseMovement.waypoints.Length)
                            {
                                isReturning = true;
                                currentGroupWaypointIndex = mouseMovement.waypoints.Length - 1;
                            }
                        }
                        else
                        {
                            currentGroupWaypointIndex--;
                            if (currentGroupWaypointIndex < 0)
                            {
                                isReturning = false;
                                currentGroupWaypointIndex = 0;
                            }
                        }

                        NotifyAllBoidsOfWaypointChange();

                        wayPointReached = false;
                        groupWaypointTimer = 0f;
                        waypointCounter++;

                        if (waypointCounter >= waypointLooking)
                        {
                            waypointCounter = 0;
                            waypointLooking = Mathf.RoundToInt(Random.Range(3f, 5f));
                        }
                    }
                }
            }

            Vector3 targetPosition;
            try
            {
                targetPosition = mouseMovement.GetCurrentTargetPosition();
            }
            catch (System.NullReferenceException)
            {
                targetPosition = controller.transform.position + controller.transform.forward * 5f;
            }

            Vector3 avoidForce = controller.obstacleAvoidance.Avoid() * obstacleAvoidanceWeight;
            Vector3 pathDirection = (targetPosition - controller.transform.position).normalized;

            float pathWeight = flockingManager != null ? flockingManager.pathfindingWeight : 0.05f;
            Vector3 pathForce = pathDirection * mouseBoid._maxSpeed * pathWeight;
            Vector3 combinedForce = pathForce + avoidForce;

            mouseBoid.SetPathFollowingForce(combinedForce);
            mouseBoid.ApplyFlocking();
            mouseBoid.UpdateMovement();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception in FlockingPatrolState.Execute: " + e.Message);
        }
    }

    private void NotifyAllBoidsOfWaypointChange()
    {
        if (flockingManager == null || flockingManager.AllBoids == null) return;

        foreach (MouseBoid boid in flockingManager.AllBoids)
        {
            if (boid == null) continue;

            MouseMovement boidMovement = boid.GetComponent<MouseMovement>();
            if (boidMovement != null)
            {
                SetBoidWaypointIndex(boidMovement, currentGroupWaypointIndex, isReturning);
            }
        }
    }

    private void SyncToGroupWaypoint()
    {
        if (mouseMovement != null)
        {
            SetBoidWaypointIndex(mouseMovement, currentGroupWaypointIndex, isReturning);
        }
    }

    private void SetBoidWaypointIndex(MouseMovement movement, int waypointIndex, bool returning)
    {
        FieldInfo indexField = typeof(MouseMovement).GetField("currentWaypointIndex", 
            BindingFlags.Instance | BindingFlags.NonPublic);
        
        FieldInfo returningField = typeof(MouseMovement).GetField("reachedEndPoint", 
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (indexField != null && returningField != null)
        {
            indexField.SetValue(movement, waypointIndex);
            returningField.SetValue(movement, returning);
        }
        else
        {
            Debug.LogError("Failed to access private fields via reflection");
        }
    }

    private void CreateTemporaryWaypoints()
    {
        if (mouseMovement.startPoint == null)
        {
            GameObject startObj = new GameObject("TempStartPoint");
            startObj.transform.position = controller.transform.position + Vector3.left * 5f;
            mouseMovement.startPoint = startObj.transform;
        }

        if (mouseMovement.endPoint == null)
        {
            GameObject endObj = new GameObject("TempEndPoint");
            endObj.transform.position = controller.transform.position + Vector3.right * 5f;
            mouseMovement.endPoint = endObj.transform;
        }

        if (mouseMovement.waypoints == null || mouseMovement.waypoints.Length == 0)
        {
            mouseMovement.waypoints = new Transform[0];
        }

        if (mouseMovement.arrivalRadius <= 0)
        {
            mouseMovement.arrivalRadius = 1.5f;
        }
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsWalking", false);
    }
}