using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;
public class TempAIStateData
{
    public string keepTime = "";
    public string patrolTimes = "";
    public string findInterval = "";
    public string stopDis = "";
    public string logStr = "";
    public string changeID = "";
    public string resetID = "";
    public string meleeAttackID = "";
    public string remoteAttackID = "";
    public string attackKeepTime = "";
    public int fixedPoint = 0;
    public string cd = "";
    public string shadreCDID = "";
    public string sort ="";
    public string attackCount = "";
    public bool findCotainSky = false;
    public bool followEnemyDir = false;
    public bool isFarSkill = false;
    public bool isFarBackTarget = false;
    public bool isKeepCDByChangeAI = false;
    public bool isFinishCDByChangeAI = false;
    public int serchType = 0;
}

[Serializable]
public class AIStateData
{
    public AIStateType aIStateType;
    public int id;
    public int resetID;
    public float keepTime;//节点最大保持时间
    public float patrolTimes;//巡逻次数后寻敌人
    public string logStr;
    public float findInterval;//寻路间隔
    public float stopDis;//多少米停止
    public float cd;//多少米停止
    public int shadreCDID;//多少米停止
    public bool findCotainSky;
    public bool followEnemyDir;
    public bool isKeepCDByChangeAI;
    public bool isFinishCDByChangeAI;
    public bool isFarSkill;
    public bool isFarBackTarget;
    public int changeID;
    public int attackCount;
    public int remoteAttackID;
    public int meleeAttackID;
    public float attackKeepTime;
    public FixedPointType fixedPoint;
    public ClosestType serchType;



    public List<List<CondType>> condTypes;
    public List<List<Vector4>> condTypeParam;
    public List<AIStateData> condAIStateData;



    [NonSerialized]
    public AIStateData firstAIState;
    [NonSerialized]
    public float nextTime;
    [NonSerialized]
    public List<List<Vector4>> nextParamTime;

    public AIStateData(AIStateData first)
    {
        this.firstAIState = first;
    }
    public void SetNextTime(float curTime)
    {
        nextTime = curTime + keepTime* RandomMgr.GetValue();
    }
    private void SetNextParamTime(float curTime)
    {
        for (int i = 0; i < condTypes.Count; i++)
        {
            for (int j = 0; j < condTypes[i].Count; j++)
            {
                if(condTypes[i][j]== CondType._d_randKeepTime__time)
                {
                    nextParamTime[i][j] = curTime * Vector4.one + Vector4.one* RandomMgr.Range(condTypeParam[i][j].x, condTypeParam[i][j].y);
                }
                else
                {
                    nextParamTime[i][j] = (curTime * Vector4.one + condTypeParam[i][j]);
                }

            }
        }
    }

    public void SetNextCondtionTime(float curTime)
    {
        if(condAIStateData!=null)
        for (int i = 0; i < condAIStateData.Count; i++)
        {
                condAIStateData[i].SetNextParamTime(curTime);
        }
    }
    
}