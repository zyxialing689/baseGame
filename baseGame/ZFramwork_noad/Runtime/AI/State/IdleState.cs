public class IdleState : AIState
{
    public override void OnUpdate()
    {
        var perception = agent.Get<IAIPerception>();

        if (perception != null && perception.HasTarget())
        {
            agent.SetState(new MoveState());
        }
    }
}