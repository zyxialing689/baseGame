using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public partial class GamePanel : BasePanel
{

    public override void Init(params object[] args)
    {
        base.Init(args);
        panelLayer = PanelLayer.Panel;
        adressPath = "Panel/GamePanel";
        AudioManager.GetInstance().PlayBgmSound(GameConst.const_bgm2);
        UIManager.Instance.AutoMatchWidthOrHeightByPortrait();
    }
    public override void OnShowing()
    {
        EventManager.Instance.AddObserver<EventGP_palceInfo>(PalceInfo);
        
        btnTree.zbtn.onClick.AddListener(() =>
        {
            setNormal();
            btnTree.zimg.color = Color.green;
            Debug.Log("进入建筑模式");
            BuildSystem.Instance.StartBuild(BuildTestInput.instance.house);
        });
        btnHouse.zbtn.onClick.AddListener(() =>
        {
            setNormal();
            btnHouse.zimg.color = Color.green;
            Debug.Log("进入建筑模式");
            BuildSystem.Instance.StartBuild(BuildTestInput.instance.tree);
        });
        btnHouse1.zbtn.onClick.AddListener(() =>
        {
            setNormal();
            btnHouse1.zimg.color = Color.green;
            Debug.Log("进入建筑模式");
            BuildSystem.Instance.StartBuild(BuildTestInput.instance.house2);
        });
        delBtn.zbtn.onClick.AddListener(() =>
        {
            setNormal();
            delBtn.zimg.color = Color.green;
            BuildSystem.Instance.StartDelete(false); // 单次删除
            Debug.Log("进入单次删除模式");
        });
        delcBtn.zbtn.onClick.AddListener(() =>
        {
            setNormal();
            delcBtn.zimg.color = Color.green;
            BuildSystem.Instance.StartDelete(true); // 连续删除
            Debug.Log("进入连续删除模式");
        });
        exitBtn.zbtn.onClick.AddListener(() =>
        {
            setNormal();
            BuildSystem.Instance.StopBuild();
            BuildSystem.Instance.StopDelete();
        });
        undoBtn.zbtn.onClick.AddListener(() =>
        {
            BuildSystem.Instance.UndoLastBuild();
            Debug.Log("撤销建造操作");
        });
    }

    private void PalceInfo(EventGP_palceInfo info)
    {
        float x = info.origin.x;
        float y = info.origin.y;
        x = x + info.width / 2f;
        y = y + info.height;
        Vector3 targetPos = new Vector3(x, y,0);
        var pos = GetUIPos(targetPos);
        Debug.Log(pos);
    }

    private void setNormal()
    {
        btnTree.zimg.color = Color.white;
        btnHouse.zimg.color = Color.white;
        btnHouse1.zimg.color = Color.white;
        delBtn.zimg.color = Color.white;
        delcBtn.zimg.color = Color.white;
        exitBtn.zimg.color = Color.white;
    }

    public override void OnOpen()
    {
        RefreshPanel();
    }

    public override void OnHide()
    {

    }

    public override void OnClosing()
    {
        EventManager.Instance.RemoveObserver<EventGP_palceInfo>(PalceInfo);
    }

    private void RefreshPanel()
    {

    }

    private Vector2 GetUIPos(Vector3 worldPos)
    {
        Vector3 screenPos = UIManager.Instance.camera_scene.WorldToScreenPoint(worldPos);
        Debug.Log(screenPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gameObject.GetComponent<RectTransform>(),        // 你的UI面板 RectTransform
            screenPos,
            UIManager.Instance.uiCamera,           // ⚠️ 关键：Canvas对应的相机
            out Vector2 localPos
        );
        // 设置UI位置
       return localPos;
    }

}
