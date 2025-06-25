using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class AINode
{
    private Dictionary<string, string> stringMap = new Dictionary<string, string>();
    public int id;
    public Rect rect;
    public string title;
    public bool isDragged;
    public bool isSelected;
    public ConnectionPoint inPoint;
    public List<ConnectionPoint> outPoints;
    public AIStateType aIState;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;
    public GUIStyle inOutDefaultStyle;
    public GUIStyle inOutSelectStyle;
    public GUIStyle style;
    public Action<AINode> OnRemoveNode;
    private Vector2 winOffset = Vector2.zero;
    private GUIContent winGuiContent = new GUIContent("介绍与数据(reset表示返回根节点)");
    private GUIContent winGuiContent2 = new GUIContent("条件");
    //数据
    public AIStateData stateData;
    public TempAIStateData tempAIStateData;
    public AINode(AIStateType aIState, int id,Vector2 pos,
        Action<ConnectionPoint> OnClickInPoint,Action<ConnectionPoint> OnClickOutPoint, Action<AINode> OnClickRemoveNode)
    {
        InitStringMap();
        tempAIStateData = new TempAIStateData();
        this.aIState = aIState;
        this.id = id;
        defaultNodeStyle = new GUIStyle();
        selectedNodeStyle = new GUIStyle();
        inOutDefaultStyle = new GUIStyle();
        inOutSelectStyle = new GUIStyle();
        inOutDefaultStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "input.png") as Texture2D;
        inOutSelectStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "intput2.png") as Texture2D;
        style = defaultNodeStyle;

        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inOutDefaultStyle,inOutSelectStyle, OnClickInPoint);
        outPoints = new List<ConnectionPoint>();
        outPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out, inOutDefaultStyle, inOutSelectStyle, OnClickOutPoint));
        OnRemoveNode = OnClickRemoveNode;
        CreateByNodeType(pos);


    }

    private void InitStringMap()
    {
        stringMap.Add(CondType.canBeAttack__dis.ToString(), "近战攻击满足");
        stringMap.Add(CondType.canNotBeAttack__dis.ToString(), "近战攻击不满足");
        stringMap.Add(CondType.skyCanBeAttack__dis.ToString(), "(空中)近战攻击满足");
        stringMap.Add(CondType._f_canFarBeAttack__dis.ToString(), "远程攻击满足（米）");
        stringMap.Add(CondType._f_canNotFarBeAttack__dis.ToString(), "远程攻击不满足（米）");
        stringMap.Add(CondType.haveOtherFriend__enemy.ToString(), "有其他队友");
        stringMap.Add(CondType.haveNoOtherFriend__enemy.ToString(), "没有其他队友");
        stringMap.Add(CondType.haveNoEnemy__enemy.ToString(), "没有敌人存在");
        stringMap.Add(CondType.haveEnemy__enemy.ToString(), "有敌人存在");
        stringMap.Add(CondType.haveTargetEnemy__enemy.ToString(), "已有目标敌人存在");
        stringMap.Add(CondType.haveNoTargetEnemy__enemy.ToString(), "目标敌人不存在");
        stringMap.Add(CondType._m_keepTime__time.ToString(), "节点持续时间（毫秒）");
        stringMap.Add(CondType._h_lowHp__hp.ToString(), "低于血量百分比(小于)(0-100)");
        stringMap.Add(CondType._h_highHp__hp.ToString(), "高于血量百分比(大于等于)(0-100)");
        stringMap.Add(CondType._h_enemyLowHp__hp.ToString(), "目标低于血量百分比(小于)(0-100)");
        stringMap.Add(CondType._h_enemyHeighHp__hp.ToString(), "目标高于血量百分比(大于等于)(0-100)");
        stringMap.Add(CondType._m_keepLifeTime__time.ToString(), "ai存在时间大于（秒）");
        stringMap.Add(CondType._d_randKeepTime__time.ToString(), "随机节点持续时间(毫秒)");
        stringMap.Add(CondType._t_patrolTimes__times.ToString(), "巡逻次数满足（次数）");
        stringMap.Add(CondType.haveSkyEnemy__enemy.ToString(), "有空中敌人");
        stringMap.Add(CondType.haveNoSkyEnemy__enemy.ToString(), "没有空中敌人");
        stringMap.Add(CondType.haveGroundEnemy__enemy.ToString(), "有地面敌人");
        stringMap.Add(CondType.haveNoGroundEnemy__enemy.ToString(), "没有地面敌人");
        stringMap.Add(CondType._m_outCdTime__time.ToString(), "cd已完成（cd功能节点id）");
        stringMap.Add(CondType._m_intCdTime__time.ToString(), "cd未完成（cd功能节点id）");
        stringMap.Add(CondType._m_outGlobalCdTime__time.ToString(), "全局cd已完成（cd功能节点id）");
        stringMap.Add(CondType._m_intGlobalCdTime__time.ToString(), "全局cd未完成（cd功能节点id）");
        stringMap.Add(CondType._m_birthOver__time.ToString(), "出生超过几秒");
        stringMap.Add(CondType._m_birthNotOver__time.ToString(), "出生未超过几秒");
        stringMap.Add(CondType.isAttacking__attack.ToString(), "处于攻击中");
        stringMap.Add(CondType.isNotAttacking__attack.ToString(), "处于未攻击中");
        stringMap.Add(CondType._o_isLeader__other.ToString(), "是队长");
        stringMap.Add(CondType._o_isNotLeader__other.ToString(), "不是队长");
        stringMap.Add(CondType._m_disLeader__dis.ToString(), "离队长大于多少（米）");
        stringMap.Add(CondType.IsSelf__enemy.ToString(), "是自己");
        stringMap.Add(CondType.IsNotSelf__enemy.ToString(), "不是自己");
        stringMap.Add(CondType.findStop__other.ToString(), "rov停止");
    }

    //Idle=0,//待机
    //Patrol = 1,//巡邏
    //Finding = 2,//寻路
    //Attack_n = 3,//普通攻击
    //BeHurt,//受击
    //Disappear,//消失
    //Death//死亡



    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    private Rect winRect = new Rect(0,0,100,100);
    private Dictionary<int, CondWindow> condRects = new Dictionary<int, CondWindow>();
    private List<int> needRemoveCondRects = new List<int>();

    public List<CondWindow> GetCondList()
    {
        List<CondWindow> list = new List<CondWindow>();
        foreach (var item in condRects)
        {
            list.Add(item.Value);
        }
        return list;
    }

    public void SetCondList(List<CondWindow> condTypes)
    {
        condRects = new Dictionary<int, CondWindow>();
        for (int i = 0; i < condTypes.Count; i++)
        {
            condRects.Add(i, condTypes[i]);
        }
    }
    private static object obj = new object();
    public void Draw()
    {
        inPoint.Draw();
        for (int i = 0; i < outPoints.Count; i++)
        {
            outPoints[i].Draw();
        }
        GUI.Box(rect, title, style);



        if (isSelected)
        {
            winRect.position = Vector2.zero+Vector2.up*20;
            winRect = GUILayout.Window(999999, winRect, DoWindow, winGuiContent);
            if (condRects != null)
            {
                foreach (var item in condRects)
                {
 
                    if (!(condRects.ContainsKey(item.Value.lastID)))
                    {
                        item.Value.rect.position = new Vector2(0, winRect.position.y + winRect.size.y);
                    }
                    else
                    {
                        item.Value.rect.position = new Vector2(0, condRects[item.Value.lastID].rect.position.y + condRects[item.Value.lastID].rect.size.y);
                    }
                    condRects[item.Key].rect = GUILayout.Window(item.Key, condRects[item.Key].rect, DoCondWindow, winGuiContent2);
                }

                if (needRemoveCondRects.Count > 0)
                {
                    for (int i = 0; i < needRemoveCondRects.Count; i++)
                    {
                        var curCondWindow = condRects[needRemoveCondRects[i]];
                        if (condRects.ContainsKey(curCondWindow.nextID))
                        {
                            condRects[curCondWindow.nextID].lastID = curCondWindow.lastID;
                        }
                        condRects.Remove(needRemoveCondRects[i]);
                    }
                    needRemoveCondRects.Clear();
                }
            }
        }
    
    }

    private void DoCondWindow(int id)
    {
        CondWindow condWindow = condRects[id];

        GUILayout.BeginHorizontal();
        GUILayout.Label(condWindow.des);
        GUILayout.EndHorizontal();

        for (int i = 0; i < condWindow.list.Count; i++)
        {
            GUILayout.BeginHorizontal();
            if (condWindow.list[i].ToString().Contains("_t_"))
            {
                GUILayout.Label(stringMap[condWindow.list[i].ToString()]);
                condWindow.listData[i] = new Vector4(float.Parse(GUILayout.TextField(condWindow.listData[i].x.ToString())), 0, 0, 0);
            }
            else if (condWindow.list[i].ToString().Contains("_m_"))
            {
                GUILayout.Label(stringMap[condWindow.list[i].ToString()]);
                condWindow.listData[i] = new Vector4(float.Parse(GUILayout.TextField(condWindow.listData[i].x.ToString())), 0, 0, 0);
            }
            else if (condWindow.list[i].ToString().Contains("_h_"))
            {
                GUILayout.Label(stringMap[condWindow.list[i].ToString()]);
                condWindow.listData[i] = new Vector4(float.Parse(GUILayout.TextField(condWindow.listData[i].x.ToString())), 0, 0, 0);
            }
            else if (condWindow.list[i].ToString().Contains("_f_"))
            {
                GUILayout.Label(stringMap[condWindow.list[i].ToString()]);
                condWindow.listData[i] = new Vector4(float.Parse(GUILayout.TextField(condWindow.listData[i].x.ToString())),
                                                     float.Parse(GUILayout.TextField(condWindow.listData[i].y.ToString())), 0, 0);
            }
            else if (condWindow.list[i].ToString().Contains("_d_"))
            {
                GUILayout.Label(stringMap[condWindow.list[i].ToString()]);
                condWindow.listData[i] = new Vector4(float.Parse(GUILayout.TextField(condWindow.listData[i].x.ToString())),
                                                     float.Parse(GUILayout.TextField(condWindow.listData[i].y.ToString())),0,0);
            }
            else
            {
                GUILayout.Label(stringMap[condWindow.list[i].ToString()]);
            }

            if (GUILayout.Button("-"))
            {
                condWindow.list.Remove(condWindow.list[i]);
                condWindow.rect.size = Vector2.zero;
                GUILayout.EndHorizontal();
                return;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        
        GUILayout.Label("空");
        if (GUILayout.Button("删除组合"))
        {
            RemoveCondMenu(id);
        }
        if (GUILayout.Button("+"))
        {
            ProcessCondMenu(id);
        }
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private void DoWindow(int id)
    {
        GUILayout.BeginVertical();
        DrawNodeInfo(aIState);
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    private void RemoveCondMenu(int id)
    {
        needRemoveCondRects.Add(id);
    }

    private void ProcessCondMenu(int id)
    {
        GenericMenu genericMenu = new GenericMenu();
        foreach (var item in Enum.GetValues(typeof(CondType)))
        {
            if ((CondType)item == CondType.None) continue;
            int tempId = id;
            string fullName = item.ToString();
            string[] spitNames = fullName.Split("__");
            string name = spitNames[spitNames.Length - 1] + "/";
            name += stringMap[fullName];
            genericMenu.AddItem(new GUIContent(name), false, ()=> { OnClickAddCondition(tempId,(CondType)item); });
        }

        genericMenu.ShowAsContext();
    }

    private void OnClickAddCondition(int id,CondType condType)
    {
        condRects[id].Add(condType);
        Debug.Log("添加"+id);
    }

    private void CreateByNodeType(Vector2 pos)
    {
        Color color;
        ColorUtility.TryParseHtmlString("#C6C6C6", out color);
        defaultNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "nodeDefault.png") as Texture2D;
        selectedNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "nodeSelect.png") as Texture2D;
        defaultNodeStyle.contentOffset = new Vector2(6, 17.5f);
        selectedNodeStyle.contentOffset = new Vector2(6, 17.5f);
        defaultNodeStyle.normal.textColor = color;
        selectedNodeStyle.normal.textColor = color;
        rect = new Rect(pos.x, pos.y, 50, 50);
        NodeType tempNodeType = AICommonFunc.GetNodeTypeByAIState(aIState);

        switch (tempNodeType)
        {
            case NodeType.condition:
                rect = new Rect(pos.x, pos.y, 70, 70);
                defaultNodeStyle.contentOffset = new Vector2(21, 27);
                selectedNodeStyle.contentOffset = new Vector2(21, 27);
                defaultNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "conDefaut.png") as Texture2D;
                selectedNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "conSelect.png") as Texture2D;
                break;
        }

        switch (aIState)
        {
            case AIStateType.Root:
                title = "根节点";
                inPoint.isEnable = false;
                winOffset = new Vector2(-25, 56);
                winRect.size = new Vector2(100, 50);
                break;
            case AIStateType.RandomCompose:
                title = "随机节点";
                rect.size = new Vector2(100, 100);
                winOffset = new Vector2(0, 95);
                winRect.size = new Vector2(100, 50);
                defaultNodeStyle.contentOffset = new Vector2(33, 42);
                selectedNodeStyle.contentOffset = new Vector2(33, 42);
                defaultNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "randomDefaut.png") as Texture2D;
                selectedNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "randomSelect.png") as Texture2D;
                break;
            case AIStateType.OrderCompose:
                title = "顺序节点";
                rect.size = new Vector2(100, 100);
                defaultNodeStyle.contentOffset = new Vector2(21, 42);
                selectedNodeStyle.contentOffset = new Vector2(21, 42);
                defaultNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "orderDefaut.png") as Texture2D;
                selectedNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "orderSelect.png") as Texture2D;
                break;
            //case AIStateType.ParallelCompose:
            //    title = "平行组合";
            //    rect.size = new Vector2(100, 100);
            //    defaultNodeStyle.contentOffset = new Vector2(21, 42);
            //    selectedNodeStyle.contentOffset = new Vector2(21, 42);
            //    defaultNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "parallelDefaut.png") as Texture2D;
            //    selectedNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "parallelSelect.png") as Texture2D;
            //    break;
            case AIStateType.Idle:
                title = "普通待机";
                rect.size = new Vector2(60, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Fllow:
                title = "(近战)跟随";
                rect.size = new Vector2(70, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Fllow_Far:
                title = "(远程)跟随";
                rect.size = new Vector2(70, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Fllow_Leader:
                title = "跟随队长";
                rect.size = new Vector2(70, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Fixed_Point:
                title = "指定位置";
                rect.size = new Vector2(70, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Patrol:
                title = "跟随巡逻";
                rect.size = new Vector2(60, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_NormalPatrol:
                title = "普通巡逻";
                rect.size = new Vector2(60, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.common_meleeAttack:
                title = "通用近战攻击";
                rect.size = new Vector2(170, 50);
                break;
            case AIStateType.common_remoteAttack:
                title = "通用远程战攻击";
                rect.size = new Vector2(170, 50);
                break;
            case AIStateType.Condition:
                title = "条件";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.CD:
                title = "CD功能";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.GlobalCD:
                title = "全局CD";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.ShareCD:
                title = "共用CD";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.ResetLocalCD:
                title = "重置CD";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.ResetGlobalCD:
                title = "重置CD";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.ResetRoot:
                title = "返回根节点";
                winOffset = new Vector2(-70, 56);
                rect.size = new Vector2(70, 50);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.ChangeAI:
                title = "切换AI";
                winOffset = new Vector2(-70, 56);
                rect.size = new Vector2(70, 50);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.SearchEnemy:
                title = "搜索敌人";
                winOffset = new Vector2(-70, 56);
                rect.size = new Vector2(70, 50);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.SearchFriend:
                title = "搜索友军";
                winOffset = new Vector2(-70, 56);
                rect.size = new Vector2(70, 50);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Debug:
                title = "打印日志";
                rect.size = new Vector2(60, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
   
        }
    }

    private void DrawNodeInfo(AIStateType aIState)
    {
        GUILayout.Label("节点ID：" + id.ToString());
        switch (aIState)
        {
            case AIStateType.Root:
                GUILayout.Label("------------------------------");
                GUILayout.Label("至少需要一个根节点");
                break;
            case AIStateType.RandomCompose:
                GUILayout.Label("------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("随机运行所连接的条件");
                GUILayout.EndHorizontal();
   
                break;
            case AIStateType.OrderCompose:
                GUILayout.Label("------------------------------");
                GUILayout.Label("不需要任何条件就可以进入下一步");
                break;
            case AIStateType.Idle:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)最大持续时间(单位秒)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_Fllow:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)最大持续时间(单位秒)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("寻路间隔(单位秒)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_Fllow_Far:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)最大持续时间(单位秒)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("寻路间隔(单位秒)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_Fllow_Leader:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)最大持续时间(单位秒)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("寻路间隔(单位秒)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("距离多少停止(米)");
                tempAIStateData.stopDis = GUILayout.TextField(tempAIStateData.stopDis);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_Fixed_Point:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("寻路间隔(单位秒)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                GUILayout.Label(((FixedPointType)tempAIStateData.fixedPoint).ToString());
                if (GUILayout.Button("制定位置"))
                {
                    ProcessFixedPoint();
                }
                break;
            case AIStateType.Finding_Patrol:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)最大持续时间(单位秒)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("寻路间隔(单位秒)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_NormalPatrol:
                GUILayout.Label("-----------------------------------");
                //tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)最大持续时间(单位秒)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("寻路间隔(单位秒)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.common_meleeAttack:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("近战攻击id");
                tempAIStateData.meleeAttackID = GUILayout.TextField(tempAIStateData.meleeAttackID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("循环攻击次数");
                tempAIStateData.attackCount = GUILayout.TextField(tempAIStateData.attackCount);
                GUILayout.EndHorizontal();
                GUILayout.Label("循环攻击持续时间（0表示非循环动画）");
                tempAIStateData.attackKeepTime = GUILayout.TextField(tempAIStateData.attackKeepTime);
                tempAIStateData.followEnemyDir = GUILayout.Toggle(tempAIStateData.followEnemyDir, new GUIContent("跟随敌人方向"));
                tempAIStateData.isFarSkill = GUILayout.Toggle(tempAIStateData.isFarSkill, new GUIContent("是否为远程技能"));
                if (tempAIStateData.isFarSkill)
                {
                    tempAIStateData.serchType = tempAIStateData.serchType = GUILayout.SelectionGrid(tempAIStateData.serchType, new GUIContent[] { new GUIContent("DIS"),
                    new GUIContent("X") ,new GUIContent("Y") ,new GUIContent("HP_Min_percent"),new GUIContent("HP_Min_value"),
                    new GUIContent("Random"),new GUIContent("Far")
                    }, 1);
                    tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                    tempAIStateData.isFarBackTarget = GUILayout.Toggle(tempAIStateData.isFarBackTarget, new GUIContent("还原目标"));
                }



                break;
            case AIStateType.common_remoteAttack:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("远程攻击id");
                tempAIStateData.remoteAttackID = GUILayout.TextField(tempAIStateData.remoteAttackID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("循环攻击次数");
                tempAIStateData.attackCount = GUILayout.TextField(tempAIStateData.attackCount);
                GUILayout.EndHorizontal();
                GUILayout.Label("循环攻击持续时间（0表示非循环动画）");
                tempAIStateData.attackKeepTime = GUILayout.TextField(tempAIStateData.attackKeepTime);
                break;
            case AIStateType.Condition:
                GUILayout.BeginHorizontal();
                GUILayout.Label("节点sort：");
                tempAIStateData.sort = GUILayout.TextField(tempAIStateData.sort);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("添加条件组合"))
                {
                    AddCond("--------------------------------------------------");
                }
                break;
            case AIStateType.CD:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("cd(秒):");
                tempAIStateData.cd = GUILayout.TextField(tempAIStateData.cd);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ResetLocalCD:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("重置局部的ID：");
                tempAIStateData.resetID = GUILayout.TextField(tempAIStateData.resetID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("多少秒");
                tempAIStateData.cd = GUILayout.TextField(tempAIStateData.cd);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ResetGlobalCD:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("重置全局的ID：");
                tempAIStateData.resetID = GUILayout.TextField(tempAIStateData.resetID);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ShareCD:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("共用id:");
                tempAIStateData.shadreCDID = GUILayout.TextField(tempAIStateData.shadreCDID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("cd(秒):");
                tempAIStateData.cd = GUILayout.TextField(tempAIStateData.cd);
                GUILayout.EndHorizontal();

                break;
            case AIStateType.GlobalCD:
                GUILayout.Label("节点ID：" + id.ToString());
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("共用全局id:");
                tempAIStateData.shadreCDID = GUILayout.TextField(tempAIStateData.shadreCDID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("cd(秒):");
                tempAIStateData.cd = GUILayout.TextField(tempAIStateData.cd);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ResetRoot:
                GUILayout.Label("-----------------------------------");
                GUILayout.Label("返回根节点");
                break;
            case AIStateType.SearchEnemy:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                GUILayout.EndHorizontal();
                GUILayout.Label("选择搜索规则");
                GUILayout.BeginHorizontal();
                tempAIStateData.serchType = GUILayout.SelectionGrid(tempAIStateData.serchType, new GUIContent[] { new GUIContent("DIS"),
                new GUIContent("X") ,new GUIContent("Y") ,new GUIContent("HP_Min_percent"),new GUIContent("HP_Min_value"),
                new GUIContent("Random"),new GUIContent("Far")
                },1);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.SearchFriend:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("包含空中单位"));
                GUILayout.EndHorizontal();
                GUILayout.Label("选择搜索规则");
                GUILayout.BeginHorizontal();
                tempAIStateData.serchType = GUILayout.SelectionGrid(tempAIStateData.serchType, new GUIContent[] { new GUIContent("DIS"),
                new GUIContent("X") ,new GUIContent("Y") ,new GUIContent("HP_Min_percent"),new GUIContent("HP_Min_value")
                ,new GUIContent("Far")}, 1);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ChangeAI:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("切换表ID");
                tempAIStateData.changeID = GUILayout.TextField(tempAIStateData.changeID);
                GUILayout.EndHorizontal();
                tempAIStateData.isKeepCDByChangeAI = GUILayout.Toggle(tempAIStateData.isKeepCDByChangeAI, new GUIContent("是否保留全局cd，切换AI的时候"));


                break;
            case AIStateType.Debug:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("打印数据：");
                tempAIStateData.logStr = GUILayout.TextField(tempAIStateData.logStr);
                GUILayout.EndHorizontal();
                break;
        }
    }
    private void AddCond(string len)
    {
        condRects.Add(condRects.Count,new CondWindow(condRects.Count,len));
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }
                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                isDragged = false;
                break;
            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
            case EventType.ScrollWheel:
                if (e.delta.y < 0)
                {
                    Debug.Log("放大");
                }
                if(e.delta.y > 0)
                {
                    Debug.Log("缩小");
                }
                break;
            case EventType.KeyDown:
                ProcessKeyDown(e);
                break;

        }
        return false;
    }
    private void ProcessKeyDown(Event e)
    {
        switch (e.keyCode)
        {
            case KeyCode.Delete:
                if (isSelected)
                {
                    this.OnClickRemoveNode();
                }
                break;
        }

    }
    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("删除节点"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }
    //指定固定位置
    private void ProcessFixedPoint()
    {
        GenericMenu genericMenu = new GenericMenu();
        var names = Enum.GetValues(typeof(FixedPointType));
        foreach (var item in names)
        {
            genericMenu.AddItem(new GUIContent(((FixedPointType)item).ToString()), false, () => { OnClickFixedPoint((FixedPointType)item); });
        }
        genericMenu.ShowAsContext();
    }

    private void OnClickFixedPoint(FixedPointType pointType)
    {
        tempAIStateData.fixedPoint =(int)pointType;
    }

    private void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            OnRemoveNode(this);
        }
    }
}
