using UnityEngine;

public class Seek : ISteering
{
    private Rigidbody rb;
    private Transform target;
    private float maxVelocity;

    public Seek(Rigidbody rb, Transform target, float maxVelocity)
    {
        this.rb = rb;
        this.target = target;
        this.maxVelocity = maxVelocity;
    }

    public Vector3 MoveDirection()
    {
        Vector3 desiredVelocity = (target.position - rb.position).normalized * maxVelocity;
        Vector3 steering = desiredVelocity - rb.linearVelocity;
        steering.y = 0;

        RotateTowards(desiredVelocity);

        return Vector3.ClampMagnitude(steering, maxVelocity);
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothedRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, 360f * Time.deltaTime);

        rb.MoveRotation(smoothedRotation);
    }

    public void SetMaxVelocity(float newMaxVelocity)
    {
        maxVelocity = newMaxVelocity;
    }
}