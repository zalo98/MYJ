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

        // Activar el escape táctico en EnemySteering
        if (controller.steering != null)
        {
            // Forzar el inicio del escape táctico
            controller.steering.StartEscapeMode(); // Necesitarás hacer este método público
        }
    }

    public override void Execute()
    {
        // Actualizar detección
        controller.enemyVision.UpdateDetection();

        // USAR EL PATHFINDING TÁCTICO en lugar del flee reactivo
        if (controller.steering != null)
        {
            // Llamar al método FollowPath que tiene el pathfinding táctico
            controller.steering.FollowPath();
        }

        // Lógica de timeout mejorada
        if (!controller.enemyVision.HasDirectDetection && !controller.enemyVision.HasPeripheralDetection)
        {
            escapeTimer += Time.deltaTime;

            // También verificar si llegó al punto A
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
            escapeTimer = 0f; // Resetear timer si sigue viendo al player
        }
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsRunning", false);

        // Asegurar que el escape termine correctamente
        if (controller.steering != null)
        {
            controller.steering.CompleteEscape(); // Necesitarás hacer este método público
        }

        Debug.Log("😴 MouseEscapeState desactivado");
    }

    // Verificar si está en zona segura (cerca del punto A)
    private bool IsInSafeZone()
    {
        if (controller.steering == null || controller.steering.mouseMovement == null)
            return false;

        float safeDistance = 4f;
        Vector3 pointA = controller.steering.mouseMovement.startPoint.position;
        return Vector3.Distance(controller.transform.position, pointA) <= safeDistance;
    }
}