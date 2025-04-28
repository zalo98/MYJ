using UnityEngine;

public class MouseEscapeState : State
{
    private EnemyController controller;
    private float escapeTimer = 0f;
    private float escapeTimeout = 5f;

    public MouseEscapeState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsRunning", true);
        escapeTimer = 0f;
    }

    public override void Execute()
    {
        EvadePlayer();
        
        controller.enemyVision.UpdateDetection();
        
        if (!controller.enemyVision.HasDirectDetection)
        {
            escapeTimer += Time.deltaTime;
        
            if (escapeTimer >= escapeTimeout)
            {
                controller.StateMachine.Transition(StateEnum.MousePatrolState);
                return;
            }
        }
        else
        {
            escapeTimer = 0f;
        }
    }

    private void EvadePlayer()
    {
        if (controller.Steering != null)
        {
            Flee fleeBehavior = controller.Steering.FleeBehavior;

            if (fleeBehavior != null)
            {
                Vector3 moveDirection = fleeBehavior.MoveDirection();
                moveDirection.y = 0f;

                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    controller.transform.rotation = Quaternion.LookRotation(moveDirection.normalized);
                }

                controller.transform.position += moveDirection.normalized * controller.runSpeed * Time.deltaTime;
            }
        }
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsRunning", false);
    }
}