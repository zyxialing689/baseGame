using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentDataNew : AgentData
{
    public AgentDataNew(int id) : base(id) { }
    protected override void Init()
    {
        var table = ExcelConfig.Get_excel_roledata(this.role_id);
        //playerCamp = (PlayerCamp)table.player_camp;
        ai_path = table.ai_path;
        ui_hp_offset = table.ui_hp_offset.ToVector2();
        ui_hp_size = table.ui_hp_size.ToVector2();
        anim_path = table.anim_path;
        sky_height = table.sky_height;
        move_speed = table.move_speed;
        sky_size = table.sky_size.ToVector2();
        body_offset = table.body_offset.ToVector2();
        body_size = table.body_size.ToVector2();
        ground_offset = table.ground_offset.ToVector2();
        ground_size = table.ground_size.ToVector2();
        render_pos = table.render_pos.ToVector2();
        max_hp = table.max_hp;
        attack_line = table.attack_line;
        emoji_pos = table.emoji_pos.ToVector2();
        effect_pos = table.effect_pos.ToVector2();
        basic_potent = table.basic_potent;
        vampirism_value = table.vampirism_value;
        skill_ids = table.skill_ids;
        self_buffs = new List<BuffData>();
        is_far_hero = table.is_far_hero;
        //for (int i = 0; i < table.self_buffs.Count; i++)
        //{
        //    var buffTable = ExcelConfig.Get_excel_buffdata(table.self_buffs[i]);
        //    var buffData = new BuffData(buffTable);
        //    self_buffs.Add(buffData);
        //}
    }
}
