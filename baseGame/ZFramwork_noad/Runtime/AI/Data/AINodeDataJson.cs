using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class AINodeDataJson
{
    public int id;//
    public  AIStateType aIState;//
    public string logStr;
    public List<CondWindow> condRects;
    public  Vector2 pos;//
    public List<int> nextIds;//
    public float keepTime;
    public int patrolTimes;
    public float findInterval;
    public float stopDis;
    public float cd;
    public int resetID;
    public int sort;
    public int shadreCDID;
    public bool findCotainSky;
    public bool followEnemyDir;
    public int changeID;
    public int meleeAttackID;
    public int remoteAttackID;
    public int attackCount;
    public int serchType;
    public float attackKeepTime;
    public int fixedPoint;
    public bool isKeepCDByChangeAI;
    public bool isFinishCDByChangeAI;
    public bool isFarSkill;
    public bool isFarBackTarget;

    public List<List<CondType>> GetCondTypes()
    {
        List<List<CondType>> condTypes = new List<List<CondType>>();
        if (condRects == null|| condRects.Count==0)
        {
            List<CondType> list = new List<CondType>();
            list.Add(CondType.None);
            condTypes.Add(list);
        }
        else
        {
            for (int i = 0; i < condRects.Count; i++)
            {
                condTypes.Add(condRects[i].list);
            }
        }
        return condTypes;
    }
    public List<List<Vector4>> GetCondTypeParam()
    {
        List<List<Vector4>> condTypeParams = new List<List<Vector4>>();
        if (condRects == null|| condRects.Count==0)
        {
            List<Vector4> list = new List<Vector4>();
            list.Add(Vector4.zero);
            condTypeParams.Add(list);
        }
        else
        {
            for (int i = 0; i < condRects.Count; i++)
            {
                List<Vector4> tempList = new List<Vector4>();
                for (int j = 0; j < condRects[i].listData.Count; j++)
                {
                    tempList.Add(condRects[i].listData[j] * 0.001f);
                }
                condTypeParams.Add(tempList);
            }
        }
        return condTypeParams;
    }
    public List<List<Vector4>> GetCondNextTimeParam()
    {
        List<List<Vector4>> condTypeParams = new List<List<Vector4>>();
        if (condRects == null|| condRects.Count==0)
        {
            List<Vector4> list = new List<Vector4>();
            list.Add(Vector4.zero);
            condTypeParams.Add(list);
        }
        else
        {
   
            for (int i = 0; i < condRects.Count; i++)
            {
                List<Vector4> list = new List<Vector4>();
                for (int j = 0; j < condRects[i].listData.Count; j++)
                {
                    list.Add(Vector4.zero);
                }
                condTypeParams.Add(list);
            }
        }
        return condTypeParams;
    }
}
