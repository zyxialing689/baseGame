using System;
using System.Collections.Generic;

public class AIAgent
{
    private Dictionary<Type, object> modules = new Dictionary<Type, object>();
    private Dictionary<int, AIStateConfig> configs;
    private Dictionary<int, AIState> states;
    private int currentStateId;
    private AIState currentState;

    public void Init(List<AIStateConfig> configList)
    {
        configs = new Dictionary<int, AIStateConfig>();
        states = new Dictionary<int, AIState>();

        foreach (var cfg in configList)
        {
            configs[cfg.id] = cfg;

            var state = AIStateFactory.Create(cfg.stateType);
            if (state != null)
            {
                state.Init(this);
                states[cfg.id] = state;
            }
        }

        // 默认进入第一个状态
        currentStateId = configList[0].id;
    }

    // 添加能力模块
    public void AddModule<T>(T module)
    {
        modules[typeof(T)] = module;
    }

    // 获取能力模块
    public T Get<T>()
    {
        if (modules.TryGetValue(typeof(T), out var m))
            return (T)m;

        return default;
    }

    // 切换状态
    public void SetState(AIState state)
    {
        currentState?.OnExit();
        currentState = state;
        currentState.Init(this);
    }

    // 更新
    public void Update()
    {
        if (!states.ContainsKey(currentStateId))
            return;

        var state = states[currentStateId];
        state.OnUpdate();

        var cfg = configs[currentStateId];

        foreach (var t in cfg.transitions)
        {
            var condition = AIConditionFactory.Create(t.conditionType);

            if (condition.Check(this))
            {
                currentStateId = t.targetStateId;
                break;
            }
        }
    }
}