public class HasTargetCondition : IAICondition
{
    public bool Check(AIAgent agent)
    {
        var p = agent.Get<IAIPerception>();
        return p != null && p.HasTarget();
    }
}