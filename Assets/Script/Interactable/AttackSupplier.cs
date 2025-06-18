using UnityEngine;

public class AttackSupplier : MonoBehaviour, IInteractable
{
    private PlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }
    
    public void Interact()
    {
        playerController.ResetAttackTimer();
        Destroy(gameObject);
    }
}
