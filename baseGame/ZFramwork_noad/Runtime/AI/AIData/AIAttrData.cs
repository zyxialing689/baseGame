using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttrData
{
    public int hp;
    public int maxHp;
    public int defense;//护甲
    public float therapeuticFactor;//治疗系数
    private AIAgent agent;

    public AIAttrData(AIAgent agent)
    {
        hp = agent.agentData.max_hp;
        maxHp = hp;
        defense = 0;
        therapeuticFactor = 1;
        this.agent = agent;
    }
    public float GetHpPrecent()
    {
        return hp / ((float)maxHp);
    }

    public void SetMaxHp(int hp)
    {
        maxHp = hp;
    }
    
    public void SetHp(int hp)
    {
        this.hp = hp;
    }

    public void ChangeHp(int chp,AgentSkill agentSkill,Vector3 pos,HurtType hurtType = HurtType.SKILL,bool useBuff = true)
    {
        chp *= 10;

        if (agent.isDead&& chp<0||agent.unmatched)
        {
            return;
        }
        if (agentSkill != null)
        {
            chp = agent.TriggerBeHurt(chp, agentSkill, hurtType, useBuff);
        }
        if (chp != 0)
        {
            chp = CountHurt(chp);
            //DamageNumberMgr._instance.Spawn(pos, chp);
            hp += chp;
            if (hp > maxHp)
            {
                hp = maxHp;
            }
            if (hp < 0)
            {
                hp = 0;
            }
            UpdateUI();
        }

    }

    private void UpdateUI()
    {
        if (agent != null)
        {
            agent.UpdateHp(hp /(float)maxHp);
            if (hp == 0)
            {
                agent.TriggerDead();
            }
        }
    }


    private int CountHurt(int hurtX)
    {

        int hurtY = 0;
        if (hurtX > 0)
        {
            hurtY = Mathf.FloorToInt(therapeuticFactor * hurtX);
        }
        else
        {
            hurtY = -Mathf.FloorToInt(300f / (defense + 300f) * -hurtX);
            if (hurtY == 0)
            {
                hurtY = -1;
            }
        }

        return hurtY;
    }

}
