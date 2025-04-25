using UnityEngine;

public class EscapeState : MonoBehaviour, IEnemyState
{
    public void EnterState(EnemyController controller)
    {
        // Configurar animación
        controller.EnemyAnimator.SetBool("IsWalking", false);
        controller.EnemyAnimator.SetBool("IsRunning", true);
        controller.EnemyAnimator.SetBool("IsLooking", false);
    }

    public void UpdateState(EnemyController controller)
    {
        // Volver al punto de inicio A
        controller.Steering.ReturnToStart();

        // Si llega al punto A, volver a patrullar
        if (controller.Steering.HasReachedCurrentTarget())
        {
            controller.StateMachine.ChangeState(controller.PatrolState);
        }
    }

    public void ExitState(EnemyController controller)
    {
        // Reiniciar el steering al estado inicial
        controller.Steering.ResetToStart();
    }
}
