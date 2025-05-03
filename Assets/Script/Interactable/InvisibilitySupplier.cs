using UnityEngine;

public class InvisibilitySupplier : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController playerController;
    
    public void Interact()
    {
        playerController.ResetInvisibleTime();
        Destroy(gameObject);
    }
}