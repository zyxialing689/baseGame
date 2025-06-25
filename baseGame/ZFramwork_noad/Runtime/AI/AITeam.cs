using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITeam
{
    public AIAgent leader;
    public List<AIAgent> members;
    private int currentIndex;

    public AITeam(AIAgent leader)
    {
        this.leader = leader;
        leader.team = this;
        members = new List<AIAgent>();
        members.Add(leader);
        currentIndex = 0;
    }

    public void AddMember(AIAgent agent)
    {
        agent.team = this;
        members.Add(agent);
    }

    public void RemvoeMember(AIAgent agent)
    {
        members.Remove(agent);
    }

    public bool IsFull()
    {
        return members.Count >= 20;
    }

    public void ChangeLeader()
    {
        if (members.Count > 0)
        {
            leader = members[0];
        }
    }
}
