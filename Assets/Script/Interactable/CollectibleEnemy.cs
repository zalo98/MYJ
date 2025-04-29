using UnityEngine;

public class CollectibleEnemy : MonoBehaviour
{
    [SerializeField] public AudioClip captureSound;
    [SerializeField] private int pointsValue = 1; // Valor de puntos al recolectar

    // Puedes añadir cualquier propiedad específica para los recolectables
    [SerializeField] private GameObject collectEffect;

    public void OnCollected()
    {
        // Reproducir sonido
        if (captureSound != null)
        {
            AudioSource.PlayClipAtPoint(captureSound, transform.position);
        }

        // Incrementar el contador usando CollectionManager
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.IncrementCount();
        }
        else
        {
            Debug.LogWarning("CollectionManager no encontrado. No se pudo incrementar el contador.");
        }

        // Mostrar efecto visual (opcional)
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
    }
}
