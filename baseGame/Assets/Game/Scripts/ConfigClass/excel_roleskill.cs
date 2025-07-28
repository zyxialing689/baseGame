using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using ZFramework;

namespace Table {
    [Serializable]
    public class excel_roleskill : IConfig
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
        /// 伤害加成
        /// </summary>
        [XmlIgnore]
        public float damage_bonus;
        [XmlAttribute("damage_bonus")]
        public string _damage_bonus {
            get { return damage_bonus.ToString(); }
            set { if (string.IsNullOrEmpty(value)) damage_bonus = 0; else damage_bonus = float.Parse(value); }
        }

        /// <summary>
        /// 针对友军
        /// </summary>
        [XmlIgnore]
        public bool focus_friend;
        [XmlAttribute("focus_friend")]
        public string _focus_friend {
            get { return focus_friend.ToString(); }
            set { if (string.IsNullOrEmpty(value)) focus_friend = false; else focus_friend = bool.Parse(value); }
        }

        /// <summary>
        /// 尾巴技能
        /// </summary>
        [XmlIgnore]
        public int tail_skill;
        [XmlAttribute("tail_skill")]
        public string _tail_skill {
            get { return tail_skill.ToString(); }
            set { if (string.IsNullOrEmpty(value)) tail_skill = 0; else tail_skill = int.Parse(value); }
        }

        /// <summary>
        /// 0距离1x2y3血百分比4血量5随机
        /// </summary>
        [XmlIgnore]
        public int search_type;
        [XmlAttribute("search_type")]
        public string _search_type {
            get { return search_type.ToString(); }
            set { if (string.IsNullOrEmpty(value)) search_type = 0; else search_type = int.Parse(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool is_ground_pos;
        [XmlAttribute("is_ground_pos")]
        public string _is_ground_pos {
            get { return is_ground_pos.ToString(); }
            set { if (string.IsNullOrEmpty(value)) is_ground_pos = false; else is_ground_pos = bool.Parse(value); }
        }

        /// <summary>
        /// 实例化多个
        /// </summary>
        [XmlIgnore]
        public int multiple;
        [XmlAttribute("multiple")]
        public string _multiple {
            get { return multiple.ToString(); }
            set { if (string.IsNullOrEmpty(value)) multiple = 0; else multiple = int.Parse(value); }
        }

        /// <summary>
        /// 使用技能回调，不走动画回调
        /// </summary>
        [XmlIgnore]
        public bool use_callBack;
        [XmlAttribute("use_callBack")]
        public string _use_callBack {
            get { return use_callBack.ToString(); }
            set { if (string.IsNullOrEmpty(value)) use_callBack = false; else use_callBack = bool.Parse(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public float fly_time;
        [XmlAttribute("fly_time")]
        public string _fly_time {
            get { return fly_time.ToString(); }
            set { if (string.IsNullOrEmpty(value)) fly_time = 0; else fly_time = float.Parse(value); }
        }

        /// <summary>
        /// 是否为飞行道具
        /// </summary>
        [XmlIgnore]
        public bool fly_prop;
        [XmlAttribute("fly_prop")]
        public string _fly_prop {
            get { return fly_prop.ToString(); }
            set { if (string.IsNullOrEmpty(value)) fly_prop = false; else fly_prop = bool.Parse(value); }
        }

        /// <summary>
        /// 动画的基础速度（由于unity取不到每个state的speed,所以只能在外部加入数据）
        /// </summary>
        [XmlIgnore]
        public float basic_anim_speed;
        [XmlAttribute("basic_anim_speed")]
        public string _basic_anim_speed {
            get { return basic_anim_speed.ToString(); }
            set { if (string.IsNullOrEmpty(value)) basic_anim_speed = 0; else basic_anim_speed = float.Parse(value); }
        }

        /// <summary>
        /// 角色播放动画名称（目前只有6种）
        /// </summary>
        [XmlIgnore]
        public string anim_name;
        [XmlAttribute("anim_name")]
        public string _anim_name {
            get { return anim_name.ToString(); }
            set { if (string.IsNullOrEmpty(value)) anim_name = ""; else anim_name = value; }
        }

        /// <summary>
        /// 技能名称
        /// </summary>
        [XmlIgnore]
        public string path;
        [XmlAttribute("path")]
        public string _path {
            get { return path.ToString(); }
            set { if (string.IsNullOrEmpty(value)) path = ""; else path = value; }
        }

        /// <summary>
        /// 音效
        /// </summary>
        [XmlIgnore]
        public string sound_path;
        [XmlAttribute("sound_path")]
        public string _sound_path {
            get { return sound_path.ToString(); }
            set { if (string.IsNullOrEmpty(value)) sound_path = ""; else sound_path = value; }
        }

        /// <summary>
        /// 大于0表示loop音效
        /// </summary>
        [XmlIgnore]
        public float sound_time;
        [XmlAttribute("sound_time")]
        public string _sound_time {
            get { return sound_time.ToString(); }
            set { if (string.IsNullOrEmpty(value)) sound_time = 0; else sound_time = float.Parse(value); }
        }

        /// <summary>
        /// 忽略碰撞（走表现就设为true,后面数据皆可以不填）
        /// </summary>
        [XmlIgnore]
        public bool ignore_collier;
        [XmlAttribute("ignore_collier")]
        public string _ignore_collier {
            get { return ignore_collier.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ignore_collier = false; else ignore_collier = bool.Parse(value); }
        }

        /// <summary>
        /// 是否是aoe攻击（非aoe，即是检测仅针对敌人，性能会大大提高）
        /// </summary>
        [XmlIgnore]
        public bool is_aoe;
        [XmlAttribute("is_aoe")]
        public string _is_aoe {
            get { return is_aoe.ToString(); }
            set { if (string.IsNullOrEmpty(value)) is_aoe = false; else is_aoe = bool.Parse(value); }
        }

        /// <summary>
        /// 死后开启aoe（必须是飞行道具，否则无效）
        /// </summary>
        [XmlIgnore]
        public bool death_aoe;
        [XmlAttribute("death_aoe")]
        public string _death_aoe {
            get { return death_aoe.ToString(); }
            set { if (string.IsNullOrEmpty(value)) death_aoe = false; else death_aoe = bool.Parse(value); }
        }

        /// <summary>
        /// buff的ID（0表示无buff）
        /// </summary>
        [XmlIgnore]
        public List<int> buff;
        [XmlAttribute("buff")]
        public string _buff {
            get { return buff.ToString(); }
            set{ if (string.IsNullOrEmpty(value)) buff = new List<int>();else buff = ZStringUtil.ArrayStringToIntList(value.Split('-'));}
        }

        /// <summary>
        /// 给与伤害的cd
        /// </summary>
        [XmlIgnore]
        public float hurt_cd;
        [XmlAttribute("hurt_cd")]
        public string _hurt_cd {
            get { return hurt_cd.ToString(); }
            set { if (string.IsNullOrEmpty(value)) hurt_cd = 0; else hurt_cd = float.Parse(value); }
        }

        /// <summary>
        /// 起始点碰撞体使用（角色的位置为起点的偏移点）
        /// </summary>
        [XmlIgnore]
        public SerV2 start_point;
        [XmlAttribute("start_point")]
        public string _start_point {
            get { return start_point.ToString(); }
            set { if (string.IsNullOrEmpty(value)) start_point = new SerV2(); else start_point = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 攻击范围
        /// </summary>
        [XmlIgnore]
        public SerV2 collider_size;
        [XmlAttribute("collider_size")]
        public string _collider_size {
            get { return collider_size.ToString(); }
            set { if (string.IsNullOrEmpty(value)) collider_size = new SerV2(); else collider_size = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 实例化对应父亲的偏移预制体偏移
        /// </summary>
        [XmlIgnore]
        public SerV2 transform_start;
        [XmlAttribute("transform_start")]
        public string _transform_start {
            get { return transform_start.ToString(); }
            set { if (string.IsNullOrEmpty(value)) transform_start = new SerV2(); else transform_start = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 地面检测大小
        /// </summary>
        [XmlIgnore]
        public SerV2 ground_check_size;
        [XmlAttribute("ground_check_size")]
        public string _ground_check_size {
            get { return ground_check_size.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ground_check_size = new SerV2(); else ground_check_size = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 地面检测偏移
        /// </summary>
        [XmlIgnore]
        public SerV2 ground_check_offset;
        [XmlAttribute("ground_check_offset")]
        public string _ground_check_offset {
            get { return ground_check_offset.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ground_check_offset = new SerV2(); else ground_check_offset = ZStringUtil.StringToSerV2(value); }
        }

        /// <summary>
        /// 是否忽略空中碰撞检测
        /// </summary>
        [XmlIgnore]
        public bool ignore_sky_check;
        [XmlAttribute("ignore_sky_check")]
        public string _ignore_sky_check {
            get { return ignore_sky_check.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ignore_sky_check = false; else ignore_sky_check = bool.Parse(value); }
        }

        /// <summary>
        /// 是否忽略地面碰撞检测
        /// </summary>
        [XmlIgnore]
        public bool ignore_ground_check;
        [XmlAttribute("ignore_ground_check")]
        public string _ignore_ground_check {
            get { return ignore_ground_check.ToString(); }
            set { if (string.IsNullOrEmpty(value)) ignore_ground_check = false; else ignore_ground_check = bool.Parse(value); }
        }

        public List<T> LoadBytes<T>()
        {
            string bytesPath = "Assets/Game/AssetDynamic/ConfigBytes/excel_roleskill";
             TextAsset asset = TextAssetUtils.GetTextAsset(bytesPath);
            List<T> excel_roleskills = DeserializeData<T>(asset);
            return excel_roleskills;
        }

         private List<T> DeserializeData<T>(UnityEngine.TextAsset textAsset)
        {
            using (MemoryStream ms = new MemoryStream(textAsset.bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                allexcel_roleskill table = formatter.Deserialize(ms) as allexcel_roleskill;
                Config<excel_roleskill>.AddExcelToDic(typeof(T).Name, table.excel_roleskills);
                return table.excel_roleskills  as List<T>;
            }
        }
    }

    [Serializable]
    public class allexcel_roleskill
    {
        public List<excel_roleskill> excel_roleskills;
    }
}
