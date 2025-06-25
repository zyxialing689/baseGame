using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentData
{
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
        is_far_hero = true;
        if(RandomMgr.GetValue()>0.5){
        playerCamp = PlayerCamp.PlayerCampA;
        }else{
        playerCamp = PlayerCamp.PlayerCampB;
        }

        ai_path = "Assets/Game/AssetDynamic/Config/AI/archer2_t1.2";
        self_buffs = new List<BuffData>();
        move_speed = 1;
        //var table = ExcelConfig.Get_excel_roledata(id);

        ////playerCamp = (PlayerCamp)table.player_camp;
        //ai_path = table.ai_path;
        //ui_hp_offset = table.ui_hp_offset.ToVector2();
        //ui_hp_size = table.ui_hp_size.ToVector2();
        //anim_path = table.anim_path;
        //sky_height = table.sky_height;
        //move_speed = table.move_speed;
        //sky_size = table.sky_size.ToVector2();
        //body_offset = table.body_offset.ToVector2();
        //body_size = table.body_size.ToVector2();
        //ground_offset = table.ground_offset.ToVector2();
        //ground_size = table.ground_size.ToVector2();
        //render_pos = table.render_pos.ToVector2();
        //max_hp = table.max_hp;
        //attack_line = table.attack_line;
        //emoji_pos = table.emoji_pos.ToVector2();
        //effect_pos = table.effect_pos.ToVector2();
        //basic_potent = table.basic_potent;
        //vampirism_value = table.vampirism_value;
        //skill_ids = table.skill_ids;
        //self_buffs = new List<BuffData>();
        //is_far_hero = table.is_far_hero;
        //for (int i = 0; i < table.self_buffs.Count; i++)
        //{
        //    var buffTable = ExcelConfig.Get_excel_buffdata(table.self_buffs[i]);
        //    var buffData = new BuffData(buffTable);
        //    self_buffs.Add(buffData);
        //}

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