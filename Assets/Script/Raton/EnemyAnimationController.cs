using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private EnemyController controller;

    public void Initialize()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<EnemyController>();

        if (animator == null)
        {
            Debug.LogError("No se encontr� componente Animator en el enemigo");
        }
    }

    public void SetWalking(bool isWalking)
    {
        animator.SetBool("IsWalking", isWalking);
    }

    public void SetRunning(bool isRunning)
    {
        animator.SetBool("IsRunning", isRunning);
    }

    public void SetLooking(bool isLooking)
    {
        animator.SetBool("IsLooking", isLooking);
    }

    // M�todo para cambiar a una animaci�n espec�fica
    public void ChangeAnimation(string animationType)
    {
        switch (animationType)
        {
            case "Walk":
                SetWalking(true);
                SetRunning(false);
                SetLooking(false);
                break;

            case "Run":
                SetWalking(false);
                SetRunning(true);
                SetLooking(false);
                break;

            case "Look":
                SetWalking(false);
                SetRunning(false);
                SetLooking(true);
                break;
        }
    }
}
