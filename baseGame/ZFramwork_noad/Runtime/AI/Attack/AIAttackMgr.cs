using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;


public class AIAttackMgr 
{
    /// <summary>
    /// 普通攻击
    /// </summary>
    /// <param name="playerTf"> 玩家transform </param>
    /// <param name="playerCamp">玩家阵营 </param>
    /// <param name="size">碰撞盒子大小</param>
    /// <param name="offset">碰撞盒子离玩家位置的偏移</param>
    /// <param name="pos">角色位置</param>
    /// <param name="hurtCD">同一个collider 碰撞后 触发伤害的cd</param>
    /// <param name="keepTime">最大存在时间</param>
    /// <param name="callBack">消失处理</param>
    /// <returns></returns>
    //public static List<AICollider> flyColliders = new List<AICollider>();
    public static void CreateAttackCollider(AgentSkill agentSkill,Action<AgentSkill,bool> callBack,float keepLoopTime = 0)
    {
        if (keepLoopTime > 0 && agentSkill.anim_name.Equals(GameAnimationName.animationName_2_Attack_Normal))
        {
            Debug.LogError("配置表有问题"+agentSkill.path+agentSkill.agent.name);
        }
        AIAgent agent = agentSkill.agent;
        switch (agentSkill.anim_name)
        {
            case GameAnimationName.animationName_5_Skill_Normal:
                agent.skillEndFunction = () => {
                    CreateSkill(agentSkill, callBack);
                };

                if (!agentSkill.use_callBack)
                {
                    agent.skillEndEndFunction = () => {
                        agent.skillEndEndFunction = null;
                        callBack?.Invoke(agentSkill,true);
                    };
                }

                agent.skillStartFunction = () => {
                    if (keepLoopTime > 0)
                    {
                        Timer.Register(keepLoopTime, () => {
                            if (agent != null)
                            {
                                agent.animator.SetInteger("type", 0);
                                //agent.skillStartFunction = null;
                                //if (!agentSkill.use_callBack)
                                //{
                                //    callBack?.Invoke(agentSkill, true);
                                //}
             
                            }
                        });
                    }
                    else
                    {
                        if (agent != null)
                        {
                            agent.animator.SetInteger("type", 0);
                            //if (!agentSkill.use_callBack)
                            //{
                            //    callBack?.Invoke(agentSkill, true);
                            //}
                        }
        
                    }
                };
        
                
                break;
            case GameAnimationName.animationName_2_Attack_Normal:
                agent.attackHalfFunction = () => {
                    CreateSkill(agentSkill,callBack);
                };
                agent.attackEndFunction = () => {
                    if (!agentSkill.use_callBack)
                    {
                        agent.animator.SetInteger("type", 0);
                        callBack?.Invoke(agentSkill, true);
                    }
                };
                break;

            default:
                CreateSkill(agentSkill,callBack);
                if (!agentSkill.use_callBack)
                {
                    callBack?.Invoke(agentSkill, true);
                }
                break;
        }



    }



    public static void CreateTailSkill(Vector3 pos,AIAgent selfAgent, AIAgent enemy,AgentSkill agentSkill)
    {
        if (agentSkill.multiple > 1)
        {
            AIAgent[] enemys = AIMgr.GetRandomAIAgents(selfAgent, agentSkill.multiple, agentSkill.search_type, agentSkill.focus_friend);
            if (!string.IsNullOrEmpty(agentSkill.sound_path))
            {
                AudioManager.GetInstance().PlayNewSound(agentSkill.sound_path, pos, agentSkill.sound_time > 0, agentSkill.sound_time);
            }
            for (int i = 0; i < enemys.Length; i++)
            {
                Vector3 targetPos = pos;
                if (agentSkill.is_ground_pos)
                {
                    targetPos = enemys[i].aICollider.GetGroundPos();
                }
                GameObject flyObj = ZGameObjectPool.Pop(agentSkill.path, () => { return PrefabUtils.Instance(agentSkill.path); });
                flyObj.SetActive(true);
                AICollider flyCollider = flyObj.GetComponent<AICollider>();
                var attackInst = flyObj.GetComponent<AttackInstance>();
                attackInst.SetTargetAgent(enemys[i]);
                attackInst.SetFllowTransform(selfAgent.aICollider);
                attackInst.SetFllowHeight(selfAgent.aICollider.tempZ);
                attackInst.SetTargetPos(targetPos);
                flyObj.transform.position = targetPos;

                flyCollider.playerCamp = selfAgent.playerCamp;
                AIBox skyBox = new AIBox(flyObj.transform, selfAgent.playerCamp,
                    selfAgent.aICollider.skyBox.size, selfAgent.aICollider.skyBox.offset,
                    flyObj.transform.position, 0, true);
                AIBox groundBox = new AIBox(flyObj.transform, selfAgent.playerCamp,
                    agentSkill.ground_check_size, agentSkill.ground_check_offset,
                    flyObj.transform.position, 0, true);
                float skillTime = agentSkill.hurt_cd;

                AIBox attackBox = new AIBox(flyObj.transform, selfAgent.playerCamp,
                agentSkill.collider_size, agentSkill.start_point,
                flyObj.transform.position, skillTime);
                flyCollider.FlyPropInit(skyBox, groundBox, attackBox);
                flyCollider.SetAttackInstance(attackInst);
                attackInst._OnStart(agentSkill);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(agentSkill.sound_path))
            {
                AudioManager.GetInstance().PlayNewSound(agentSkill.sound_path, pos, agentSkill.sound_time > 0, agentSkill.sound_time);
            }
            Vector3 targetPos = pos;
            if (agentSkill.is_ground_pos)
            {
                targetPos = enemy.aICollider.GetGroundPos();
            }
            GameObject flyObj = ZGameObjectPool.Pop(agentSkill.path, () => { return PrefabUtils.Instance(agentSkill.path); });
            flyObj.SetActive(true);
            AICollider flyCollider = flyObj.GetComponent<AICollider>();
            var attackInst = flyObj.GetComponent<AttackInstance>();
            attackInst.SetTargetAgent(enemy);
            attackInst.SetFllowTransform(selfAgent.aICollider);
            attackInst.SetFllowHeight(selfAgent.aICollider.tempZ);
            attackInst.SetTargetPos(targetPos);
            flyObj.transform.position = targetPos;

            flyCollider.playerCamp = selfAgent.playerCamp;
            AIBox skyBox = new AIBox(flyObj.transform, selfAgent.playerCamp,
                selfAgent.aICollider.skyBox.size, selfAgent.aICollider.skyBox.offset,
                flyObj.transform.position, 0, true);
            AIBox groundBox = new AIBox(flyObj.transform, selfAgent.playerCamp,
                agentSkill.ground_check_size, agentSkill.ground_check_offset,
                flyObj.transform.position, 0, true);
            float skillTime = agentSkill.hurt_cd;

            AIBox attackBox = new AIBox(flyObj.transform, selfAgent.playerCamp,
            agentSkill.collider_size, agentSkill.start_point,
            flyObj.transform.position, skillTime);
            flyCollider.FlyPropInit(skyBox, groundBox, attackBox);
            flyCollider.SetAttackInstance(attackInst);
            attackInst._OnStart(agentSkill);
        }
    }
    private static void CreateSkill(AgentSkill agentSkill, Action<AgentSkill, bool> callBack)
    {
        AIAgent agent = agentSkill.agent;
        if (agentSkill.multiple > 1)
        {
            AIAgent[] enemys = AIMgr.GetRandomAIAgents(agent, agentSkill.multiple, agentSkill.search_type, agentSkill.focus_friend);
            if (enemys.Length == 0)
            {
                agent.animator.SetInteger("type", 0);
                callBack?.Invoke(agentSkill,false);
                return;
            }
            if (!string.IsNullOrEmpty(agentSkill.sound_path))
            {
                AudioManager.GetInstance().PlayNewSound(agentSkill.sound_path, agent.transform.position, agentSkill.sound_time > 0, agentSkill.sound_time);
            }
            for (int i = 0; i < enemys.Length; i++)
            {
                Vector3 targetPos;
                if (agentSkill.is_ground_pos)
                {
                    targetPos = enemys[i].aICollider.GetGroundPos();
                }
                else
                {
                    targetPos = enemys[i].aICollider.GetHurtPos();
                }
                GameObject flyObj = ZGameObjectPool.Pop(agentSkill.path, () => { return PrefabUtils.Instance(agentSkill.path); });

                var flyCollider = flyObj.GetComponent<AICollider>();
                var attackInst = flyObj.GetComponent<AttackInstance>();
                attackInst.SetTargetAgent(enemys[i]);
                attackInst.SetFllowTransform(agent.aICollider);
                attackInst.SetFllowHeight(agent.aICollider.tempZ);
                attackInst.SetTargetPos(targetPos);
                Vector3 tempPos = agent.transform.position;
                flyObj.transform.position = tempPos;
                if (agent.transform.localScale.x > 0)
                {
                    flyObj.transform.position = new Vector3(tempPos.x - agentSkill.transform_start.x, tempPos.y + agentSkill.transform_start.y, 0);
                }
                else
                {
                    flyObj.transform.position = new Vector3(tempPos.x + agentSkill.transform_start.x, tempPos.y + agentSkill.transform_start.y, 0);
                }
                flyCollider.playerCamp = agent.playerCamp;
                AIBox skyBox = new AIBox(flyObj.transform, agent.playerCamp,
                    agent.aICollider.skyBox.size, agent.aICollider.skyBox.offset,
                    flyObj.transform.position, 0, agentSkill.ignore_sky_check);
                AIBox groundBox = new AIBox(flyObj.transform, agent.playerCamp,
                    agentSkill.ground_check_size, agentSkill.ground_check_offset,
                    flyObj.transform.position, 0, agentSkill.ignore_ground_check);
                float skillTime = agentSkill.hurt_cd;

                AIBox attackBox = new AIBox(flyObj.transform, agent.playerCamp,
                agentSkill.collider_size, agentSkill.start_point,
                flyObj.transform.position, skillTime);
                flyCollider.FlyPropInit(skyBox, groundBox, attackBox);
                flyCollider.SetAttackInstance(attackInst);
                attackInst._OnStart(agentSkill);
                flyObj.SetActive(true);
            }
        }
        else
        {
            if (agent.attackTarget == null)
            {
                agent.animator.SetInteger("type", 0);
                callBack?.Invoke(agentSkill, false);
                return;
            }
            if (!string.IsNullOrEmpty(agentSkill.sound_path))
            {
                AudioManager.GetInstance().PlayNewSound(agentSkill.sound_path, agent.transform.position, agentSkill.sound_time > 0, agentSkill.sound_time);
            }
            Vector3 targetPos;
            if (agentSkill.is_ground_pos)
            {
                targetPos = agent.attackTarget.aICollider.GetGroundPos();
            }
            else
            {
                targetPos = agent.attackTarget.aICollider.GetHurtPos();
            }
            GameObject flyObj = ZGameObjectPool.Pop(agentSkill.path, () => { return PrefabUtils.Instance(agentSkill.path); });

            var flyCollider = flyObj.GetComponent<AICollider>();
            var attackInst = flyObj.GetComponent<AttackInstance>();
            attackInst.SetTargetAgent(agent.attackTarget);
            attackInst.SetFllowTransform(agent.aICollider);
            attackInst.SetFllowHeight(agent.aICollider.tempZ);
            attackInst.SetTargetPos(targetPos);
            Vector3 tempPos = agent.transform.position;
            flyObj.transform.position = tempPos;
            if (agent.transform.localScale.x > 0)
            {
                flyObj.transform.position = new Vector3(tempPos.x - agentSkill.transform_start.x, tempPos.y + agentSkill.transform_start.y, 0);
            }
            else
            {
                flyObj.transform.position = new Vector3(tempPos.x + agentSkill.transform_start.x, tempPos.y + agentSkill.transform_start.y, 0);
            }
            flyCollider.playerCamp = agent.playerCamp;
            AIBox skyBox = new AIBox(flyObj.transform, agent.playerCamp,
                agent.aICollider.skyBox.size, agent.aICollider.skyBox.offset,
                flyObj.transform.position, 0, agentSkill.ignore_sky_check);
            AIBox groundBox = new AIBox(flyObj.transform, agent.playerCamp,
                agentSkill.ground_check_size, agentSkill.ground_check_offset,
                flyObj.transform.position, 0, agentSkill.ignore_ground_check);
            float skillTime = agentSkill.hurt_cd;

            AIBox attackBox = new AIBox(flyObj.transform, agent.playerCamp,
            agentSkill.collider_size, agentSkill.start_point,
            flyObj.transform.position, skillTime);
            flyCollider.FlyPropInit(skyBox, groundBox, attackBox);
            flyCollider.SetAttackInstance(attackInst);
            attackInst._OnStart(agentSkill);
            flyObj.SetActive(true);
        }

    }
}
