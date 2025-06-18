using System.Collections.Generic;
using UnityEngine;

public class BlueMouse : EnemyController
{
    private static List<BlueMouse> allBlueMice = new List<BlueMouse>();

    [HideInInspector] public State FlockingPatrolState;
    [HideInInspector] public State FlockingAttackState;
    [HideInInspector] public State BlueEscapeState;

    private MouseBoid mouseBoid;

    private void OnEnable()
    {
        if (!allBlueMice.Contains(this))
        {
            allBlueMice.Add(this);
        }
    }

    private void OnDisable()
    {
        if (allBlueMice.Contains(this))
        {
            allBlueMice.Remove(this);
        }
    }

    public static void BroadcastToAllMice(StateEnum targetState, BlueMouse initiator, Vector3 detectionPosition)
    {
        foreach (var mouse in allBlueMice)
        {
            if (mouse != initiator && mouse != null)
            {
                mouse.TransitionToState(targetState);
                mouse.SetLastSeenPosition(detectionPosition);
            }
        }
    }

    protected override void InitializeStates()
    {
        FlockingPatrolState = new FlockingPatrolState(this, StateMachine);
        FlockingAttackState = new FlockingAttackState(this, StateMachine);
        BlueEscapeState = new BlueEscapeState(this, StateMachine);

        FlockingPatrolState.AddTransition(StateEnum.FlockingAttackState, FlockingAttackState);
        
        FlockingAttackState.AddTransition(StateEnum.FlockingPatrolState, FlockingPatrolState);
        FlockingAttackState.AddTransition(StateEnum.BlueEscapeState, BlueEscapeState);

        BlueEscapeState.AddTransition(StateEnum.FlockingPatrolState, FlockingPatrolState);

        StateMachine.SetInit(FlockingPatrolState);
    }

    private void Start()
    {
        base.Start();

        mouseBoid = GetComponent<MouseBoid>();
        if (mouseBoid == null)
        {
            mouseBoid = gameObject.AddComponent<MouseBoid>();
        }

        mouseBoid.SetMaxSpeed(walkSpeed);
    }

    public void TransitionToState(StateEnum targetState)
    {
        StateMachine.Transition(targetState);
    }

    public void SetLastSeenPosition(Vector3 position)
    {
        if (enemyVision != null && !enemyVision.HasDirectDetection)
        {
            enemyVision.ForceLastSeenPosition(position);
        }
    }
    
    public static List<BlueMouse> GetAllBlueMice()
    {
        return allBlueMice;
    }
}