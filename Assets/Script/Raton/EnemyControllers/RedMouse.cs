using UnityEngine;

public class RedMouse : EnemyController
{
    protected override void InitializeStates()
    {
        PatrolState = new PatrolState(this, StateMachine);
        AlertState = new AlertState(this, StateMachine);
        AttackState = new AttackState(this, StateMachine);
        EscapeState = new EscapeState(this, StateMachine);
        EnemylookingState = new EnemyLookingState(this, StateMachine);

        PatrolState.AddTransition(StateEnum.EnemyAlert, AlertState);
        PatrolState.AddTransition(StateEnum.Attack, AttackState);
        PatrolState.AddTransition(StateEnum.EnemyLookingState, EnemylookingState);

        EnemylookingState.AddTransition(StateEnum.EnemyPatrol, PatrolState);
        EnemylookingState.AddTransition(StateEnum.EnemyAlert, AlertState);

        AlertState.AddTransition(StateEnum.EnemyPatrol, PatrolState);
        AlertState.AddTransition(StateEnum.Attack, AttackState);

        AttackState.AddTransition(StateEnum.EnemyEscape, EscapeState);
        AttackState.AddTransition(StateEnum.EnemyAlert, AlertState);
        AttackState.AddTransition(StateEnum.EnemyPatrol, PatrolState);

        EscapeState.AddTransition(StateEnum.EnemyAlert, AlertState);

        StateMachine.SetInit(PatrolState);
    }
}