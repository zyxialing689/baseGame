using UnityEngine;
using System.Collections.Generic;

public class AITestRunner : MonoBehaviour, IAIProvider
{
    public TextAsset text; // ⭐ 拖JSON进来

    private AIAgent agent;

    public TextAsset GetAIAsset()
    {
        return text;
    }
    public AIAgent GetAgent()
    {
        return agent;
    }
    void Start()
    {
        agent = new AIAgent();

        agent.AddModule<IAIPerception>(new TestPerception());

        // ⭐ 解析编辑器数据
        AIEditorData data = JsonUtility.FromJson<AIEditorData>(text.text);

        // ⭐ 转运行时
        var configs = AIEditorToRuntimeConverter.Convert(data);

        agent.Init(configs);
    }

    void Update()
    {
        agent.Update();
    }


}