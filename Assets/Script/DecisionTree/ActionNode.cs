using UnityEngine;

public class ActionNode : IDecisionNode
{
    private System.Action action;

    public ActionNode(System.Action action)
    {
        this.action = action;
    }

    public void Execute()
    {
        action?.Invoke();
    }
}