using UnityEngine;

public class MoveState : AIState
{
    public override void OnUpdate()
    {
        var perception = agent.Get<IAIPerception>();
        var move = agent.Get<IAIMove>();

        if (perception == null || move == null)
            return;

        if (!perception.HasTarget())
        {
            agent.SetState(new IdleState());
            return;
        }

        move.MoveTo(perception.GetTargetPosition());
    }
}