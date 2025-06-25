using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using Random = UnityEngine.Random;

public class AttackInstance : MonoBehaviour
{
    public int attackSort = 0;
    [HideInInspector]
    public AICollider aiCollider;
    [HideInInspector]
    public AICollider fllowCollider;
    [HideInInspector]
    public AIAgent targetAgent;
    [HideInInspector]
    public Transform virtualBody;
    [HideInInspector]
    public Vector3 targetPos;
    [HideInInspector]
    public AgentSkill agentSkill;
    [HideInInspector]
    public float height;
    [HideInInspector]
    public float animLeftTime;

    public float curTime;
    [HideInInspector]
    public float targetHeight;
    [HideInInspector]
    public float targetAngleHeight;

    private List<AICollider> inAreaAIColliders;

    protected bool isFlyPorp = false;
    [HideInInspector]
    public bool triggerOnce = false;
    [Header("出生后检测一次碰撞，勾选后，性能大幅度提升（持续性技能请关闭）")]
    public bool onceCheck = true;
    [Header("出生后检测到碰撞后就不在检测，（持续性技能关闭，其他必选）")]
    public bool onceColliderCheck = false;
    [HideInInspector]
    public bool _onceCheck = true;
    [HideInInspector]
    public bool _onceColliderCheck = true;
    public virtual void Init()
    {
        aiCollider.SetVirtualAngle(0);
    }
    public void _OnStart(AgentSkill agentSkill)
    {
        _onceCheck = true;
        _onceColliderCheck = true;
        triggerOnce = true;
        curTime = 0;
        this.animLeftTime = agentSkill.animLength;
        this.agentSkill = agentSkill;
        virtualBody = transform.GetChild(0);
        aiCollider = GetComponent<AICollider>();
        if (agentSkill.focus_friend)
        {
            aiCollider.playerCamp = PlayerCamp.PlayerCampA == aiCollider.playerCamp ? PlayerCamp.PlayerCampA : PlayerCamp.PlayerCampB;
        }
        if (agentSkill.use_callBack)
        {
            if(agentSkill.agent!=null)
            agentSkill.agent.agentTempData.isAttacking = true;
        }
        aiCollider.attackSort = attackSort;
        aiCollider.ClearHurtCDMap();
        this.isFlyPorp = agentSkill.isFlyPorp;

        Init();
        AttackMgr.AddAttackInstance(this);
    }
    public void _OnDestroy()
    {
        AttackMgr.RemoveAttackInstance(this);
    }

    // Update is called once per frame
    public virtual void _FixedUpdate()
    {
        if (isFlyPorp)
        {
            aiCollider.UpdateAIClollider(transform.position.x, transform.position.y, transform.position.z);
        }
        else
        {
            if (fllowCollider != null)
            {
                transform.localScale = fllowCollider.transform.localScale;
                aiCollider.UpdateAIClollider(fllowCollider.tempX, fllowCollider.tempY, fllowCollider.tempZ);

                if (transform.localScale.x > 0)
                {
                    transform.position = aiCollider.groundBox.pos - agentSkill.transform_start;
                }
                else
                {
                    transform.position = aiCollider.groundBox.pos + agentSkill.transform_start;
                }
            }
        }

    }
    // Update is called once per frame
    public virtual void _Update()
    {
        curTime += Time.deltaTime;
        if (isFlyPorp)
        {
            if (curTime > agentSkill.fly_time)
            {
                ReadyDestroy();
            }
        }
        else
        {
            if (curTime > animLeftTime)
            {
                ReadyDestroy();
            }
        }

    }



    public virtual void ReadyDestroy()
    {
        if (agentSkill.use_callBack)
        {
            if (agentSkill.agent != null)
                agentSkill.agent.agentTempData.isAttacking = false;
        }
        SpawnNewAttackInstance();
        if (inAreaAIColliders != null)
        {
            foreach (var item in inAreaAIColliders)
            {
               OutArea(item);
            }
            inAreaAIColliders.Clear();
        }
        _OnDestroy();
        curTime = 0;
        ZGameObjectPool.Push(agentSkill.path, gameObject);
    }

    public virtual void SpawnNewAttackInstance()
    {
        if (agentSkill.tailSkill!=null)
        {
            AIAttackMgr.CreateTailSkill(transform.position, fllowCollider.agent, targetAgent, agentSkill.tailSkill);
        }
    }

    public virtual void  SetAttackEffect(AICollider enemyCollider)
    {
        if (enemyCollider.agent != null && enemyCollider.agent.attrData != null)
        {
            int hurt = GetDamage();
            enemyCollider.agent.attrData.ChangeHp(hurt, agentSkill,enemyCollider.GetHurtPos());
            //DamageNumberMgr._instance.Spawn(enemyCollider, hurt);
        }
    }

    public virtual void TrigerOnceAll(AICollider[] aIColliders)
    {

    }
    internal void SetTargetAgent(AIAgent enemyAgent)
    {
        targetAgent = enemyAgent;
    }
    internal void SetFllowTransform(AICollider tf)
    {
        fllowCollider = tf;
        if (targetAgent == null && fllowCollider != null&&fllowCollider.agent.attackTarget!=null)
        {
            targetAgent = fllowCollider.agent.attackTarget;
        }
    }
    internal void SetFllowHeight(float height)
    {
        this.height = height;
    }

    internal void SetTargetPos(Vector3 targetPos)
    {
        this.targetPos = targetPos;
    }
    internal void SetTargetHeight(float targetHeight)
    {
        this.targetHeight = targetHeight;
    }
    internal void SetTargetAngleHeight(float targetAngleHeight)
    {
        this.targetAngleHeight = targetAngleHeight;
    }
    public void JudgeInArea(AICollider[] aIColliders)
    {
        if (inAreaAIColliders == null)
        {
            inAreaAIColliders = new List<AICollider>();
        }
        for (int i = 0; i < aIColliders.Length; i++)
        {
            if (!inAreaAIColliders.Contains(aIColliders[i]))
            {
                inAreaAIColliders.Add(aIColliders[i]);
            }
            InArea(aIColliders[i]);
        }
        InArea(aIColliders);
    }
    internal void JudgeOutArea(AICollider[] aIColliders)
    {
        if (inAreaAIColliders == null)
        {
            return;
        }
        foreach (var item in inAreaAIColliders)
        {
            for (int i = 0; i < aIColliders.Length; i++)
            {
                if (aIColliders[i] == item)
                {
                    continue;
                }
            }
            OutArea(item);
        }
    }
    public virtual void InArea(AICollider aICollider)
    {

    }
    public virtual void InArea(AICollider[] aIColliders)
    {

    }

    public virtual void OutArea(AICollider aICollider)
    {

    }

    public bool IsSafeFllower()
    {
        return fllowCollider != null && !fllowCollider.agent.trueDeath;
    }

    public bool IsSafeTarget()
    {
        return targetAgent != null && !targetAgent.trueDeath;
    }
    
    public bool IsSafe()
    {
        return IsSafeFllower() && IsSafeTarget();
    }

    public virtual int GetDamage()
    {
        if (agentSkill.focus_friend)
        {
            return Mathf.CeilToInt(agentSkill.basic_potent * (1 + agentSkill.damage_bonus));
        }
        else
        {
            return -Mathf.CeilToInt(agentSkill.basic_potent * (1 + agentSkill.damage_bonus));
        }

    }

}
