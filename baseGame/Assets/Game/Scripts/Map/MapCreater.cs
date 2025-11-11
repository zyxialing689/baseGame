using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class CommonGameObject
{
    public static GameObject hpObj;
}

public class MapCreater : MonoBehaviour
{
    private Camera camera_scene;
    void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        PathFindMgr.Init();//地图相关初始化
        UpdateMgr.Init();//自定义update loop
        BattleEventMgr.Init();//游戏核心事件
        DamageNumberMgr.Init();//伤害ui
        yield return StartCoroutine(CreateSceneNodes());
        yield return null;
        //////////
        Destroy(gameObject);
    }

    IEnumerator CreateSceneNodes()
    {
        GameObject sceneObj = new GameObject("scene");
        GameObject rootObj = new GameObject("root");
        GameObject backObj = new GameObject("back");
        GameObject centerObj = new GameObject("center");
        GameObject forceObj = new GameObject("force");
        GameObject sceneUIObj = new GameObject("sceneUI");
        GameObject hpObj = new GameObject("hp");
        CommonGameObject.hpObj = hpObj;
        sceneUIObj.transform.SetParent(forceObj.transform);
        backObj.transform.SetParent(rootObj.transform);
        centerObj.transform.SetParent(rootObj.transform);
        forceObj.transform.SetParent(rootObj.transform);
        rootObj.transform.SetParent(sceneObj.transform);
        hpObj.transform.SetParent(sceneUIObj.transform);
        var eventObj = GameObject.FindGameObjectWithTag("EventSystem");
        if (eventObj == null)
        {
            var tempObj_1 = new GameObject("EventSystem");
            tempObj_1.AddComponent<EventSystem>();
            tempObj_1.AddComponent<StandaloneInputModule>();
        }
        //var mapLoaderObj = new GameObject("MapLoader");
        ////var cameraRootObj = new GameObject("CameraRoot");
        ////CreateCameras(cameraRootObj.transform);
        ////cameraRootObj.transform.position = new Vector3(0, 0, -100);

        //CreateMapData(mapLoaderObj);
        //Transform[,] enemyTfs;
        //var tfs = CreateMapObj(backObj.transform, forceObj.transform, out enemyTfs);
        CreateSceneUI(sceneUIObj);
        sceneObj.AddComponent<GameCreater>();
        for (int j = 0; j < 5; j++)
        {
            for (int i1 = 0; i1 < 10; i1++)
            {
                for (int i2 = 0; i2 < 10; i2++)
                {
                    DamageNumberMgr._instance.InitPool();//初始化对象池数量
                }
                yield return null;
            }

        }



        yield return StartCoroutine(CreateTeamObj());

        yield return null;
        //EventManager.Instance.Dispatch(Event_Common_EnterBattleFinishEvent.AutoCreate());
    }

    IEnumerator CreateTeamObj()
    {
        //List<TestMonster> list = BattleDataMgr.Instance.GetFightCardDataList();

        //for (int i = 0; i < list.Count; i++)
        //{
        //    GameCreater._instance.CreatePlayer(list[i], true, true);
        //    //GameCreater._instance.CreatePlayer(enemyList[i], false, false);
        //}
        yield return null;

    }

    private void CreateSceneUI(GameObject sceneUIObj)
    {
        sceneUIObj.layer = 7;
        var sceneCanvas = sceneUIObj.AddComponent<Canvas>();
        var sceneRT = sceneUIObj.GetComponent<RectTransform>();
        sceneCanvas.renderMode = RenderMode.WorldSpace;
        sceneCanvas.worldCamera = camera_scene;
        sceneCanvas.sortingOrder = 20001;
        RectTransformUtils.SetStretchBottomLeft(sceneUIObj);
        sceneRT.localScale = Vector2.one * 0.01f;
        sceneRT.sizeDelta = new Vector2(6900, 1500);
        DamageNumberMgr._instance.SetUIParent(sceneUIObj.transform);
    }


    void CreateCameras(Transform cameraRoot)
    {
        var cameraObj = new GameObject("sceneCamera");
        var listenerObj = new GameObject("audioListener");
        listenerObj.transform.SetParent(cameraObj.transform);
        listenerObj.transform.localPosition = Vector3.forward * 100;
        listenerObj.AddComponent<AudioListener>();
        cameraObj.transform.SetParent(cameraRoot);
        cameraObj.transform.localPosition = new Vector3(18, 7f, 0);
        //cameraObj.AddComponent<CameraControl>();
        camera_scene = cameraObj.AddComponent<Camera>();
        camera_scene.clearFlags = CameraClearFlags.SolidColor;
        camera_scene.cullingMask = (1 << 6) + (1 << 7) + (1 << 0);
        camera_scene.orthographic = true;
        camera_scene.orthographicSize = 8.5f;
        camera_scene.depth = 0;

    }

    private void CreateMapData(GameObject obj)
    {
        obj.AddComponent<PathFindMgr>();
        var textAsset = TextAssetUtils.GetTextAsset("Assets/Game/AssetDynamic/Config/Map/map1");
        //PathFindMgr._instance.InitMap(textAsset.bytes);
        //obj.AddComponent<QuadTreeMgr>();
        //QuadTreeMgr._instance.area = new Rect(new Vector2(0, PathFindMgr._instance.mapOffsetY), new Vector2(PathFindMgr._instance.maxWidth, PathFindMgr._instance.mapHeightY));
        //QuadTreeMgr._instance.Init();

    }


    Transform[,] CreateMapObj(Transform back, Transform force, out Transform[,] enemyTfs)
    {
        enemyTfs = new Transform[25, 25];
        var backObj = PrefabUtils.Instance("__smallMap/Forest_Day");
        backObj.transform.SetParent(back);
        var tf = backObj.transform.Find("birthPos");
        var ememyTf = backObj.transform.Find("enemyBirthPos");
        Transform[,] tfs = new Transform[25, 25];
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                tfs[i, j] = tf.GetChild(i * 25 + j);
                enemyTfs[i, j] = ememyTf.GetChild(i * 25 + j);
            }
        }
        return tfs;
    }

}
