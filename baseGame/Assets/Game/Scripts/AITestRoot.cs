using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITestRoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Singleton<BaseServiceBinder>.SetInstance(Singleton<ServiceBinder>.Instance);//必要代码
        ExcelConfig.Instance.LoadAllExcel();//必要代码
        UIManager.Instance.SetUISceneCameraType(false);//必要代码

        //gameObject.AddComponent<MapCreater>();
        //youxi bibei
        //UIManager.Instance.SetUISceneCameraType(false);
        //PathFindMgr.Init();
        //UpdateMgr.Init();
        JumpManager.JumpPanel<GamePanel>(PanelLayer.Panel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
