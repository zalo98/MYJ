using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{
    public static FlockingManager Instance { get; private set; }

    [Header("Pathfinding Configuration")]
    public Transform startPoint;
    public Transform endPoint;
    public Transform[] waypoints;
    public float waypointArrivalRadius = 1.5f;

    [Header("Flocking vs Pathfinding Balance")]
    [Range(0f, 0.1f)] 
    public float pathfindingWeight = 0.5f;

    [Header("Flocking Weights")]
    [SerializeField, Range(0f, 3f)] public float separationWeight = 1.5f;
    [SerializeField, Range(0f, 1f)] public float cohesionWeight = 0.5f;

    public List<MouseBoid> AllBoids { get; private set; }

    private int currentWaypointIndex = 0;
    private bool reachedEndPoint = false;
    private int waypointDirection = 1;
    private Vector3 currentTargetPosition;

    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        AllBoids = new List<MouseBoid>();

        if (waypoints.Length > 0)
            currentTargetPosition = waypoints[0].position;
        else
            currentTargetPosition = endPoint.position;

        reachedEndPoint = false;
        currentWaypointIndex = 0;
        waypointDirection = 1;
    }

    public void AddBoid(MouseBoid b)
    {
        if (!AllBoids.Contains(b))
            AllBoids.Add(b);
    }

    public Vector3 GetCurrentTargetPosition()
    {
        return currentTargetPosition;
    }

    public float GetAlignmentWeight()
    {
        return 1f - pathfindingWeight;
    }

    public float GetSeparationWeight()
    {
        return separationWeight;
    }

    public float GetCohesionWeight()
    {
        return cohesionWeight;
    }

    public void CheckWaypointArrival()
    {
        bool anyBoidReachedTarget = false;

        foreach (var boid in AllBoids)
        {
            if (Vector3.Distance(boid.transform.position, currentTargetPosition) <= waypointArrivalRadius)
            {
                anyBoidReachedTarget = true;
                break;
            }
        }

        if (anyBoidReachedTarget)
        {
            if (Vector3.Distance(currentTargetPosition, endPoint.position) < 0.1f)
            {
                reachedEndPoint = true;
                waypointDirection = -1;

                if (waypoints.Length > 0)
                {
                    currentWaypointIndex = waypoints.Length - 1;
                    currentTargetPosition = waypoints[currentWaypointIndex].position;
                }
                else
                {
                    currentTargetPosition = startPoint.position;
                }

                Debug.Log("⚠️ Reached endpoint - now returning to start");
            }
            else if (Vector3.Distance(currentTargetPosition, startPoint.position) < 0.1f)
            {
                reachedEndPoint = false;
                waypointDirection = 1;

                if (waypoints.Length > 0)
                {
                    currentWaypointIndex = 0;
                    currentTargetPosition = waypoints[0].position;
                }
                else
                {
                    currentTargetPosition = endPoint.position;
                }

                Debug.Log("⚠️ Reached startpoint - now heading to end");
            }
            else
            {
                MoveToNextWaypoint();
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        if (!reachedEndPoint)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex < waypoints.Length)
            {
                currentTargetPosition = waypoints[currentWaypointIndex].position;
            }
            else
            {
                currentTargetPosition = endPoint.position;
            }
        }
        else
        {
            currentWaypointIndex--;

            if (currentWaypointIndex >= 0)
            {
                currentTargetPosition = waypoints[currentWaypointIndex].position;
            }
            else
            {
                currentTargetPosition = startPoint.position;
            }
        }
    }

    public Vector3 GetPathFollowingForce(Vector3 boidPosition, float maxSpeed)
    {
        Vector3 dirToTarget = (currentTargetPosition - boidPosition).normalized;
        return dirToTarget * maxSpeed * pathfindingWeight;
    }

    private void Update()
    {
        CheckWaypointArrival();
    }
    
    public void BroadcastStateTransition(StateEnum targetState, BlueMouse initiator = null)
    {
        foreach (var boid in AllBoids)
        {
            BlueMouse blueMouse = boid.GetComponent<BlueMouse>();
            if (blueMouse != null && blueMouse != initiator)
            {
                blueMouse.TransitionToState(targetState);
            }
        }
    }

    public void AlertAllBoids(Vector3 detectionPosition)
    {
        foreach (var boid in AllBoids)
        {
            BlueMouse blueMouse = boid.GetComponent<BlueMouse>();
            if (blueMouse != null)
            {
                blueMouse.SetLastSeenPosition(detectionPosition);
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentTargetPosition, waypointArrivalRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.3f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.3f);
        }

        if (startPoint && endPoint && waypoints.Length > 0)
        {
            Gizmos.color = Color.white;

            if (waypoints[0])
                Gizmos.DrawLine(startPoint.position, waypoints[0].position);

            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] && waypoints[i+1])
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
            }

            if (waypoints[waypoints.Length-1])
                Gizmos.DrawLine(waypoints[waypoints.Length-1].position, endPoint.position);
        }
    }
}