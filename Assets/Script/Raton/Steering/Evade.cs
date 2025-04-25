using UnityEngine;

public class Evade : MonoBehaviour, ISteeringBehavior
{
    public float predictionTime = 0.5f;

    public Vector3 CalculateSteering(EnemySteering owner)
    {
        Transform target = owner.controller.PlayerTransform;
        if (target == null)
            return Vector3.zero;

        // Predecir posici�n futura del objetivo
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        Vector3 targetPos;

        if (targetRb != null)
        {
            // Si tiene rigidbody, predicci�n basada en velocidad
            targetPos = target.position + targetRb.linearVelocity * predictionTime;
        }
        else
        {
            // Si no, aproximaci�n basada en posici�n anterior
            targetPos = target.position + (target.position - owner.lastTargetPosition) / Time.deltaTime * predictionTime;
        }

        // Guardar posici�n para siguiente frame
        owner.lastTargetPosition = target.position;

        // Usar Flee desde la posici�n futura
        Vector3 desiredVelocity = (owner.transform.position - targetPos).normalized * owner.currentMaxSpeed;
        return desiredVelocity - owner.currentVelocity;
    }
}
