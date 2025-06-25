using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class AIDataJson
{
    public List<AINodeDataJson> aINodeDatas;
    public List<AIConectionDataJson> aIConectionDataJsons;

    public string ToJsonStr()
    {
        return JsonUtility.ToJson(this);
    }
    public List<AIStateData> GetHeadList()
    {
        List<AINodeDataJson> headJson = GetHead();
        List<AIStateData> headList = new List<AIStateData>();
        for (int i = 0; i < headJson.Count; i++)
        {
            AIStateData firstData = new AIStateData(null);
            CoverToAIStateData(headJson[i].id, firstData, firstData);
            headList.Add(firstData);

        }
        return headList;
    }
    //获取当前ai所有形态 测试用
    public List<int> GetAllChangeIds()
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < aINodeDatas.Count; i++)
        {
            if(aINodeDatas[i].aIState == AIStateType.ChangeAI)
            {
                ids.Add(aINodeDatas[i].changeID);
            }
        }
        return ids;
    }
    //获取当前ai所有攻击
    public List<TempAIdataForTest> GetAllAttackids()
    {
        List<TempAIdataForTest> ids = new List<TempAIdataForTest>();
        for (int i = 0; i < aINodeDatas.Count; i++)
        {
            int index = (int)aINodeDatas[i].aIState;
            if (index>=4000&&index<5000)
            {
                if(aINodeDatas[i].aIState == AIStateType.common_meleeAttack)
                {
                    ids.Add(new TempAIdataForTest(aINodeDatas[i].meleeAttackID,aINodeDatas[i].attackKeepTime));
                }
                else
                {
                    ids.Add(new TempAIdataForTest(aINodeDatas[i].remoteAttackID, aINodeDatas[i].attackKeepTime));
                }
            }
        }
        return ids;
    }

    private void CoverToAIStateData(int nodeId, AIStateData parent,AIStateData first)
    {

        AINodeDataJson initNodeData = GetNodeDataById(nodeId);
        parent.aIStateType = initNodeData.aIState;//节点type
        parent.keepTime = initNodeData.keepTime;//节点持续时间
        parent.findInterval = initNodeData.findInterval;//节点持续时间
        parent.patrolTimes = initNodeData.patrolTimes;//巡逻次数停止巡逻
        parent.stopDis = initNodeData.stopDis;//距离多少停止（跟随队长）
        parent.cd = initNodeData.cd;//距离多少停止（跟随队长）
        parent.resetID = initNodeData.resetID;//距离多少停止（跟随队长）
        parent.shadreCDID = initNodeData.shadreCDID;//距离多少停止（跟随队长）
        parent.isFinishCDByChangeAI = initNodeData.isFinishCDByChangeAI;//距离多少停止（跟随队长）
        parent.isKeepCDByChangeAI = initNodeData.isKeepCDByChangeAI;//距离多少停止（跟随队长）
        parent.findCotainSky = initNodeData.findCotainSky;//距离多少停止（跟随队长）
        parent.followEnemyDir = initNodeData.followEnemyDir;//距离多少停止（跟随队长）
        parent.remoteAttackID = initNodeData.remoteAttackID;//
        parent.attackCount = initNodeData.attackCount;//
        parent.meleeAttackID = initNodeData.meleeAttackID;//
        parent.attackKeepTime = initNodeData.attackKeepTime;//
        parent.fixedPoint = (FixedPointType)initNodeData.fixedPoint;//
        parent.id = initNodeData.id;
        parent.isFarSkill = initNodeData.isFarSkill;
        parent.isFarBackTarget = initNodeData.isFarBackTarget;
        parent.serchType = (ClosestType)initNodeData.serchType;
        parent.condTypes = initNodeData.GetCondTypes();
        parent.condTypeParam = initNodeData.GetCondTypeParam();
        parent.nextParamTime = initNodeData.GetCondNextTimeParam();
        parent.logStr = initNodeData.logStr;
        parent.changeID = initNodeData.changeID;
        parent.firstAIState = first;
        List<int> nextIds = initNodeData.nextIds;
        nextIds.Sort((a, b) => {
            return GetNodeDataById(a).sort.CompareTo(GetNodeDataById(b).sort); 
        });
        if (nextIds != null&& nextIds.Count!=0)
        {

            parent.condAIStateData = new List<AIStateData>();
            for (int i = 0; i < nextIds.Count; i++)
            {
                AIStateData node = new AIStateData(first);
                CoverToAIStateData(nextIds[i], node, first);
                parent.condAIStateData.Add(node);
            }
        }
    }

    public AINodeDataJson GetNodeDataById(int id)
    {
        AINodeDataJson adj = null;
        for (int i = 0; i < aINodeDatas.Count; i++)
        {
            if(aINodeDatas[i].id == id)
            {
                adj = aINodeDatas[i];
            }
        }
        if (adj == null)
        {
            ZLogUtil.LogError("结构有问题");
            return adj;
        }
        else
        {
            return adj;
        }
    }

    private List<AINodeDataJson> GetHead()
    {
        List<AINodeDataJson> nodes = new List<AINodeDataJson>();
        for (int i = 0; i < aINodeDatas.Count; i++)
        {
            if(aINodeDatas[i].aIState == AIStateType.Root)
            {
                nodes.Add(aINodeDatas[i]);
            }
        }
        return nodes;
    }

}

public class TempAIdataForTest
{
    public int id;
    public float time;
    public TempAIdataForTest(int id,float time)
    {
        this.id = id;
        this.time = time;
    }
}
