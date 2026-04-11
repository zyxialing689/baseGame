public abstract class AIState
{
    protected AIAgent agent;

    public void Init(AIAgent agent)
    {
        this.agent = agent;
        OnEnter();
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}