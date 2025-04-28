using UnityEngine;

public class InvisibleState : State
{
    public InvisibleState(FSM fsm) : base(fsm) { }

    public override void Awake()
    {
        Debug.Log("Estado Invisible activado.");
    }

    public override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fsm.Transition(StateEnum.Visible);
        }
    }

    public override void Sleep()
    {
        Debug.Log("Estado Invisible desactivado.");
    }
}