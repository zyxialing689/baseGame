using System;
using System.Collections.Generic;
using UnityEngine;

public class AIAgent
{
    private Dictionary<Type, object> modules = new Dictionary<Type, object>();
    private Dictionary<int, AIStateConfig> configs;
    private Dictionary<int, AIState> states;
    private int currentStateId;
    private AIState currentState;
    public float StateTime { get; private set; }
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
            // ⭐⭐⭐ 新增：缓存 condition
            foreach (var t in cfg.transitions)
            {
                t.condition = AIConditionFactory.Create(t.conditionType);
            }
        }

        // 默认进入第一个状态
        currentStateId = configList[0].id;
        currentState = states[currentStateId];
        StateTime = 0f; // ⭐ 加这一句
        currentState.OnEnter(); // ✅ 必须加
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
    public void SetState(int stateId)
    {
        currentState?.OnExit();

        currentStateId = stateId;
        currentState = states[stateId];
        StateTime = 0f; // ⭐⭐ 核心：重置状态时间
        currentState.OnEnter();
    }

    // 更新
    public void Update()
    {
        StateTime += Time.deltaTime; // ⭐ 新增
        currentState.OnUpdate();

        // ⭐⭐⭐ 关键：由State控制是否能离开
        if (!currentState.CanExit())
        {
            AIDebug(currentStateId, -1);
            return;
        }

        var cfg = configs[currentStateId];

        bool switched = false;

        foreach (var t in cfg.transitions)
        {

            if (t.condition.Check(this, t.param))
            {
                AIDebug(currentStateId, t.targetStateId);
                SetState(t.targetStateId);

                switched = true;
                break;
            }
        }

        if (!switched)
        {
            AIDebug(currentStateId, -1);
        }
    }

    private void AIDebug(int current, int next)
    {
        if (!AIDebugger.debugData.TryGetValue(this, out var data))
        {
            data = new AIDebugData();
            AIDebugger.debugData[this] = data;
        }

        data.currentStateId = current;
        data.nextStateId = next;

        AIDebugger.onUpdate?.Invoke();
    }
}