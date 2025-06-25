using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{

    void Start()
    {
         Singleton<BaseServiceBinder>.SetInstance(Singleton<ServiceBinder>.Instance);//必要代码
        UIManager.Instance.SetUISceneCameraType(true);
        UIManager.Instance.OpenPanel<EnterPanel>();
  
         Application.targetFrameRate = 1000;
        // RandomMgr.Instance.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
