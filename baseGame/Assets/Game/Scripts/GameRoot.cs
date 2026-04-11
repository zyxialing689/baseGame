using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameRoot : MonoBehaviour
{

    void Start()
    {
        Singleton<BaseServiceBinder>.SetInstance(Singleton<ServiceBinder>.Instance);//必要代码
        UIManager.Instance.SetUISceneCameraType(false);
        UIManager.Instance.OpenPanel<EnterPanel>();
        Application.targetFrameRate = 1000;
        // RandomMgr.Instance.Init();

        UIManager.Instance.camera_scene.AddComponent<CameraController2D>();
    }

    // Update is called once per frame

}
