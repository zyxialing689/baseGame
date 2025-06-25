using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CondWindow
{
    [NonSerialized]
    public Rect rect;

    public int lastID;
    public int nextID;
    public string des;
    public List<CondType> list;//游戏需要的数据
    public List<Vector4> listData;//游戏需要的数据
    public CondWindow(int id,string des)
    {
        lastID = id - 1;
        nextID = id + 1;
        rect = new Rect();
        this.des = des;
        list = new List<CondType>();
        listData = new List<Vector4>();
    }

    public void Add(CondType condType)
    {
        list.Add(condType);
        listData.Add(Vector4.zero);
    }

    public void Remove(CondType condType)
    {
        list.Remove(condType);
    }
}
