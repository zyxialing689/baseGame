
public class BuffData
{
    public BuffType buffType;
    public BuffType[] conflict_buff_type;
    public float duration;
    public float trigger_time;
    public string des;
    public string effect_path;
    public int id;
    public bool trigger_beHurt;
    public bool is_overly;

    public BuffData()
    {
        //if (buffTable == null)
        //{
        //    this.buffType = BuffType.None;
        //    return;
        //}
        //var list = buffTable.conflict_buff_type.ToArray();
        //this.buffType = (BuffType)buffTable.id;
        //this.id = buffTable.id;
        //this.des = buffTable.des;
        //this.duration = buffTable.duration;
        //this.effect_path = buffTable.effect_path;
        //this.trigger_time = buffTable.trigger_time;
        //this.conflict_buff_type = new BuffType[list.Length];
        //this.trigger_beHurt = buffTable.trigger_beHurt;
        //this.is_overly = buffTable.is_overly;
        //for (int i = 0; i < list.Length; i++)
        //{
        //    this.conflict_buff_type[i] = (BuffType)list[i];
        //}
    }

    public bool IsImmunityBuff()
    {
        if (this.id > 1000)
        {
            return true;
        }
        return false;
    }
}