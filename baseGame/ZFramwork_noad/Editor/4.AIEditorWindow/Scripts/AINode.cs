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
    private GUIContent winGuiContent = new GUIContent("����������(reset��ʾ���ظ��ڵ�)");
    private GUIContent winGuiContent2 = new GUIContent("����");
    //����
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
        stringMap.Add(CondType.canBeAttack__dis.ToString(), "��ս��������");
        stringMap.Add(CondType.canNotBeAttack__dis.ToString(), "��ս����������");
        stringMap.Add(CondType.skyCanBeAttack__dis.ToString(), "(����)��ս��������");
        stringMap.Add(CondType._f_canFarBeAttack__dis.ToString(), "Զ�̹������㣨�ף�");
        stringMap.Add(CondType._f_canNotFarBeAttack__dis.ToString(), "Զ�̹��������㣨�ף�");
        stringMap.Add(CondType.haveOtherFriend__enemy.ToString(), "����������");
        stringMap.Add(CondType.haveNoOtherFriend__enemy.ToString(), "û����������");
        stringMap.Add(CondType.haveNoEnemy__enemy.ToString(), "û�е��˴���");
        stringMap.Add(CondType.haveEnemy__enemy.ToString(), "�е��˴���");
        stringMap.Add(CondType.haveTargetEnemy__enemy.ToString(), "����Ŀ����˴���");
        stringMap.Add(CondType.haveNoTargetEnemy__enemy.ToString(), "Ŀ����˲�����");
        stringMap.Add(CondType._m_keepTime__time.ToString(), "�ڵ����ʱ�䣨���룩");
        stringMap.Add(CondType._h_lowHp__hp.ToString(), "����Ѫ���ٷֱ�(С��)(0-100)");
        stringMap.Add(CondType._h_highHp__hp.ToString(), "����Ѫ���ٷֱ�(���ڵ���)(0-100)");
        stringMap.Add(CondType._h_enemyLowHp__hp.ToString(), "Ŀ�����Ѫ���ٷֱ�(С��)(0-100)");
        stringMap.Add(CondType._h_enemyHeighHp__hp.ToString(), "Ŀ�����Ѫ���ٷֱ�(���ڵ���)(0-100)");
        stringMap.Add(CondType._m_keepLifeTime__time.ToString(), "ai����ʱ����ڣ��룩");
        stringMap.Add(CondType._d_randKeepTime__time.ToString(), "����ڵ����ʱ��(����)");
        stringMap.Add(CondType._t_patrolTimes__times.ToString(), "Ѳ�ߴ������㣨������");
        stringMap.Add(CondType.haveSkyEnemy__enemy.ToString(), "�п��е���");
        stringMap.Add(CondType.haveNoSkyEnemy__enemy.ToString(), "û�п��е���");
        stringMap.Add(CondType.haveGroundEnemy__enemy.ToString(), "�е������");
        stringMap.Add(CondType.haveNoGroundEnemy__enemy.ToString(), "û�е������");
        stringMap.Add(CondType._m_outCdTime__time.ToString(), "cd����ɣ�cd���ܽڵ�id��");
        stringMap.Add(CondType._m_intCdTime__time.ToString(), "cdδ��ɣ�cd���ܽڵ�id��");
        stringMap.Add(CondType._m_outGlobalCdTime__time.ToString(), "ȫ��cd����ɣ�cd���ܽڵ�id��");
        stringMap.Add(CondType._m_intGlobalCdTime__time.ToString(), "ȫ��cdδ��ɣ�cd���ܽڵ�id��");
        stringMap.Add(CondType._m_birthOver__time.ToString(), "������������");
        stringMap.Add(CondType._m_birthNotOver__time.ToString(), "����δ��������");
        stringMap.Add(CondType.isAttacking__attack.ToString(), "���ڹ�����");
        stringMap.Add(CondType.isNotAttacking__attack.ToString(), "����δ������");
        stringMap.Add(CondType._o_isLeader__other.ToString(), "�Ƕӳ�");
        stringMap.Add(CondType._o_isNotLeader__other.ToString(), "���Ƕӳ�");
        stringMap.Add(CondType._m_disLeader__dis.ToString(), "��ӳ����ڶ��٣��ף�");
        stringMap.Add(CondType.IsSelf__enemy.ToString(), "���Լ�");
        stringMap.Add(CondType.IsNotSelf__enemy.ToString(), "�����Լ�");
        stringMap.Add(CondType.findStop__other.ToString(), "rovֹͣ");
    }

    //Idle=0,//����
    //Patrol = 1,//Ѳ߉
    //Finding = 2,//Ѱ·
    //Attack_n = 3,//��ͨ����
    //BeHurt,//�ܻ�
    //Disappear,//��ʧ
    //Death//����



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
        
        GUILayout.Label("��");
        if (GUILayout.Button("ɾ�����"))
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
        Debug.Log("���"+id);
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
                title = "���ڵ�";
                inPoint.isEnable = false;
                winOffset = new Vector2(-25, 56);
                winRect.size = new Vector2(100, 50);
                break;
            case AIStateType.RandomCompose:
                title = "����ڵ�";
                rect.size = new Vector2(100, 100);
                winOffset = new Vector2(0, 95);
                winRect.size = new Vector2(100, 50);
                defaultNodeStyle.contentOffset = new Vector2(33, 42);
                selectedNodeStyle.contentOffset = new Vector2(33, 42);
                defaultNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "randomDefaut.png") as Texture2D;
                selectedNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "randomSelect.png") as Texture2D;
                break;
            case AIStateType.OrderCompose:
                title = "˳��ڵ�";
                rect.size = new Vector2(100, 100);
                defaultNodeStyle.contentOffset = new Vector2(21, 42);
                selectedNodeStyle.contentOffset = new Vector2(21, 42);
                defaultNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "orderDefaut.png") as Texture2D;
                selectedNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "orderSelect.png") as Texture2D;
                break;
            //case AIStateType.ParallelCompose:
            //    title = "ƽ�����";
            //    rect.size = new Vector2(100, 100);
            //    defaultNodeStyle.contentOffset = new Vector2(21, 42);
            //    selectedNodeStyle.contentOffset = new Vector2(21, 42);
            //    defaultNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "parallelDefaut.png") as Texture2D;
            //    selectedNodeStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(AIEditorWindow.editorUIPath + "parallelSelect.png") as Texture2D;
            //    break;
            case AIStateType.Idle:
                title = "��ͨ����";
                rect.size = new Vector2(60, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Fllow:
                title = "(��ս)����";
                rect.size = new Vector2(70, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Fllow_Far:
                title = "(Զ��)����";
                rect.size = new Vector2(70, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Fllow_Leader:
                title = "����ӳ�";
                rect.size = new Vector2(70, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Fixed_Point:
                title = "ָ��λ��";
                rect.size = new Vector2(70, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_Patrol:
                title = "����Ѳ��";
                rect.size = new Vector2(60, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Finding_NormalPatrol:
                title = "��ͨѲ��";
                rect.size = new Vector2(60, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.common_meleeAttack:
                title = "ͨ�ý�ս����";
                rect.size = new Vector2(170, 50);
                break;
            case AIStateType.common_remoteAttack:
                title = "ͨ��Զ��ս����";
                rect.size = new Vector2(170, 50);
                break;
            case AIStateType.Condition:
                title = "����";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.CD:
                title = "CD����";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.GlobalCD:
                title = "ȫ��CD";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.ShareCD:
                title = "����CD";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.ResetLocalCD:
                title = "����CD";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.ResetGlobalCD:
                title = "����CD";
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                defaultNodeStyle.contentOffset = new Vector2(14, 27);
                selectedNodeStyle.contentOffset = new Vector2(14, 27);
                break;
            case AIStateType.ResetRoot:
                title = "���ظ��ڵ�";
                winOffset = new Vector2(-70, 56);
                rect.size = new Vector2(70, 50);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.ChangeAI:
                title = "�л�AI";
                winOffset = new Vector2(-70, 56);
                rect.size = new Vector2(70, 50);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.SearchEnemy:
                title = "��������";
                winOffset = new Vector2(-70, 56);
                rect.size = new Vector2(70, 50);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.SearchFriend:
                title = "�����Ѿ�";
                winOffset = new Vector2(-70, 56);
                rect.size = new Vector2(70, 50);
                winRect.size = new Vector2(200, 50);
                break;
            case AIStateType.Debug:
                title = "��ӡ��־";
                rect.size = new Vector2(60, 50);
                winOffset = new Vector2(-70, 56);
                winRect.size = new Vector2(200, 50);
                break;
   
        }
    }

    private void DrawNodeInfo(AIStateType aIState)
    {
        GUILayout.Label("�ڵ�ID��" + id.ToString());
        switch (aIState)
        {
            case AIStateType.Root:
                GUILayout.Label("------------------------------");
                GUILayout.Label("������Ҫһ�����ڵ�");
                break;
            case AIStateType.RandomCompose:
                GUILayout.Label("------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("������������ӵ�����");
                GUILayout.EndHorizontal();
   
                break;
            case AIStateType.OrderCompose:
                GUILayout.Label("------------------------------");
                GUILayout.Label("����Ҫ�κ������Ϳ��Խ�����һ��");
                break;
            case AIStateType.Idle:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)������ʱ��(��λ��)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_Fllow:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)������ʱ��(��λ��)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ѱ·���(��λ��)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_Fllow_Far:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)������ʱ��(��λ��)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ѱ·���(��λ��)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_Fllow_Leader:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)������ʱ��(��λ��)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ѱ·���(��λ��)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("�������ֹͣ(��)");
                tempAIStateData.stopDis = GUILayout.TextField(tempAIStateData.stopDis);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_Fixed_Point:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ѱ·���(��λ��)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                GUILayout.Label(((FixedPointType)tempAIStateData.fixedPoint).ToString());
                if (GUILayout.Button("�ƶ�λ��"))
                {
                    ProcessFixedPoint();
                }
                break;
            case AIStateType.Finding_Patrol:
                GUILayout.Label("-----------------------------------");
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)������ʱ��(��λ��)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ѱ·���(��λ��)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.Finding_NormalPatrol:
                GUILayout.Label("-----------------------------------");
                //tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                GUILayout.BeginHorizontal();
                GUILayout.Label("(reset)������ʱ��(��λ��)");
                tempAIStateData.keepTime = GUILayout.TextField(tempAIStateData.keepTime);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ѱ·���(��λ��)");
                tempAIStateData.findInterval = GUILayout.TextField(tempAIStateData.findInterval);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.common_meleeAttack:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("��ս����id");
                tempAIStateData.meleeAttackID = GUILayout.TextField(tempAIStateData.meleeAttackID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ѭ����������");
                tempAIStateData.attackCount = GUILayout.TextField(tempAIStateData.attackCount);
                GUILayout.EndHorizontal();
                GUILayout.Label("ѭ����������ʱ�䣨0��ʾ��ѭ��������");
                tempAIStateData.attackKeepTime = GUILayout.TextField(tempAIStateData.attackKeepTime);
                tempAIStateData.followEnemyDir = GUILayout.Toggle(tempAIStateData.followEnemyDir, new GUIContent("������˷���"));
                tempAIStateData.isFarSkill = GUILayout.Toggle(tempAIStateData.isFarSkill, new GUIContent("�Ƿ�ΪԶ�̼���"));
                if (tempAIStateData.isFarSkill)
                {
                    tempAIStateData.serchType = tempAIStateData.serchType = GUILayout.SelectionGrid(tempAIStateData.serchType, new GUIContent[] { new GUIContent("DIS"),
                    new GUIContent("X") ,new GUIContent("Y") ,new GUIContent("HP_Min_percent"),new GUIContent("HP_Min_value"),
                    new GUIContent("Random"),new GUIContent("Far")
                    }, 1);
                    tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                    tempAIStateData.isFarBackTarget = GUILayout.Toggle(tempAIStateData.isFarBackTarget, new GUIContent("��ԭĿ��"));
                }



                break;
            case AIStateType.common_remoteAttack:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Զ�̹���id");
                tempAIStateData.remoteAttackID = GUILayout.TextField(tempAIStateData.remoteAttackID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ѭ����������");
                tempAIStateData.attackCount = GUILayout.TextField(tempAIStateData.attackCount);
                GUILayout.EndHorizontal();
                GUILayout.Label("ѭ����������ʱ�䣨0��ʾ��ѭ��������");
                tempAIStateData.attackKeepTime = GUILayout.TextField(tempAIStateData.attackKeepTime);
                break;
            case AIStateType.Condition:
                GUILayout.BeginHorizontal();
                GUILayout.Label("�ڵ�sort��");
                tempAIStateData.sort = GUILayout.TextField(tempAIStateData.sort);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("����������"))
                {
                    AddCond("--------------------------------------------------");
                }
                break;
            case AIStateType.CD:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("cd(��):");
                tempAIStateData.cd = GUILayout.TextField(tempAIStateData.cd);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ResetLocalCD:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("���þֲ���ID��");
                tempAIStateData.resetID = GUILayout.TextField(tempAIStateData.resetID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("������");
                tempAIStateData.cd = GUILayout.TextField(tempAIStateData.cd);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ResetGlobalCD:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("����ȫ�ֵ�ID��");
                tempAIStateData.resetID = GUILayout.TextField(tempAIStateData.resetID);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ShareCD:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("����id:");
                tempAIStateData.shadreCDID = GUILayout.TextField(tempAIStateData.shadreCDID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("cd(��):");
                tempAIStateData.cd = GUILayout.TextField(tempAIStateData.cd);
                GUILayout.EndHorizontal();

                break;
            case AIStateType.GlobalCD:
                GUILayout.Label("�ڵ�ID��" + id.ToString());
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("����ȫ��id:");
                tempAIStateData.shadreCDID = GUILayout.TextField(tempAIStateData.shadreCDID);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("cd(��):");
                tempAIStateData.cd = GUILayout.TextField(tempAIStateData.cd);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ResetRoot:
                GUILayout.Label("-----------------------------------");
                GUILayout.Label("���ظ��ڵ�");
                break;
            case AIStateType.SearchEnemy:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                GUILayout.EndHorizontal();
                GUILayout.Label("ѡ����������");
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
                tempAIStateData.findCotainSky = GUILayout.Toggle(tempAIStateData.findCotainSky, new GUIContent("�������е�λ"));
                GUILayout.EndHorizontal();
                GUILayout.Label("ѡ����������");
                GUILayout.BeginHorizontal();
                tempAIStateData.serchType = GUILayout.SelectionGrid(tempAIStateData.serchType, new GUIContent[] { new GUIContent("DIS"),
                new GUIContent("X") ,new GUIContent("Y") ,new GUIContent("HP_Min_percent"),new GUIContent("HP_Min_value")
                ,new GUIContent("Far")}, 1);
                GUILayout.EndHorizontal();
                break;
            case AIStateType.ChangeAI:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("�л���ID");
                tempAIStateData.changeID = GUILayout.TextField(tempAIStateData.changeID);
                GUILayout.EndHorizontal();
                tempAIStateData.isKeepCDByChangeAI = GUILayout.Toggle(tempAIStateData.isKeepCDByChangeAI, new GUIContent("�Ƿ���ȫ��cd���л�AI��ʱ��"));


                break;
            case AIStateType.Debug:
                GUILayout.Label("-----------------------------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("��ӡ���ݣ�");
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
                    Debug.Log("�Ŵ�");
                }
                if(e.delta.y > 0)
                {
                    Debug.Log("��С");
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
        genericMenu.AddItem(new GUIContent("ɾ���ڵ�"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }
    //ָ���̶�λ��
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
