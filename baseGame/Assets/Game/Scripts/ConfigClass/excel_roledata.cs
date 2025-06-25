using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using ZFramework;

namespace Table {
    [Serializable]
    public class excel_roledata : IConfig
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [XmlIgnore]
        public int id;
        [XmlAttribute("id")]
        public string _id {
            get { return id.ToString(); }
            set { if (string.IsNullOrEmpty(value)) id = 0; else id = int.Parse(value); }
        }

        /// <summary>
        /// 描述
        /// </summary>
        [XmlIgnore]
        public string des;
        [XmlAttribute("des")]
        public string _des {
            get { return des.ToString(); }
            set { if (string.IsNullOrEmpty(value)) des = ""; else des = value; }
        }

        /// <summary>
        /// 远程AI
        /// </summary>
        [XmlIgnore]
        public bool is_far_hero;
        [XmlAttribute("is_far_hero")]
        public string _is_far_hero {
            get { return is_far_hero.ToString(); }
            set { if (string.IsNullOrEmpty(value)) is_far_hero = false; else is_far_hero = bool.Parse(value); }
        }

        /// <summary>
        /// 最小基础攻击力
        /// </summary>
        [XmlIgnore]
        public int basic_potent;
        [XmlAttribute("basic_potent")]
        public string _basic_potent {
            get { return basic_potent.ToString(); }
            set { if (string.IsNullOrEmpty(value)) basic_potent = 0; else basic_potent = int.Parse(value); }
        }

        /// <summary>
        /// 吸血百分比
        /// </summary>
        [XmlIgnore]
        public float vampirism_value;
        [XmlAttribute("vampirism_value")]
        public string _vampirism_value {
            get { return vampirism_value.ToString(); }
            set { if (string.IsNullOrEmpty(value)) vampirism_value = 0; else vampirism_value = float.Parse(value); }
        }

        /// <summary>
        /// 是完整状态
        /// </summary>
        [XmlIgnore]
        public bool is_full;
        [XmlAttribute("is_full")]
        public string _is_full {
            get { return is_full.ToString(); }
            set { if (string.IsNullOrEmpty(value)) is_full = false; else is_full = bool.Parse(value); }
        }

        /// <summary>
        /// 攻击长度
        /// </summary>
        [XmlIgnore]
        public float attack_line;
        [XmlAttribute("attack_line")]
        public string _attack_line {
            get { return attack_line.ToString(); }
            set { if (string.IsNullOrEmpty(value)) attack_line = 0; else attack_line = float.Parse(value); }
        }

        /// <summary>
        /// 攻击id
        /// </summary>
        [XmlIgnore]
        public List<int> skill_ids;
        [XmlAttribute("skill_ids")]
        public string _skill_ids {
            get { return skill_ids.ToString(); }
            set{ if (string.IsNullOrEmpty(value)) skill_ids = new List<int>();else skill_ids = ZStringUtil.ArrayStringToIntList(value.Split('-'));}
        }

        /// <summary>
        /// 预制体路径
        /// </summary>
        [XmlIgnore]
        public string prefab_path;
        [XmlAttribute("prefab_path")]
        public string _prefab_path {
            get { return prefab_path.ToString(); }
            set { if (string.IsNullOrEmpty(value)) prefab_path = ""; else prefab_path = value; }
        }

        /// <summary>
        /// ai路径
        /// </summary>
        [XmlIgnore]
        public string ai_path;
        [XmlAttribute("ai_path")]
        public string _ai_path {
            get { return ai_path.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ai_path = ""; else ai_path = value; }
        }

        /// <summary>
        /// 最大血量
        /// </summary>
        [XmlIgnore]
        public int max_hp;
        [XmlAttribute("max_hp")]
        public string _max_hp {
            get { return max_hp.ToString(); }
            set { if (string.IsNullOrEmpty(value)) max_hp = 0; else max_hp = int.Parse(value); }
        }

        /// <summary>
        /// 动画路径
        /// </summary>
        [XmlIgnore]
        public string anim_path;
        [XmlAttribute("anim_path")]
        public string _anim_path {
            get { return anim_path.ToString(); }
            set { if (string.IsNullOrEmpty(value)) anim_path = ""; else anim_path = value; }
        }

        /// <summary>
        /// 自带buff
        /// </summary>
        [XmlIgnore]
        public List<int> self_buffs;
        [XmlAttribute("self_buffs")]
        public string _self_buffs {
            get { return self_buffs.ToString(); }
            set{ if (string.IsNullOrEmpty(value)) self_buffs = new List<int>();else self_buffs = ZStringUtil.ArrayStringToIntList(value.Split('-'));}
        }

        /// <summary>
        /// 移动速度
        /// </summary>
        [XmlIgnore]
        public float move_speed;
        [XmlAttribute("move_speed")]
        public string _move_speed {
            get { return move_speed.ToString(); }
            set { if (string.IsNullOrEmpty(value)) move_speed = 0; else move_speed = float.Parse(value); }
        }

        /// <summary>
        /// 预制体渲染图高度（只针对地升动画）
        /// </summary>
        [XmlIgnore]
        public SerV2 render_pos;
        [XmlAttribute("render_pos")]
        public string _render_pos {
            get { return render_pos.ToString(); }
            set { if (string.IsNullOrEmpty(value)) render_pos = new SerV2(); else render_pos = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 表情父亲位置
        /// </summary>
        [XmlIgnore]
        public SerV2 emoji_pos;
        [XmlAttribute("emoji_pos")]
        public string _emoji_pos {
            get { return emoji_pos.ToString(); }
            set { if (string.IsNullOrEmpty(value)) emoji_pos = new SerV2(); else emoji_pos = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 特效父亲位置
        /// </summary>
        [XmlIgnore]
        public SerV2 effect_pos;
        [XmlAttribute("effect_pos")]
        public string _effect_pos {
            get { return effect_pos.ToString(); }
            set { if (string.IsNullOrEmpty(value)) effect_pos = new SerV2(); else effect_pos = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 空中高度
        /// </summary>
        [XmlIgnore]
        public float sky_height;
        [XmlAttribute("sky_height")]
        public string _sky_height {
            get { return sky_height.ToString(); }
            set { if (string.IsNullOrEmpty(value)) sky_height = 0; else sky_height = float.Parse(value); }
        }

        /// <summary>
        /// ui高度
        /// </summary>
        [XmlIgnore]
        public SerV2 ui_hp_offset;
        [XmlAttribute("ui_hp_offset")]
        public string _ui_hp_offset {
            get { return ui_hp_offset.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ui_hp_offset = new SerV2(); else ui_hp_offset = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 血条大小
        /// </summary>
        [XmlIgnore]
        public SerV2 ui_hp_size;
        [XmlAttribute("ui_hp_size")]
        public string _ui_hp_size {
            get { return ui_hp_size.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ui_hp_size = new SerV2(); else ui_hp_size = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 空中碰撞体大小
        /// </summary>
        [XmlIgnore]
        public SerV2 sky_size;
        [XmlAttribute("sky_size")]
        public string _sky_size {
            get { return sky_size.ToString(); }
            set { if (string.IsNullOrEmpty(value)) sky_size = new SerV2(); else sky_size = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 身体碰撞体偏移
        /// </summary>
        [XmlIgnore]
        public SerV2 body_offset;
        [XmlAttribute("body_offset")]
        public string _body_offset {
            get { return body_offset.ToString(); }
            set { if (string.IsNullOrEmpty(value)) body_offset = new SerV2(); else body_offset = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 身体碰撞体大小
        /// </summary>
        [XmlIgnore]
        public SerV2 body_size;
        [XmlAttribute("body_size")]
        public string _body_size {
            get { return body_size.ToString(); }
            set { if (string.IsNullOrEmpty(value)) body_size = new SerV2(); else body_size = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 身体碰撞体大小
        /// </summary>
        [XmlIgnore]
        public SerV2 ground_offset;
        [XmlAttribute("ground_offset")]
        public string _ground_offset {
            get { return ground_offset.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ground_offset = new SerV2(); else ground_offset = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 身体碰撞体大小
        /// </summary>
        [XmlIgnore]
        public SerV2 ground_size;
        [XmlAttribute("ground_size")]
        public string _ground_size {
            get { return ground_size.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ground_size = new SerV2(); else ground_size = ZStringUtil.StringToSerV2(value); }
        }

        public List<T> LoadBytes<T>()
        {
            string bytesPath = "Assets/Game/AssetDynamic/ConfigBytes/excel_roledata";
             TextAsset asset = TextAssetUtils.GetTextAsset(bytesPath);
            List<T> excel_roledatas = DeserializeData<T>(asset);
            return excel_roledatas;
        }

         private List<T> DeserializeData<T>(UnityEngine.TextAsset textAsset)
        {
            using (MemoryStream ms = new MemoryStream(textAsset.bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                allexcel_roledata table = formatter.Deserialize(ms) as allexcel_roledata;
                Config<excel_roledata>.AddExcelToDic(typeof(T).Name, table.excel_roledatas);
                return table.excel_roledatas  as List<T>;
            }
        }
    }

    [Serializable]
    public class allexcel_roledata
    {
        public List<excel_roledata> excel_roledatas;
    }
}
