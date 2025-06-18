using UnityEngine;

public class AttackSupplier : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController playerController;
    
    public void Interact()
    {
        playerController.ResetAttackTimer();
        Destroy(gameObject);
    }
}
