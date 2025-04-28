using UnityEngine;

public class PatrolState : IEnemyState
{
    public void EnterState(EnemyController controller)
    {
        // Configurar animación
        controller.EnemyAnimator.SetBool("IsWalking", true);
        controller.EnemyAnimator.SetBool("IsRunning", false);
        controller.EnemyAnimator.SetBool("IsLooking", false);
    }

    public void UpdateState(EnemyController controller)
    {
        // Comprobar si el jugador está visible
        if (controller.LineOfSight.CanSeePlayer())
        {
            controller.StateMachine.ChangeState(controller.EscapingState);
            return;
        }

        // Seguir la ruta definida
        controller.Steering.FollowPath();

        // Revisar si ha llegado al punto objetivo actual
        if (controller.WaypointSystem.HasReachedCurrentTarget(controller.transform.position))
        {
            // Decidir aleatoriamente si mirar alrededor
            if (Random.value < 0.2f) // 20% de probabilidad
            {
                controller.StateMachine.ChangeState(controller.LookingState);
            }
            else
            {
                // Avanzar al siguiente punto
                controller.WaypointSystem.MoveToNextTarget();
            }
        }
    }

    public void ExitState(EnemyController controller)
    {
        // Cualquier limpieza necesaria al salir de este estado
    }
}
