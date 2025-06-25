using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class BuffManager
{
    public static BuffManager _instance;
    
    public static BuffManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new BuffManager();
        }
        return _instance;
    }



    internal BaseBuff CreateBuff(AIAgent agent,BuffData buff)
    {
        BaseBuff baseBuff = null;
        switch (buff.buffType)
        {
            case BuffType.Frozen:
                baseBuff = new FrozenBuff().InitBuff(agent,buff);
                break;
            case BuffType.Bleeding:
                baseBuff = new BleedingBuff().InitBuff(agent, buff);
                break;
            case BuffType.Burning:
                baseBuff = new BurningBuff().InitBuff(agent, buff);
                break;
            case BuffType.Light:
                baseBuff = new LightBuff().InitBuff(agent, buff);
                break;
            case BuffType.PoorLess:
                baseBuff = new PoorLessBuff().InitBuff(agent, buff);
                break;  
            case BuffType.Toxin:
                baseBuff = new ToxinBuff().InitBuff(agent, buff);
                break; 
            case BuffType.Shadow:
                baseBuff = new ShadowBuff().InitBuff(agent, buff);
                break;
            case BuffType.Dizziness:
                baseBuff = new DizzinessBuff().InitBuff(agent, buff);
                break;
            case BuffType.VoidAvoidance:
                baseBuff = new VoidAvoidanceBuff().InitBuff(agent, buff);
                break;
            case BuffType.DamageReduction:
                baseBuff = new DamageReductionBuff().InitBuff(agent, buff);
                break;
            case BuffType.DamageShift:
                baseBuff = new DamageShiftBuff().InitBuff(agent, buff);
                break;
            case BuffType.DamageBack:
                baseBuff = new DamageBackBuff().InitBuff(agent, buff);
                break;
            case BuffType.DamageAdd:
                baseBuff = new DamageAdd().InitBuff(agent, buff);
                break;
            case BuffType.HealHp:
                baseBuff = new HealHpBuff().InitBuff(agent, buff);
                break;
            default:
                baseBuff = CreateImmunityBuff(agent, buff);
                break;
        }
        return baseBuff;
    }

    BaseBuff CreateImmunityBuff(AIAgent agent, BuffData buff)
    {
        BaseBuff baseBuff = null;
        switch (buff.buffType)
        {
            case BuffType.FrozenImmunity:
                baseBuff = new FrozenImmunityBuff().InitBuff(agent, buff);
                break;
            case BuffType.BleedingImmunity:
                baseBuff = new BleedingImmunityBuff().InitBuff(agent, buff);
                break;
            case BuffType.BurningImmunity:
                baseBuff = new BurningImmunityBuff().InitBuff(agent, buff);
                break;
            case BuffType.LightImmunity:
                baseBuff = new LightImmunityBuff().InitBuff(agent, buff);
                break;
            case BuffType.PoorLessImmunity:
                baseBuff = new PoorLessImmunityBuff().InitBuff(agent, buff);
                break;
            case BuffType.ToxinImmunity:
                baseBuff = new ToxinImmunityBuff().InitBuff(agent, buff);
                break;
            case BuffType.ShadowImmunity:
                baseBuff = new ShadowImmunityBuff().InitBuff(agent, buff);
                break;
            case BuffType.DizzinessImmunity:
                baseBuff = new DizzinessImmunityBuff().InitBuff(agent, buff);
                break;
        }
        return baseBuff;
    }
}
