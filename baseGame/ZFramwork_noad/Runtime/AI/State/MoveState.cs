using UnityEngine;

public class MoveState : AIState
{
    public override void OnEnter()
    {
        Debug.Log("进入 Move");
    }
    public override void OnExit()
    {
        Debug.Log("离开 Move");
    }
    public override void OnUpdate()
    {

    }
}