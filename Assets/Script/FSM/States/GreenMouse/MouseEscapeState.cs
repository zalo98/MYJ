using UnityEngine;

public class MouseEscapeState : State
{
    private EnemyController controller;
    private float escapeTimer = 0f;
    private float escapeTimeout = 5f;
    private bool escapeInitiated = false;

    public MouseEscapeState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsRunning", true);
        escapeTimer = 0f;
        escapeInitiated = false;

        Debug.Log("🚨 MouseEscapeState activado - iniciando escape táctico");

        if (controller.steering != null)
        {
            controller.steering.StartEscapeMode();
        }
    }

    public override void Execute()
    {
        controller.enemyVision.UpdateDetection();

        if (controller.steering != null)
        {
            controller.steering.FollowPath();
        }

        if (!controller.enemyVision.HasDirectDetection && !controller.enemyVision.HasPeripheralDetection)
        {
            escapeTimer += Time.deltaTime;

            bool reachedSafeZone = IsInSafeZone();

            if (escapeTimer >= escapeTimeout || reachedSafeZone)
            {
                Debug.Log("✅ Escape completado - volviendo a patrullaje");
                controller.StateMachine.Transition(StateEnum.MousePatrolState);
                return;
            }
        }
        else
        {
            escapeTimer = 0f;
        }
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsRunning", false);

        if (controller.steering != null)
        {
            controller.steering.CompleteEscape();
        }

        Debug.Log("😴 MouseEscapeState desactivado");
    }

    private bool IsInSafeZone()
    {
        if (controller.steering == null || controller.steering.mouseMovement == null)
            return false;

        float safeDistance = 4f;
        Vector3 pointA = controller.steering.mouseMovement.startPoint.position;
        return Vector3.Distance(controller.transform.position, pointA) <= safeDistance;
    }
}