using UnityEngine;

public class Flee : ISteering
{
    private Rigidbody rb;
    private Transform target;
    private float maxVelocity;

    public Flee(Rigidbody rb, Transform target, float maxVelocity)
    {
        this.rb = rb;
        this.target = target;
        this.maxVelocity = maxVelocity;
    }

    public Vector3 MoveDirection()
    {
        Vector3 desiredVelocity = (rb.position - target.position).normalized * maxVelocity;
        Vector3 steering = desiredVelocity - rb.linearVelocity;
        steering.y = 0;
        return Vector3.ClampMagnitude(steering, maxVelocity);
    }

    public void SetMaxVelocity(float newMaxVelocity)
    {
        maxVelocity = newMaxVelocity;
    }
}