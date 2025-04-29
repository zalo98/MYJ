using System;
using System.Collections.Generic;

public class FSM
{
    public State _currentState;
    Action<StateEnum, State, State> onTransition = delegate { };

    public FSM() { }

    public void SetInit(State init)
    {
        _currentState = init;
        _currentState.Awake();
    }

    public void Update()
    {
        _currentState.Execute();
    }

    public void Transition(StateEnum input)
    {
        State newState = _currentState.GetTransition(input);
        if (newState == null) return;

        _currentState.Sleep();
        onTransition(input, _currentState, newState);
        _currentState = newState;
        _currentState.Awake();
    }
    
    public State GetCurrentState()
    {
        return _currentState;
    }
}