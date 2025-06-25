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
    //��ȡ��ǰai������̬ ������
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
    //��ȡ��ǰai���й���
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
        parent.aIStateType = initNodeData.aIState;//�ڵ�type
        parent.keepTime = initNodeData.keepTime;//�ڵ����ʱ��
        parent.findInterval = initNodeData.findInterval;//�ڵ����ʱ��
        parent.patrolTimes = initNodeData.patrolTimes;//Ѳ�ߴ���ֹͣѲ��
        parent.stopDis = initNodeData.stopDis;//�������ֹͣ������ӳ���
        parent.cd = initNodeData.cd;//�������ֹͣ������ӳ���
        parent.resetID = initNodeData.resetID;//�������ֹͣ������ӳ���
        parent.shadreCDID = initNodeData.shadreCDID;//�������ֹͣ������ӳ���
        parent.isFinishCDByChangeAI = initNodeData.isFinishCDByChangeAI;//�������ֹͣ������ӳ���
        parent.isKeepCDByChangeAI = initNodeData.isKeepCDByChangeAI;//�������ֹͣ������ӳ���
        parent.findCotainSky = initNodeData.findCotainSky;//�������ֹͣ������ӳ���
        parent.followEnemyDir = initNodeData.followEnemyDir;//�������ֹͣ������ӳ���
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
            ZLogUtil.LogError("�ṹ������");
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
