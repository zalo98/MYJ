using UnityEngine;

public class MouseBoid : SteeringBase
{
    [Header("Flocking Parameters")]
    [SerializeField] float separationRadius = 2f;
    [SerializeField] float viewRadius = 5f;
    [SerializeField] LayerMask boidsMask;
    
    private Vector3 pathFollowingForce;

    public void SetPathFollowingForce(Vector3 force)
    {
        pathFollowingForce = force;
    }

    public void ApplyFlocking()
    {
        if (FlockingManager.Instance == null) return;
        
        float separationWeight = FlockingManager.Instance.GetSeparationWeight();
        float cohesionWeight = FlockingManager.Instance.GetCohesionWeight();
        float alignmentWeight = FlockingManager.Instance.GetAlignmentWeight();

        Vector3 flockingForce = (Separation() * separationWeight) + (Cohesion() * cohesionWeight) + (Alignment() * alignmentWeight);
        Vector3 combinedForce = flockingForce + pathFollowingForce;

        AddForce(combinedForce);
    }
    
    public void UpdateMovement()
    {
        Move();
    }
    
    public LayerMask GetBoidsMask()
    {
        return boidsMask;
    }
    
    public void SetMaxSpeed(float speed)
    {
        _maxSpeed = speed;
    }

    private Vector3 Separation()
    {
        int count = 0;
        Vector3 dir = Vector3.zero;
        var boidsInRange = Physics.OverlapSphere(transform.position, separationRadius, boidsMask);

        foreach (var collider in boidsInRange)
        {
            MouseBoid boid = collider.GetComponent<MouseBoid>();
            if (boid == null || boid == this) continue;

            dir += (transform.position - boid.transform.position).normalized;
            count++;
        }

        if (count == 0) return dir;
        dir /= count;
        return CalculateSteering(dir);
    }

    private Vector3 Cohesion()
    {
        int count = 0;
        Vector3 centrePos = Vector3.zero;
        var boidsInRange = Physics.OverlapSphere(transform.position, viewRadius, boidsMask);

        foreach (var collider in boidsInRange)
        {
            MouseBoid boid = collider.GetComponent<MouseBoid>();
            if (boid == null || boid == this) continue;

            centrePos += boid.transform.position;
            count++;
        }

        if (count == 0) return centrePos;
        centrePos /= count;
        return Seek(centrePos);
    }

    private Vector3 Alignment()
    {
        int count = 0;
        Vector3 desired = Vector3.zero;
        var boidsInRange = Physics.OverlapSphere(transform.position, viewRadius, boidsMask);

        foreach (var collider in boidsInRange)
        {
            MouseBoid boid = collider.GetComponent<MouseBoid>();
            if (boid == null || boid == this) continue;

            desired += boid.Velocity;
            count++;
        }

        if (count == 0) return desired;
        desired /= count;
        return CalculateSteering(desired.normalized * _maxSpeed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}