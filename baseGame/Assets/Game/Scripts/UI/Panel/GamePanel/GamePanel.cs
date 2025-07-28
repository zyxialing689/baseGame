using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public partial class GamePanel : BasePanel
{

    public override void Init(params object[] args)
    {
        base.Init(args);
        panelLayer = PanelLayer.Panel;
        adressPath = "Panel/GamePanel";
    }
    public override void OnShowing()
    {
        
        this.boxgreen.onClick.AddListener(() => {
            RoleFactory.GenerateRoleA(4);
        });

        this.boxred.onClick.AddListener(() => {
            RoleFactory.GenerateRoleB(4);
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

    }



}