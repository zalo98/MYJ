using System.Collections.Generic;

public abstract class State
{
    protected FSM fsm;
    private Dictionary<StateEnum, State> transitions = new Dictionary<StateEnum, State>();

    public State(FSM fsm)
    {
        this.fsm = fsm;
    }

    public virtual void Awake() { }
    public virtual void Execute() { }
    public virtual void Sleep() { }
    
    public void AddTransition(StateEnum input, State nextState)
    {
        if (!transitions.ContainsKey(input))
        {
            transitions.Add(input, nextState);
        }
    }
    
    public State GetTransition(StateEnum input)
    {
        if (transitions.ContainsKey(input))
        {
            return transitions[input];
        }
        return null;
    }
}