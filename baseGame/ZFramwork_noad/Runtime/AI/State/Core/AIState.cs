using UnityEngine;

public class AIState 
{
    public AIAgent agent;
    public AIStateData stateData;
    public AITryCondition tryCond;
    public bool enableUpdate;
    public bool enableFixedUpdate;
    public AIState Init(AIAgent aIAgent,AIStateData aIStateData)
    {
        agent = aIAgent;
        stateData = aIStateData;
        tryCond = new AITryCondition(agent);
        enableUpdate = false;
        enableFixedUpdate = false;
        Awake();
        return this;
    }

    public virtual void Awake()
    {

    }
    public virtual void Start()
    {

    }

    public virtual void UpdateExecute()
    {

    }
    public virtual void FixedUpdateExecute()
    {

    }

    public virtual AIStateData TryNextCond()
    {
        if (stateData.condAIStateData != null)
        {
            for (int i = 0; i < stateData.condAIStateData.Count; i++)
            {
                if (tryCond.GetTryCondition(stateData.condAIStateData[i].condTypes, stateData.condAIStateData[i]))
                {
                    return stateData.condAIStateData[i];
                }
            }
        }

        return null;
    }

    public virtual bool TryRestCond()
    {
        return false;
    }
    public virtual void Exit()
    {

    }

    ////////////////////////////////////////////通用所有ai条件判断
    
    public virtual bool ISOverTime()
    {
        return (stateData.keepTime > 0 && stateData.nextTime <= agent.curTime);
    }
}
