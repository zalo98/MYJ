using UnityEngine;

public class ActionNode : IDesitionNode
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
