using UnityEngine;

public class CollectibleEnemy : MonoBehaviour
{
    [SerializeField] public AudioClip captureSound;
    [SerializeField] private int pointsValue = 1;
    
    [SerializeField] private GameObject collectEffect;

    public void OnCollected()
    {
        if (captureSound != null)
        {
            AudioSource.PlayClipAtPoint(captureSound, transform.position);
        }
        
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.IncrementCount();
        }
        else
        {
            Debug.LogWarning("CollectionManager no encontrado. No se pudo incrementar el contador.");
        }
        
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
    }
}
