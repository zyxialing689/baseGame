using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public partial class EnterPanel : BasePanel
{
    [Inject] public ITestMgr testMgr;
    public override void Init(params object[] args)
    {
        base.Init(args);
        panelLayer = PanelLayer.Overlay;
        adressPath = "Overlay/EnterPanel";
    }
    public override void OnShowing()
    {
       enterBtn.onClick.AddListener(()=>{
         ZLogUtil.Log(testMgr.GetTestStr());
       });
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
     
    }

    private void RefreshPanel()
    {
        AudioManager.GetInstance().PlayBgmSound(GameConst.const_bgm1);
        ExcelConfig.Instance.LoadAllExcel();
        var data = ExcelConfig.Get_excel_roledata(1);
        ZLogUtil.Log(data.anim_path);
    }

}