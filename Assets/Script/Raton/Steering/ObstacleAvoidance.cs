using UnityEngine;
using UnityEngine.UIElements;

public class ObstacleAvoidance : MonoBehaviour, ISteeringBehavior
{
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float avoidForce = 2f;
    [SerializeField] private LayerMask obstacleMask;

    public Vector3 CalculateSteering(EnemySteering owner)
    {
        // Detectar obstáculos cercanos
        Collider[] obstacles = Physics.OverlapSphere(owner.transform.position, owner.obstacleDetectionRadius, owner.obstacleLayer);

        if (obstacles.Length == 0)
            return Vector3.zero;

        // Vector de evasión acumulado
        Vector3 avoidanceForce = Vector3.zero;

        foreach (Collider obstacle in obstacles)
        {
            // Calcular vector desde el obstáculo hacia el enemigo
            Vector3 dirToObstacle = owner.transform.position - obstacle.transform.position;
            float distance = dirToObstacle.magnitude;

            // La fuerza es inversamente proporcional a la distancia
            float weight = (owner.obstacleDetectionRadius - distance) / owner.obstacleDetectionRadius;
            weight = Mathf.Pow(weight, 2); // Aumentar influencia de objetos cercanos
            Vector3 forceToAdd = dirToObstacle.normalized * weight * 3.0f;
            avoidanceForce += forceToAdd;
        }
        return avoidanceForce;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
