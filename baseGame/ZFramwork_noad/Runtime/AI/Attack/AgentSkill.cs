using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSkill
{
    public string path;
    public string anim_name;
    public string des;
    public Vector2 start_point;
    public Vector2 collider_size;
    public Vector2 transform_start;
    public Vector2 ground_check_offset;
    public Vector2 ground_check_size;
    public float hurt_cd;
    public bool isFlyPorp;
    public bool is_aoe;
    public bool ignore_sky_check;
    public bool ignore_ground_check;
    private const string animKey = "type";
    public float basic_anim_speed;
    public float sound_time;
    public string sound_path;
    public int multiple;
    public float fly_time;
    public int basic_potent;
    public bool ignore_collier;
    public bool focus_friend;
    public bool death_aoe;
    public bool is_ground_pos;
    public bool use_callBack;
    public float damage_bonus;
    public ClosestType search_type;
    public AgentSkill tailSkill;
    public List<BuffData> buffDatas;
    public AIAgent agent;
    public float extraParam;
    public int extraParam2;
    public float animLength;
    
    public AgentSkill(AIAgent agent)
    {
        this.agent = agent;
        //var table = ExcelConfig.Get_excel_roleskill(id);
        //anim_name = table.anim_name;
        //basic_anim_speed = table.basic_anim_speed;
        //path = table.path;
        //start_point = table.start_point.ToVector2();
        //collider_size = table.collider_size.ToVector2();
        //transform_start = table.transform_start.ToVector2();
        //isFlyPorp = table.fly_prop;
        //hurt_cd = table.hurt_cd;
        //des = table.des;
        //ignore_sky_check = table.ignore_sky_check;
        //ground_check_offset = table.ground_check_offset.ToVector2();
        //ground_check_size = table.ground_check_size.ToVector2();
        //is_aoe = table.is_aoe;
        //sound_path = table.sound_path;
        //multiple = table.multiple;
        //ignore_ground_check = table.ignore_ground_check;
        //fly_time = table.fly_time;
        //death_aoe = table.death_aoe;
        //damage_bonus = table.damage_bonus;
        //use_callBack = table.use_callBack;
        //buffDatas = new List<BuffData>();
        //for (int i = 0; i < table.buff.Count; i++)
        //{
        //    var buffTable = ExcelConfig.Get_excel_buffdata(table.buff[i]);
        //    buffDatas.Add(new BuffData(buffTable));
        //}
        //basic_potent = agent.agentData.basic_potent;
        //ignore_collier = table.ignore_collier;
        //if (agent.AngryState)
        //{
        //    focus_friend = false;
        //}
        //else
        //{
        //    focus_friend = table.focus_friend;
        //}


        //if (table.tail_skill > 10000)
        //{
        //    tailSkill = new AgentSkill(table.tail_skill,agent);
        //}
        //search_type = (ClosestType)table.search_type;
        //sound_time = table.sound_time;
        //is_ground_pos = table.is_ground_pos;
        //animLength = 0.1f;
    }

    public void SetAnim(Animator animator)
    {

        switch (anim_name)
        {
            case GameAnimationName.animationName_2_Attack_Normal:
                animator.SetInteger(animKey, 1);
                animator.SetFloat("speed", 0);
                break;
            case GameAnimationName.animationName_0_idle:
                animator.SetInteger(animKey, 0);
                animator.SetFloat("speed", 0);
                break;
            default:
                animator.SetInteger(animKey, 2);
                animator.SetFloat("speed", 0);
                break;
        }

    }

    public bool IsOpenDeathAoe()
    {
        return death_aoe && isFlyPorp;
    }

}