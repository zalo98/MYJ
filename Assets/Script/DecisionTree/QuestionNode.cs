using UnityEngine;

public class QuestionNode : IDecisionNode
{
    private IDecisionNode trueNode;
    private IDecisionNode falseNode;
    private System.Func<bool> question;

    public QuestionNode(IDecisionNode trueNode, IDecisionNode falseNode, System.Func<bool> question)
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