using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBuff
{
    public float curTime;
    public float curLoopTime;
    public AIAgent agent;
    public BuffType buffType;
    public int curLoopTimes = 0;
    public float duration;
    public float trigger_time;
    public string effect_path;
    public bool trigger_beHurt;
    /// <summary>
    /// 初始化buff
    /// </summary>
    /// <param name="buff"></param>
    public BaseBuff InitBuff(AIAgent agent,BuffData buff)
    {
        this.agent = agent;
        trigger_beHurt = buff.trigger_beHurt;
        curLoopTimes = 0;
        curTime = 0;
        curLoopTime = 0;
        duration = buff.duration;
        trigger_time = buff.trigger_time;
        buffType = buff.buffType;
        effect_path = buff.effect_path;
        for (int i = 0; i < buff.conflict_buff_type.Length; i++)
        {
            if (agent.IsInBuff(buff.conflict_buff_type[i]))
            {
                agent.CanelBuff(buff.conflict_buff_type[i]);
            }
        }
        agent.buffRenderMgr.AddBuff(buff.buffType);
        InitEffect();
        return this;
    }

    //计算时间
    public virtual void _Update()
    {
        curTime += Time.deltaTime;
        curLoopTime += Time.deltaTime;
        if (curLoopTime >= trigger_time)
        {
            curLoopTime = 0;
            if (trigger_time > 0)
            {
                TriggerHurt();
            }
        }
        if (curTime >= duration)
        {
            curTime = 0;
            if(duration>0)
            RemoveBuff();
        }
    }

    /// <summary>
    /// 更新buff
    /// </summary>
    /// <param name="buff"></param>
    public void UpdateBuff(BuffData buff)
    {
        curLoopTimes = 0;

        if (buff.is_overly)
        {
            curTime -= buff.duration;
        }
        else
        {
            curTime = 0;
            duration = buff.duration;
        }
    
        trigger_time = buff.trigger_time;

        UpdateEffect();
    }
    public virtual void InitEffect()
    {
      
    }

    public virtual void UpdateEffect()
    {
       
    }


    public virtual void TriggerHurt()
    {

    }

    public virtual int TriggerBeHurt(int chp,AgentSkill enemySkill,HurtType hurtType)
    {
        return chp;
    }

    private void RemoveBuff()
    {
        agent.CanelBuff(buffType);
    }

    /// <summary>
    /// 移除buff
    /// </summary>
    /// <param name="buffType"></param>
    public virtual void RemoveBuffEffect()
    {
        agent.buffRenderMgr.RemoveBuff(buffType);
    }


}