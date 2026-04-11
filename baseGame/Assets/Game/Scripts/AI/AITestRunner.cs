using UnityEngine;
using System.Collections.Generic;

public class AITestRunner : MonoBehaviour
{
    private AIAgent agent;

    void Start()
    {
        agent = new AIAgent();

        agent.AddModule<IAIMove>(new TestMove());
        agent.AddModule<IAIPerception>(new TestPerception());

        // 构造数据（模拟编辑器）
        // var configs = new List<AIStateConfig>()
        // {
        //     new AIStateConfig
        //     {
        //         id = 1,
        //         stateType = "Idle",
        //         nextStates = new List<int>{2}
        //     },
        //     new AIStateConfig
        //     {
        //         id = 2,
        //         stateType = "Move",
        //         nextStates = new List<int>{1}
        //     }
        // };

        // agent.Init(configs);
    }

    void Update()
    {
        agent.Update();
    }
}