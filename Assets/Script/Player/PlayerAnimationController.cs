using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    // Nombres de los parámetros en el Animator
    private readonly string ANIM_WALK = "IsWalking";
    private readonly string ANIM_RUN = "IsRunning";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No se encontró el componente Animator en el jugador");
        }
    }

    public void PlayIdleAnimation()
    {
        // Para Idle, desactivamos todas las animaciones
        animator.SetBool(ANIM_WALK, false);
        animator.SetBool(ANIM_RUN, false);
    }

    public void PlayWalkAnimation()
    {
        // Activamos Walk, desactivamos Run
        animator.SetBool(ANIM_WALK, true);
        animator.SetBool(ANIM_RUN, false);
    }

    public void PlayRunAnimation()
    {
        // Activamos Run, desactivamos Walk
        animator.SetBool(ANIM_RUN, true);
        animator.SetBool(ANIM_WALK, false);
    }
}
