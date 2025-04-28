using UnityEngine;

public class AlertState : State
{
    private EnemyController controller;

    public AlertState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        Debug.Log("Entering AlertState");
    }

    public override void Execute()
    {
        
    }

    public override void Sleep()
    {
        
    }
}
