using UnityEngine;

public class EscapeState : IEnemyState
{
    public void EnterState(EnemyController controller)
    {
        // Configurar animación
        controller.EnemyAnimator.SetBool("IsWalking", false);
        controller.EnemyAnimator.SetBool("IsRunning", true);
        controller.EnemyAnimator.SetBool("IsLooking", false);

        controller.audioSource.clip = controller.escapeSound;
        controller.audioSource.Play();
    }

    public void UpdateState(EnemyController controller)
    {
        // Volver al punto de inicio A
        controller.Steering.ReturnToStart();

        // Si llega al punto A, volver a patrullar
        if (controller.WaypointSystem.HasReachedCurrentTarget(controller.transform.position))
        {
            controller.StateMachine.ChangeState(controller.PatrolState);
        }
    }

    public void ExitState(EnemyController controller)
    {
        // Reiniciar el steering al estado inicial
        controller.WaypointSystem.ResetToStart();
        controller.audioSource.Stop();
    }
}
