using UnityEngine;

public class Evade : ISteering
{
    private Rigidbody rb;
    private Rigidbody targetRb;
    private float maxVelocity;
    private float timePrediction;

    public Evade(Rigidbody rb, Rigidbody targetRb, float maxVelocity, float timePrediction)
    {
        this.rb = rb;
        this.targetRb = targetRb;
        this.maxVelocity = maxVelocity;
        this.timePrediction = timePrediction;
    }

    public Vector3 MoveDirection()
    {
        float distance = Vector3.Distance(rb.position, targetRb.position);
        Vector3 predictedPosition = targetRb.position + targetRb.linearVelocity * timePrediction * distance;

        Vector3 desiredVelocity = (rb.position - predictedPosition).normalized * maxVelocity;
        Vector3 steering = desiredVelocity - rb.linearVelocity;
        steering.y = 0;
        return Vector3.ClampMagnitude(steering, maxVelocity);
    }

    public void SetMaxVelocity(float newMaxVelocity)
    {
        maxVelocity = newMaxVelocity;
    }
}