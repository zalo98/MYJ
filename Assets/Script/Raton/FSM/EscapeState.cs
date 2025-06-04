using UnityEngine;

public class EscapeState : State
{
    private EnemyController enemyController;

    public EscapeState(EnemyController controller, FSM fsm) : base(fsm)
    {
        enemyController = controller;
    }

    public override void Awake()
    {
        
    }

    public override void Execute()
    {
        EscapeBehavior();
    }

    private void EscapeBehavior()
    {
        enemyController.EnemyAnimator.SetBool("IsWalking", false);
        enemyController.EnemyAnimator.SetBool("IsRunning", true);
        enemyController.EnemyAnimator.SetBool("IsLooking", false);
        
        if (!enemyController.audioSource.isPlaying)
        {
            enemyController.audioSource.clip = enemyController.escapeSound;
            enemyController.audioSource.Play();
        }
        
        enemyController.steering.ReturnToStart();
        
        if (enemyController.mouseMovement.HasReachedCurrentTarget(enemyController.transform.position))
        {
            enemyController.StateMachine.Transition(StateEnum.EnemyPatrol);
        }
    }

    public override void Sleep()
    {
        enemyController.mouseMovement.ResetToStart();
        
        enemyController.audioSource.Stop();
    }
}