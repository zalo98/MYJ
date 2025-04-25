using UnityEngine;

public class Seek : MonoBehaviour, ISteeringBehavior
{
    public Vector3 CalculateSteering(EnemySteering owner)
    {
        Vector3 targetPosition = owner.currentTargetPosition;
        Vector3 desiredVelocity = (targetPosition - owner.transform.position).normalized * owner.currentMaxSpeed;
        return desiredVelocity - owner.currentVelocity;
    }
}
