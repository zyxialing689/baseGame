using UnityEngine;

public class AttackState : AIState
{
    private float timer;
    private float duration = 0.5f;
    public override void OnEnter()
    {
        Debug.Log("进入 Attack");
        timer = 0f;
    }
    public override void OnExit()
    {
        Debug.Log("离开 Attack");
    }

    public override void OnUpdate()
    {
        timer += Time.deltaTime;
    }
    public override bool CanExit()
    {
        return timer >= duration;
    }
}