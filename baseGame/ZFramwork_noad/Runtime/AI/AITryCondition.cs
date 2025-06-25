using System;
using System.Collections.Generic;
using UnityEngine;

//GUILayout.BeginHorizontal();
//GUILayout.Label("(reset)Ñ²Âß´ÎÊý");
//tempAIStateData.patrolTimes = GUILayout.TextField(tempAIStateData.patrolTimes);
//GUILayout.EndHorizontal();
public delegate bool TryCondition(AIStateData aIStateData,Vector4 timeData, Vector4 nextTime);
public class AITryCondition
{
    public Dictionary<CondType,TryCondition> tryConditions;
    private AIAgent agent;
    bool tempBool;
    private float nextTime;
    public AITryCondition(AIAgent agent)
    {
        this.agent = agent;
        tryConditions = new Dictionary<CondType, TryCondition>();
        tryConditions.Add(CondType.None, TryCondition_None);
        tryConditions.Add(CondType.canBeAttack__dis, TryCondition_CanBeAttack);
        tryConditions.Add(CondType.canNotBeAttack__dis, TryCondition_canNotBeAttack__dis);
        tryConditions.Add(CondType.skyCanBeAttack__dis, TryCondition_skyCanBeAttack__dis);
        tryConditions.Add(CondType._m_keepTime__time, TryCondition_KeepTime);
        tryConditions.Add(CondType._d_randKeepTime__time, TryCondition_RandKeepTime);
        tryConditions.Add(CondType._t_patrolTimes__times, TryCondition_patrolTimes);
        tryConditions.Add(CondType._f_canFarBeAttack__dis, TryCondition_canFarBeAttack);
        tryConditions.Add(CondType._f_canNotFarBeAttack__dis, TryCondition__f_canNotFarBeAttack__dis);
        tryConditions.Add(CondType.haveNoEnemy__enemy, TryCondition_haveNoEnemy);
        tryConditions.Add(CondType.haveEnemy__enemy, TryCondition_haveEnemy);
        tryConditions.Add(CondType.haveTargetEnemy__enemy, TryCondition_haveTargetEnemy__enemy);
        tryConditions.Add(CondType.haveNoTargetEnemy__enemy, TryCondition_haveNoTargetEnemy__enemy);
        tryConditions.Add(CondType._h_lowHp__hp, TryCondition_h_lowHp);
        tryConditions.Add(CondType._h_highHp__hp, TryCondition_h_highHp);
        tryConditions.Add(CondType._h_enemyLowHp__hp, TryCondition_h_enemyLowHp__hp);
        tryConditions.Add(CondType._h_enemyHeighHp__hp, TryCondition_h_enemyHeighHp__hp);
        tryConditions.Add(CondType.haveSkyEnemy__enemy, TryCondition_haveSkyEnemy);
        tryConditions.Add(CondType.haveNoSkyEnemy__enemy, TryCondition_haveNoSkyEnemy);
        tryConditions.Add(CondType.haveGroundEnemy__enemy, TryCondition_haveGroundEnemy);
        tryConditions.Add(CondType.haveNoGroundEnemy__enemy, TryCondition_haveNoGroundEnemy);
        tryConditions.Add(CondType._m_outCdTime__time, TryCondition_m_outCdTime__time);
        tryConditions.Add(CondType._m_intCdTime__time, TryCondition_m_intCdTime__time);
        tryConditions.Add(CondType._m_outGlobalCdTime__time, TryCondition_m_outGlobalCdTime__time);
        tryConditions.Add(CondType._m_intGlobalCdTime__time, TryCondition_m_intGlobalCdTime__time);
        tryConditions.Add(CondType._m_birthOver__time, TryCondition__m_birthOver__time);
        tryConditions.Add(CondType._m_birthNotOver__time, TryCondition_m_birthNotOver__time);
        tryConditions.Add(CondType.isAttacking__attack, TryCondition_isAttacking);
        tryConditions.Add(CondType.isNotAttacking__attack, TryCondition_isNotAttacking);
        tryConditions.Add(CondType._o_isLeader__other, TryCondition_o_isLeader__other);
        tryConditions.Add(CondType._o_isNotLeader__other, TryCondition_o_isNotLeader__other);
        tryConditions.Add(CondType._m_disLeader__dis, TryCondition_o_disLeader__other);
        tryConditions.Add(CondType.haveOtherFriend__enemy, TryCondition_haveOtherFriend__enemy);
        tryConditions.Add(CondType.haveNoOtherFriend__enemy, TryCondition_haveNoOtherFriend__enemy);
        tryConditions.Add(CondType._m_keepLifeTime__time, TryCondition_m_keepLifeTime__time);
        tryConditions.Add(CondType.IsSelf__enemy, TryCondition_IsSelf__enemy);
        tryConditions.Add(CondType.IsNotSelf__enemy, TryCondition_IsNotSelf__enemy);
        tryConditions.Add(CondType.findStop__other, TryCondition_findStop__other);
    }

    public bool GetTryCondition(List<List<CondType>> condTypes, AIStateData aIStateData)
    {
        for (int i = 0; i < condTypes.Count; i++)
        {
            if (GetTryCondition(condTypes[i],aIStateData.condTypeParam[i],aIStateData.nextParamTime[i], aIStateData))
            {
                return true;
            }
        }
        return false;
    }

    private bool GetTryCondition(List<CondType> condTypes,List<Vector4> condParams, List<Vector4> nextTimes, AIStateData aIStateData)
    {
        for (int i = 0; i < condTypes.Count; i++)
        {
            if (!tryConditions[condTypes[i]](aIStateData, condParams[i], nextTimes[i]))
            {
                return false;
            }
        }
        return true;
    }

    private bool TryCondition_None(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return true;
    }
    
    private bool TryCondition_CanBeAttack(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        tempBool = false;
        if (agent.attackTarget != null)
        {
            tempBool = AICondition.IsCanAttack(agent.aICollider, agent.attackTarget.aICollider);
        }
        return tempBool;
    }
    private bool TryCondition_canNotBeAttack__dis(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        tempBool = false;
        if (agent.attackTarget != null)
        {
            tempBool = !AICondition.IsCanAttack(agent.aICollider, agent.attackTarget.aICollider);
        }
        return tempBool;
    }
    private bool TryCondition_skyCanBeAttack__dis(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        tempBool = false;
        if (agent.attackTarget != null)
        {
            tempBool = AICondition.IsCanAttackNoSky(agent.aICollider, agent.attackTarget.aICollider);
        }
        return tempBool;
    }

    private bool TryCondition_KeepTime(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        if (floatData.x <= 0)
        {
            return false;
        }
        return agent.curTime > nextTime.x;
    }
    
    private bool TryCondition_RandKeepTime(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.curTime > nextTime.x;
    }

    private bool TryCondition_patrolTimes(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {

        return agent.agentTempData.patrolTimes>aIStateData.patrolTimes;
    }
    private bool TryCondition_canFarBeAttack(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        tempBool = false;
        floatData *= 1000;
        if (agent.attackTarget != null)
        {
            tempBool = AICondition.IsCanFarAttack(agent.aICollider, agent.attackTarget.aICollider, floatData.x,floatData.y);
        }
        return tempBool;
    }
    private bool TryCondition__f_canNotFarBeAttack__dis(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        tempBool = false;
        floatData *= 1000;
        if (agent.attackTarget != null)
        {
            tempBool = !AICondition.IsCanFarAttack(agent.aICollider, agent.attackTarget.aICollider, floatData.x,floatData.y);
        }
        return tempBool;
    }
    private bool TryCondition_haveNoEnemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return !AIMgr.HaveEmenyAIAgent(agent.playerCamp);
    }
    private bool TryCondition_haveEnemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return AIMgr.HaveEmenyAIAgent(agent.playerCamp);
    }
    private bool TryCondition_haveTargetEnemy__enemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.attackTarget != null;
    }
    private bool TryCondition_haveNoTargetEnemy__enemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.attackTarget == null;
    }
    private bool TryCondition_haveSkyEnemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return AIMgr.HaveSkyEmenyAIAgent(agent.playerCamp);
    }
    private bool TryCondition_haveNoSkyEnemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return !AIMgr.HaveSkyEmenyAIAgent(agent.playerCamp);
    }
    private bool TryCondition_haveGroundEnemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return AIMgr.HaveGroundEmenyAIAgent(agent.playerCamp);
    }
    private bool TryCondition_haveNoGroundEnemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return !AIMgr.HaveGroundEmenyAIAgent(agent.playerCamp);
    }
    private bool TryCondition_m_outCdTime__time(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        int id = (int)(floatData.x*1000);
        if (agent.agentTempData.cdMap.ContainsKey(id))
        {
            return agent.agentTempData.cdMap[id] < agent.curTime;
        }
        return true;
    }
    private bool TryCondition_m_outGlobalCdTime__time(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        int id = (int)(floatData.x * 1000);
        if (agent.agentTempData.globalCdMap.ContainsKey(id))
        {
            return agent.agentTempData.globalCdMap[id].time < agent.curTime;
        }

        return true;
    }
    private bool TryCondition_m_intCdTime__time(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        int id = (int)(floatData.x*1000);
        if (agent.agentTempData.cdMap.ContainsKey(id))
        {
            return agent.agentTempData.cdMap[id] >= agent.curTime;
        }
        return false;
    }
    private bool TryCondition_m_intGlobalCdTime__time(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        int id = (int)(floatData.x * 1000);
        if (agent.agentTempData.globalCdMap.ContainsKey(id))
        {
            return agent.agentTempData.globalCdMap[id].time > agent.curTime;
        }

        return false;
    }
    private bool TryCondition__m_birthOver__time(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.curTime > floatData.x*1000;
    }
    private bool TryCondition_m_birthNotOver__time(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.curTime <= floatData.x*1000;
    }
    private bool TryCondition_isAttacking(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.agentTempData.isAttacking;
    }
    private bool TryCondition_isNotAttacking(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return !agent.agentTempData.isAttacking;
    }
    private bool TryCondition_o_isLeader__other(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.team.leader==agent;
    }
    private bool TryCondition_o_isNotLeader__other(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.team.leader!=agent;
    }
    private bool TryCondition_o_disLeader__other(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        bool tempBool = false;
        if (agent.team.leader != null)
        {
            tempBool = Vector3.Distance(agent.team.leader.transform.position, agent.transform.position) > floatData.x * 1000;
        }
        return tempBool;
    }
    private bool TryCondition_h_lowHp(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        float percent = floatData.x * 10;
        float curPrcent = agent.attrData.GetHpPrecent();
        return curPrcent < percent;
    }
    private bool TryCondition_h_highHp(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        float percent = floatData.x * 10;
        return agent.attrData.GetHpPrecent() >= percent;
    }
    private bool TryCondition_h_enemyLowHp__hp(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        float percent = floatData.x * 10;
        return agent.attackTarget!=null?agent.attackTarget.attrData.GetHpPrecent() < 2:false;
    }
    private bool TryCondition_h_enemyHeighHp__hp(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        float percent = floatData.x * 10;
        return agent.attackTarget != null ? agent.attackTarget.attrData.GetHpPrecent() >= percent:false;
    }
    ///
    private bool TryCondition_haveOtherFriend__enemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return AIMgr.GetCampCount(agent.playerCamp) > 1;
    }
    private bool TryCondition_haveNoOtherFriend__enemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return AIMgr.GetCampCount(agent.playerCamp) <= 1;
    } 
    private bool TryCondition_m_keepLifeTime__time(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.curTime - agent.agentTempData.aiTotalTime >   floatData.x*1000;
    }
    private bool TryCondition_IsSelf__enemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.attackTarget == agent;
    }
    private bool TryCondition_IsNotSelf__enemy(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.attackTarget != agent;
    }
    private bool TryCondition_findStop__other(AIStateData aIStateData, Vector4 floatData, Vector4 nextTime)
    {
        return agent.agentTempData.findStop;
    }
}
