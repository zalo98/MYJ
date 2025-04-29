using UnityEngine;

public class QuestionNode : IDesitionNode
{
    private IDesitionNode trueNode;
    private IDesitionNode falseNode;
    private System.Func<bool> question;

    public QuestionNode(IDesitionNode trueNode, IDesitionNode falseNode, System.Func<bool> question)
    {
        this.trueNode = trueNode;
        this.falseNode = falseNode;
        this.question = question;
    }
    
    public void Execute()
    {
        if (question())
        {
            trueNode.Execute();
        }
        else
        {
            falseNode.Execute();
        }
    }
}