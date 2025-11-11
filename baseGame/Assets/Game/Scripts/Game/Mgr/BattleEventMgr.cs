using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using DG.Tweening;

public class BattleEventMgr : MonoBehaviour
{
    public static BattleEventMgr _instance;
    private bool gameEnd = false;

    public static void Init()
    {
        GameObject obj = new GameObject("BattleEventMgr");
        _instance = obj.AddComponent<BattleEventMgr>();
    }

    private void Start()
    {
        gameEnd = false;
        EventManager.Instance.AddObserver<Event_Battle_AddPlayerNum>(OnRec_Event_Battle_AddPlayerNum);
        EventManager.Instance.AddObserver<Event_Battle_RemovePlayerNum>(OnRec_Event_Battle_RemovePlayerNum);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveObserver<Event_Battle_AddPlayerNum>(OnRec_Event_Battle_AddPlayerNum);
        EventManager.Instance.RemoveObserver<Event_Battle_RemovePlayerNum>(OnRec_Event_Battle_RemovePlayerNum);
    }

    private void OnRec_Event_Battle_RemovePlayerNum(Event_Battle_RemovePlayerNum obj)
    {
        if (gameEnd) return;
        if (!AIMgr.HaveEmenyAIAgent(PlayerCamp.PlayerCampA))
        {
            ZLogUtil.Log("游戏结束:敌人死完");
            AIMgr.EndBattle();
            gameEnd = true;
            //JumpManager.JumpPanel<BattleEndPanel>(PanelLayer.Panel);
            return;
        }

        if (!AIMgr.HaveEmenyAIAgent(PlayerCamp.PlayerCampB))
        {
            ZLogUtil.Log("游戏结束:队友死完");
            AIMgr.EndBattle();
            gameEnd = true;
            //JumpManager.JumpPanel<BattleEndPanel>(PanelLayer.Panel);
            return;
        }
    }
    private void OnRec_Event_Battle_AddPlayerNum(Event_Battle_AddPlayerNum obj)
    {
        ZLogUtil.Log(AIMgr._allAgents.Count);
    }
}
