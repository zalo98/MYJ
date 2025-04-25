using UnityEngine;

public class EnemyLineOfSight : MonoBehaviour
{
    // Referencias
    private EnemyController controller;

    [Header("Configuraci�n de Visi�n")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public LayerMask obstacleLayer;

    public void Initialize()
    {
        controller = GetComponent<EnemyController>();
    }

    public bool CanSeePlayer()
    {
        if (controller.PlayerTransform == null)
            return false;

        Vector3 dirToPlayer = controller.PlayerTransform.position - transform.position;
        float distanceToPlayer = dirToPlayer.magnitude;

        // Comprobamos si est� dentro del rango de visi�n
        if (distanceToPlayer > viewDistance)
            return false;

        // Comprobamos si est� dentro del �ngulo de visi�n
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2)
            return false;

        // Comprobamos si hay obst�culos entre el enemigo y el jugador
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer.normalized, out hit, distanceToPlayer, obstacleLayer))
        {
            if (!hit.transform.CompareTag("Player"))
                return false; // Hay un obst�culo
        }

        // Si llegamos aqu�, el jugador est� a la vista
        return true;
    }

    public float GetDistanceToPlayer()
    {
        if (controller.PlayerTransform == null)
            return float.MaxValue;

        return Vector3.Distance(transform.position, controller.PlayerTransform.position);
    }

    public Vector3 GetDirectionToPlayer()
    {
        if (controller.PlayerTransform == null)
            return Vector3.zero;

        return (controller.PlayerTransform.position - transform.position).normalized;
    }

    // Para visualizar el cono de visi�n en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Solo dibujar el cono si estamos en modo de edici�n
        if (!Application.isPlaying)
        {
            Vector3 forward = transform.forward;
            Vector3 leftRayDirection = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
            Vector3 rightRayDirection = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

            Gizmos.DrawRay(transform.position, leftRayDirection * viewDistance);
            Gizmos.DrawRay(transform.position, rightRayDirection * viewDistance);

            // Dibujar arco para mejor visualizaci�n
            int segments = 20;
            Vector3 prevPos = transform.position + leftRayDirection * viewDistance;

            for (int i = 1; i <= segments; i++)
            {
                float angle = -viewAngle / 2 + viewAngle * i / segments;
                Vector3 newDir = Quaternion.Euler(0, angle, 0) * forward;
                Vector3 newPos = transform.position + newDir * viewDistance;

                Gizmos.DrawLine(prevPos, newPos);
                prevPos = newPos;
            }
        }
    }
}
