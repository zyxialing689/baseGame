using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAIState : AIState
{
    public override void Awake()
    {
        //var changeTable = ExcelConfig.Get_excel_changeaidata(stateData.changeID);
        //var roleTable = ExcelConfig.Get_excel_roledata(changeTable.role_id);
        //if (!string.IsNullOrEmpty(roleTable.ai_path))
        //{
        //    agent.agentData = new AgentData(changeTable.role_id);
        //    agent.UpdateAI(roleTable.ai_path);
        //    agent.UpdateBindUI();
        //    agent.aICollider.UpdateColliderData();
        //    if (stateData.isKeepCDByChangeAI)
        //    {

        //    }
        //    else
        //    {
        //        agent.agentTempData.globalCdMap.Clear();
        //    }
        //    if (!string.IsNullOrEmpty(roleTable.anim_path))
        //    {
        //        var anim = ResHanderManager.Instance.GetGetRuntimeAnimatorController(roleTable.anim_path);
        //        agent.UpdateAnimator(anim,changeTable.have_change_anim);
        //    }
        //}


    }

    public override AIStateData TryNextCond()
    {
        return null;
    }

    public override bool TryRestCond()
    {
        return false;
    }
}
