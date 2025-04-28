using System.Collections;
using System.Collections.Generic;
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
        Vector3 directionForce = desiredVelocity - rb.linearVelocity;
        directionForce.y = 0;
        directionForce = Vector3.ClampMagnitude(directionForce, maxVelocity);

        return directionForce;
    }
}
