using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentData
{
    public int role_id;
    public string ai_path;
    public string anim_path;
    public PlayerCamp playerCamp;
    public Vector2 ui_hp_offset;
    public Vector2 ui_hp_size;
    public Vector2 sky_size;
    public Vector2 body_offset;
    public Vector2 body_size;
    public Vector2 ground_offset;
    public Vector2 ground_size;
    public float sky_height;
    public float move_speed;
    public Vector3 render_pos;
    public Vector3 emoji_pos;
    public Vector3 effect_pos;
    public int max_hp;
    public float attack_line;
    public float vampirism_value;
    public List<BuffData> self_buffs;
    public List<int> skill_ids;
    public int basic_potent;
    public bool is_far_hero;
    public AgentData(int id)
    {
        this.role_id = id;
        this.Init();
    }

    protected virtual void Init()
    {

    }

    public bool IsVampirism()
    {
        return vampirism_value>0;
    }

    public bool isSkyRole()
    {
        return sky_height > 0;
    }
}