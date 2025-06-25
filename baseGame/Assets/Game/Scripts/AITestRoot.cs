using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITestRoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Singleton<BaseServiceBinder>.SetInstance(Singleton<ServiceBinder>.Instance);//±ØÒª´úÂë
        UIManager.Instance.SetUISceneCameraType(false);
        PathFindMgr.Init();
        UpdateMgr.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
