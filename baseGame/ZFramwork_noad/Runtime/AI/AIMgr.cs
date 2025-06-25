using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMgr
{
    private static bool startBattle = true;
    private static Dictionary<PlayerCamp, List<AIAgent>> _campMap;
    public static List<AIAgent> _allAgents = new List<AIAgent>();
    private static Dictionary<PlayerCamp, List<AITeam>> _campTeamMap;


    public static void Clear()
    {
        SetStartBattle(false);
        for (int i = 0; i < _allAgents.Count; i++)
        {
            _allAgents[i].Clear();
        }
        if (_campMap != null)
            _campMap.Clear();
        _allAgents.Clear();
        if (_campTeamMap != null)
            _campTeamMap.Clear();

    }


    public static void SetStartBattle(bool battle)
    {
        startBattle = battle;
    }

    public static int GetCount()
    {
        int count = 0;
        if(_campMap!=null)
        foreach (var item in _campMap)
        {
            count += item.Value.Count;
        }
        return count;
    }

    private static void AddTeamMap(AIAgent agent)
    {
        if (_campTeamMap == null)
        {
            _campTeamMap = new Dictionary<PlayerCamp, List<AITeam>>();
        }
        bool alreadyAdd = false;
        if (_campTeamMap.ContainsKey(agent.playerCamp))
        {
            foreach (var item in _campTeamMap[agent.playerCamp])
            {
                if (!item.IsFull())
                {
                    item.AddMember(agent);
                    alreadyAdd = true;
                    break;
                }
            }
            if (!alreadyAdd)
            {
                AITeam aITeam = new AITeam(agent);
                _campTeamMap[agent.playerCamp].Add(aITeam);
            }
        }
        else
        {
            List<AITeam> teams = new List<AITeam>();
            _campTeamMap.Add(agent.playerCamp, teams);
            var team = new AITeam(agent);
            teams.Add(team);
        }
    }

    public static void AddPlayer(AIAgent agent)
    {
        if (_campMap == null)
        {
            _campMap = new Dictionary<PlayerCamp, List<AIAgent>>();
        }
        if (_campMap.ContainsKey(agent.playerCamp))
        {
            if (!_campMap[agent.playerCamp].Contains(agent))
            {
                _campMap[agent.playerCamp].Add(agent);
                _allAgents.Add(agent);
                AddTeamMap(agent);
                EventManager.Instance.Dispatch(Event_Battle_AddPlayerNum.AutoCreate());
            }
         }
        else
        {
            List<AIAgent> list = new List<AIAgent>();
            list.Add(agent);
            _allAgents.Add(agent);
            _campMap.Add(agent.playerCamp, list);
            AddTeamMap(agent);
            EventManager.Instance.Dispatch(Event_Battle_AddPlayerNum.AutoCreate());
        }
    }

    public static void RemovePlayer(AIAgent agent)
    {
        if (_campMap == null)
        {
           return;
        }
        if (_campMap.ContainsKey(agent.playerCamp))
        {
            if (_campMap[agent.playerCamp].Contains(agent))
            {
                _campMap[agent.playerCamp].Remove(agent);
                _allAgents.Remove(agent);
                agent.team.RemvoeMember(agent);
            }
        }
    }

    public static AIAgent GetCloserTarget(AIAgent selfAgent,ClosestType closestType,bool containSky,bool isEnemy = true,bool containSelf=false)
    {
        if (selfAgent == null|| !startBattle)
        {
            return null;
        }
        AIAgent target = null;
        float curDis = float.MaxValue;
        bool isFriend = false;
        foreach (var item in _campMap)
        {
            isFriend = isEnemy ? selfAgent.playerCamp != item.Key : selfAgent.playerCamp == item.Key;
            if (isFriend)
            {
                foreach (var agent in item.Value)
                {
                    if (agent.isDead) continue;
                    if (!containSky)
                    {
                        if (agent.agentData.sky_height > 0)
                        {
                            continue;
                        }
                    }
                    float tempDis = GetDistance(selfAgent, agent, closestType);
                    if (tempDis < curDis)
                    {
                        target = agent;
                        curDis = tempDis;
                    }
                }
            }
        }
        if (!containSelf&&target == selfAgent)
        {
            return null;
        }
        return target;
    }

    static List<AIAgent> tempAIAgents = new List<AIAgent>();
    public static AIAgent[] GetRandomAIAgents(AIAgent selfAgent,int num,ClosestType search_type,bool focus_friend = false,bool isContainSelf = true)
    {
        tempAIAgents.Clear();
        if (selfAgent == null|| !startBattle)
        {
            return tempAIAgents.ToArray();
        }
        PlayerCamp enemyCamp;
        if (focus_friend)
        {
            enemyCamp = selfAgent.playerCamp;
        }
        else
        {
            if (selfAgent.playerCamp == PlayerCamp.PlayerCampA)
            {
                enemyCamp = PlayerCamp.PlayerCampB;
            }
            else
            {
                enemyCamp = PlayerCamp.PlayerCampA;
            }
        }
       
        if (_campMap.ContainsKey(enemyCamp))
        {
            List<AIAgent> list = new List<AIAgent>();
            for (int i = 0; i < _campMap[enemyCamp].Count; i++)
            {
                if (!_campMap[enemyCamp][i].isDead)
                {
                    list.Add(_campMap[enemyCamp][i]);
                }
            }

            list.Sort((a, b) => {
                return GetDistance(selfAgent, a, search_type).CompareTo(GetDistance(selfAgent, b, search_type));
            });
            if (!isContainSelf)
            {
                if (list.Count >= num)
                {
                    for (int i = 0; i < num; i++)
                    {
                        if (selfAgent != list[i])
                        {
                            tempAIAgents.Add(list[i]);
                        }
       
                    }
                }
                else
                {
                    num = list.Count;
                    for (int i = 0; i < num; i++)
                    {
                        if (selfAgent != list[i])
                        {
                            tempAIAgents.Add(list[i]);
                        }
                    }
                }
            }
            else
            {
                if (list.Count >= num)
                {
                    for (int i = 0; i < num; i++)
                    {
                        tempAIAgents.Add(list[i]);
                    }
                }
                else
                {
                    num = list.Count;
                    for (int i = 0; i < num; i++)
                    {
                        tempAIAgents.Add(list[i]);
                    }
                }
            }

        }

        return tempAIAgents.ToArray();
    }
    public static AIAgent GetRandomAIAgent(AIAgent selfAgent)
    {
        AIAgent aIAgent = null;
        if (selfAgent == null|| !startBattle)
        {
            return aIAgent;
        }
        PlayerCamp enemyCamp;
        if (selfAgent.playerCamp == PlayerCamp.PlayerCampA)
        {
            enemyCamp = PlayerCamp.PlayerCampB;
        }
        else
        {
            enemyCamp = PlayerCamp.PlayerCampA;
        }
        if (_campMap.ContainsKey(enemyCamp))
        {
            List<AIAgent> list = new List<AIAgent>();
            for (int i = 0; i < _campMap[enemyCamp].Count; i++)
            {
                if (!_campMap[enemyCamp][i].isDead)
                {
                    list.Add(_campMap[enemyCamp][i]);
                }
            }
            if (list.Count > 0)
            {
                aIAgent = list[Random.Range(0, list.Count)];
            }
        }

        return aIAgent;
    }
    public static float GetDistance(AIAgent agentA,AIAgent agentB,ClosestType closestType)
    {
        float dis = 0;
        switch (closestType)
        {
            case ClosestType.X:
                dis = Mathf.Abs(agentA.transform.position.x - agentB.transform.position.x);
                break;
            case ClosestType.Y:
                dis = Mathf.Abs(agentA.transform.position.y - agentB.transform.position.y);
                break;
            case ClosestType.DIS:
                dis = Mathf.Abs(agentA.transform.position.y - agentB.transform.position.y)+ Mathf.Abs(agentA.transform.position.x - agentB.transform.position.x);
                break;
            case ClosestType.HP_Min_precent:
                dis = agentB.attrData.GetHpPrecent();
                break;
            case ClosestType.HP_Min_value:
                dis = agentB.attrData.hp;
                break;
            case ClosestType.Random:
                dis = RandomMgr.GetValue();
                break;
            case ClosestType.Far:
                dis =100000 -( Mathf.Abs(agentA.transform.position.y - agentB.transform.position.y) + Mathf.Abs(agentA.transform.position.x - agentB.transform.position.x));
                break;
        }
        
        return dis;
    }

    public static List<AIAgent> GetEmenyAIAgent(PlayerCamp playerCamp)
    {
        List<AIAgent> enemys = new List<AIAgent>();
        if (!startBattle)
        {
            return enemys;
        }
        foreach (var item in _campMap)
        {
            if (item.Key != playerCamp)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    enemys.Add(item.Value[i]);
                }
            }
        }
        return enemys;
    }

    public static AITeam GetFirstTeam(PlayerCamp playerCamp)
    {
        AITeam team = null;
        if(_campTeamMap!=null&& _campTeamMap.ContainsKey(playerCamp))
        {
            if (_campTeamMap[playerCamp].Count > 0)
            {
                team = _campTeamMap[playerCamp][0];
            }
        }
        return team;
    }
    public static bool HaveEmenyAIAgent(PlayerCamp playerCamp)
    {
        if (!startBattle)
        {
            return false;
        }
        foreach (var item in _campMap)
        {
            if (item.Key != playerCamp)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (!item.Value[i].isDead)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public static int GetCampCount(PlayerCamp playerCamp)
    {
        int count = 0;
        foreach (var item in _campMap)
        {
            if (item.Key == playerCamp)
            {
                count = item.Value.Count;
            }
        }
        return count;
    }
    public static bool HaveSkyEmenyAIAgent(PlayerCamp playerCamp)
    {
        if (!startBattle)
        {
            return false;
        }
        foreach (var item in _campMap)
        {
            if (item.Key != playerCamp)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (item.Value[i].agentData.sky_height > 0 && !item.Value[i].isDead)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public static bool HaveGroundEmenyAIAgent(PlayerCamp playerCamp)
    {
        if (!startBattle)
        {
            return false;
        }
        foreach (var item in _campMap)
        {
            if (item.Key != playerCamp)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (item.Value[i].agentData.sky_height == 0&& !item.Value[i].isDead)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public static void KillAllAgent()
    {
        foreach (var item in _campMap)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                item.Value[i].TriggerDead();
            }
        }
    }
    public static void KillAAgent()
    {
        foreach (var item in _campMap)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                if(item.Value[i].playerCamp== PlayerCamp.PlayerCampA)
                {
                    item.Value[i].TriggerDead();
                }
            }
        }
    }
    public static void KillBAgent()
    {
        foreach (var item in _campMap)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                if (item.Value[i].playerCamp == PlayerCamp.PlayerCampB)
                {
                    item.Value[i].TriggerDead();
                }
            }
        }
    }


    public static void StartBattle()
    {
        if (_allAgents != null)
        {
            for (int i = 0; i < _allAgents.Count; i++)
            {
                _allAgents[i].isStake = false;

            }
        }
        SetStartBattle(true);
    }

    public static void EndBattle()
    {

        SetStartBattle(false);
    }
}
