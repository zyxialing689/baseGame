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
    public int skillId;


    public AgentSkill(int id,AIAgent agent)
    {
        this.skillId = id;
        this.agent = agent;
        this.Init();
    }

    protected virtual void Init()
    {

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