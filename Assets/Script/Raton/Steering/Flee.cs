using UnityEngine;

public class Flee : MonoBehaviour, ISteeringBehavior
{
    public Vector3 CalculateSteering(EnemySteering owner)
    {
        Vector3 targetPosition = owner.controller.PlayerTransform.position;
        Vector3 desiredVelocity = (owner.transform.position - targetPosition).normalized * owner.currentMaxSpeed;

        // Si estamos muy cerca, aplicar más fuerza
        float distance = Vector3.Distance(owner.transform.position, targetPosition);
        float multiplier = Mathf.Clamp(1.0f + (owner.obstacleDetectionRadius - distance) / owner.obstacleDetectionRadius, 1.0f, 2.0f);

        return (desiredVelocity - owner.currentVelocity) * multiplier;
    }
}
