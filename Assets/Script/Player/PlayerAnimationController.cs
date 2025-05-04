using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    
    private readonly string ANIM_WALK = "IsWalking";
    private readonly string ANIM_RUN = "IsRunning";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayIdleAnimation()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) return;
        }
        animator.SetBool(ANIM_WALK, false);
        animator.SetBool(ANIM_RUN, false);
    }

    public void PlayWalkAnimation()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) return;
        }
        animator.SetBool(ANIM_WALK, true);
        animator.SetBool(ANIM_RUN, false);
    }

    public void PlayRunAnimation()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) return;
        }
        animator.SetBool(ANIM_RUN, true);
        animator.SetBool(ANIM_WALK, false);
    }

    public void PlayInvisibleAnimation()
    { 
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) return;
        }
        animator.SetBool(ANIM_WALK, false);
        animator.SetBool(ANIM_RUN, false);
    }
}
