using System;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public string portalID = "Portal_01";
    public Transform teleportDestination; // Punto A del ratón
    public float activationRadius = 1f;
    public bool isActive = true;

    [Header("Cooldown")]
    public float cooldownTime = 2f;
    private float lastUsedTime = -999f;

    [Header("Visual Effects")]
    public GameObject portalEffect;
    public AudioClip teleportSound;

    // Estado del portal
    public bool IsAvailable => isActive && (Time.time - lastUsedTime) >= cooldownTime;

    // Eventos
    public System.Action<GameObject> OnPortalUsed;

    void Start()
    {
        // Configurar efectos visuales
        if (portalEffect != null)
            portalEffect.SetActive(isActive);
    }

    void OnTriggerEnter(Collider other)
    {
        // Solo ratones pueden usar portales
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null && IsAvailable)
        {
            TeleportEnemy(enemy);
        }
    }

    private void TeleportEnemy(EnemyController enemy)
    {
        if (teleportDestination == null)
        {
            return;
        }

        // Teleportar al enemigo
        enemy.transform.position = teleportDestination.position;
        enemy.transform.rotation = teleportDestination.rotation;

        // Notificar al sistema de movimiento
        var mouseMovement = enemy.GetComponent<MouseMovement>();
        if (mouseMovement != null)
        {
            mouseMovement.CompleteEscapeViaPortal();
        }

        // Activar cooldown
        lastUsedTime = Time.time;

        // Efectos (opcional)
        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);
        }
    }
}
