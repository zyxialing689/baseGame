using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using DG.Tweening;

public class UpdateMgr : MonoBehaviour
{
    public static UpdateMgr _instance;
    public bool Pause = false;
    public static void Init()
    {
        GameObject obj = new GameObject("UpdateMgr");
        _instance = obj.AddComponent<UpdateMgr>();
    }

    public static void Clear()
    {
        if (_instance != null)
        {
            _instance.Pause = false;
            _instance.StopAllCoroutines();
            _instance = null;
        }
    }

    public static void PauseGame()
    {
        if (_instance != null)
        {
            _instance.Pause = true;
        }
    }

    void Start()
    {
        StartCoroutine(CustomUpdate());
        StartCoroutine(CustomFixedUpdate());
        //ThreadStart threadStart = new ThreadStart(AUpdate);
        //playerCampThreadA = new Thread(threadStart);
        //playerCampThreadA.Start();
    }


    IEnumerator CustomFixedUpdate()
    {
        while (true)
        {
            if (Pause)
            {

            }
            else
            {
                for (int i = 0; i < AIMgr._allAgents.Count; i++)
                {
                    AIMgr._allAgents[i]._FixedUpdate();

                }
                for (int i = 0; i < AttackMgr._allAttacks.Count; i++)
                {
                    AttackMgr._allAttacks[i]._FixedUpdate();
                }

            }

            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }

    }

    IEnumerator CustomUpdate()
    {
        while (true)
        {
            if (Pause)
            {

            }
            else
            {
                for (int i = 0; i < AIMgr._allAgents.Count; i++)
                {
                    AIMgr._allAgents[i]._Update();

                }
                for (int i = 0; i < AttackMgr._allAttacks.Count; i++)
                {
                    AttackMgr._allAttacks[i]._Update();
                }
                DOTween.ManualUpdate(Time.deltaTime, Time.timeScale);
            }

            yield return null;
        }

    }

}
