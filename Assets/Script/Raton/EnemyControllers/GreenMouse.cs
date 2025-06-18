using UnityEngine;

public class GreenMouse : EnemyController
{
    [HideInInspector] public State MousePatrolState;
    [HideInInspector] public State MouseLookingState;
    [HideInInspector] public State MouseEscapeState;

    
    protected override void InitializeStates()
    {
        MousePatrolState = new MousePatrolState(this, StateMachine);
        MouseLookingState = new LookingState(this, StateMachine);
        MouseEscapeState = new MouseEscapeState(this, StateMachine);

        MousePatrolState.AddTransition(StateEnum.MouseLookingState, MouseLookingState);
        MousePatrolState.AddTransition(StateEnum.MouseEscapeState, MouseEscapeState);

        MouseLookingState.AddTransition(StateEnum.MouseEscapeState, MouseEscapeState);
        MouseLookingState.AddTransition(StateEnum.MousePatrolState, MousePatrolState);

        MouseEscapeState.AddTransition(StateEnum.MousePatrolState, MousePatrolState);
        MouseEscapeState.AddTransition(StateEnum.MouseLookingState, MouseLookingState);

        StateMachine.SetInit(MousePatrolState);
    }
}