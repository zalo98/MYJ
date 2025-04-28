using UnityEngine;

public class VisibleState : State
{
    public VisibleState(FSM fsm) : base(fsm) { }

    public override void Awake()
    {
        Debug.Log("Estado Visible activado.");
    }

    public override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fsm.Transition(StateEnum.Invisible);
        }
    }

    public override void Sleep()
    {
        Debug.Log("Estado Visible desactivado.");
    }
}