using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    [Header("Portal Management")]
    public Portal[] allPortals;
    public LayerMask obstacleMask = -1;
    public LayerMask playerMask = 1;

    [Header("Portal Selection")]
    public float playerBlockRadius = 3f; // Radio para considerar que player bloquea portal
    public float maxPathDistance = 50f; // Distancia máxima de path válido

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-encontrar portales si no están asignados
        if (allPortals == null || allPortals.Length == 0)
        {
            allPortals = FindObjectsOfType<Portal>();    
        }
    }

    // Encontrar el mejor portal para escape
    public Portal FindBestPortal(Vector3 fromPosition, Vector3 playerPosition)
    {
        if (allPortals == null || allPortals.Length == 0)
        {
            return null;
        }

        List<PortalOption> portalOptions = new List<PortalOption>();

        // Evaluar cada portal
        foreach (Portal portal in allPortals)
        {
            if (portal == null || !portal.IsAvailable) continue;

            PortalOption option = EvaluatePortal(portal, fromPosition, playerPosition);
            if (option.isValid)
            {
                portalOptions.Add(option);
            }
        }

        if (portalOptions.Count == 0)
        {
            return null;
        }

        // Ordenar por score (mejor primero)
        portalOptions = portalOptions.OrderByDescending(p => p.score).ToList();

        Portal bestPortal = portalOptions[0].portal;

        return bestPortal;
    }

    // Evaluar un portal específico
    PortalOption EvaluatePortal(Portal portal, Vector3 fromPosition, Vector3 playerPosition)
    {
        PortalOption option = new PortalOption
        {
            portal = portal,
            isValid = false,
            score = 0f
        };

        Vector3 portalPos = portal.transform.position;

        // 1. Distancia al portal (más cerca = mejor)
        float distanceToPortal = Vector3.Distance(fromPosition, portalPos);
        float distanceScore = Mathf.Lerp(10f, 0f, distanceToPortal / 20f); // Score de 0-10

        // 2. ¿Player bloquea el portal?
        float distancePlayerToPortal = Vector3.Distance(playerPosition, portalPos);
        bool playerBlocksPortal = distancePlayerToPortal < playerBlockRadius;
        float playerScore = playerBlocksPortal ? -5f : 5f; // Penalizar si player está cerca

        // 3. ¿Hay línea de vista directa al portal?
        bool hasLineOfSight = !Physics.Linecast(fromPosition, portalPos, obstacleMask);
        float lineOfSightScore = hasLineOfSight ? 3f : 0f;

        // 4. ¿Player está entre el ratón y el portal?
        bool playerInPath = IsPlayerInPath(fromPosition, portalPos, playerPosition);
        float pathScore = playerInPath ? -3f : 2f;

        // Score total
        option.score = distanceScore + playerScore + lineOfSightScore + pathScore;
        option.isValid = option.score > 0f; // Solo portales con score positivo

        return option;
    }

    // Verificar si player está en el camino al portal
    bool IsPlayerInPath(Vector3 from, Vector3 to, Vector3 playerPos)
    {
        Vector3 pathDirection = (to - from).normalized;
        Vector3 toPlayer = playerPos - from;

        // Proyección del player sobre la línea del path
        float projection = Vector3.Dot(toPlayer, pathDirection);

        // Si la proyección está fuera del path, player no interfiere
        if (projection < 0 || projection > Vector3.Distance(from, to))
            return false;

        // Punto más cercano en el path al player
        Vector3 closestPointOnPath = from + pathDirection * projection;

        // Distancia del player al path
        float distanceToPath = Vector3.Distance(playerPos, closestPointOnPath);

        // Player interfiere si está muy cerca del path
        return distanceToPath < 2f;
    }

    // Verificar si un portal es accesible desde una posición
    public bool IsPortalAccessible(Portal portal, Vector3 fromPosition)
    {
        if (portal == null || !portal.IsAvailable) return false;

        // Verificar distancia máxima
        float distance = Vector3.Distance(fromPosition, portal.transform.position);
        return distance <= maxPathDistance;
    }

    // Obtener todos los portales disponibles
    public Portal[] GetAvailablePortals()
    {
        return allPortals?.Where(p => p != null && p.IsAvailable).ToArray() ?? new Portal[0];
    }

    // Estructura para evaluar portales
    [System.Serializable]
    public class PortalOption
    {
        public Portal portal;
        public bool isValid;
        public float score;
    }

    // Debugging en editor
    void OnDrawGizmos()
    {
        if (allPortals == null) return;

        foreach (Portal portal in allPortals)
        {
            if (portal == null) continue;

            // Mostrar radio de bloqueo del player
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawSphere(portal.transform.position, playerBlockRadius);
        }
    }
}
