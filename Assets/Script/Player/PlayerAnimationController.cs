using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    
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
        animator.SetBool(ANIM_WALK, false);
        animator.SetBool(ANIM_RUN, false);
    }

    public void PlayWalkAnimation()
    {
        animator.SetBool(ANIM_WALK, true);
        animator.SetBool(ANIM_RUN, false);
    }

    public void PlayRunAnimation()
    {
        animator.SetBool(ANIM_RUN, true);
        animator.SetBool(ANIM_WALK, false);
    }

    public void PlayInvisibleAnimation()
    { 
        animator.SetBool(ANIM_WALK, false);
        animator.SetBool(ANIM_RUN, false);
    }
}
