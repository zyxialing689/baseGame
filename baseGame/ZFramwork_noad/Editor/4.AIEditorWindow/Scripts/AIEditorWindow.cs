using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;


public enum NodeIdleType
{
    IDLE0 = 0,
    IDLE1,
    IDLE2,
}
public class AIEditorWindow : EditorWindow
{
    private List<AINode> nodes;
    private List<Connection> connections;
    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;
    private GUIStyle greenStyle;
    private GUIStyle redStyle;
    private GUIStyle textWhiteColorStyle;
    private GUIStyle textGreenColorStyle;
    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;
    private Vector2 offset;
    private Vector2 drag;
    private static int saveID = 0;

    [MenuItem("ZFramework/Window/AIEditorWindow")]
    private static void OpenWindow()
    {
        var window = GetWindow<AIEditorWindow>();
        window.titleContent = new GUIContent("AIEditorWindow");
        string path =  SaveUtils.GetStringForKey("AISavePath");
        if (!string.IsNullOrEmpty(savePath))
        {
            savePath = path;
        }
        else
        {
            savePath = localPath;
        }

    }
    public const string editorUIPath = "Packages/com.zyxialing.zframework/Editor/4.AIEditorWindow/UI/";
    private void OnEnable()
    {
        textWhiteColorStyle = new GUIStyle();
        textGreenColorStyle = new GUIStyle();
        textWhiteColorStyle.normal.textColor = Color.white;
        textGreenColorStyle.normal.textColor = Color.green;
        greenStyle = new GUIStyle();
        redStyle = new GUIStyle();
        greenStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(editorUIPath + "green.png") as Texture2D;
        redStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(editorUIPath + "gray.png") as Texture2D;
        //nodeStyle.border = new RectOffset(12, 12, 12, 12);

        //selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(editorUIPath + "node.png") as Texture2D;
        inPointStyle.active.background = AssetDatabase.LoadMainAssetAtPath(editorUIPath + "node.png") as Texture2D;
        //inPointStyle.border = new RectOffset(4, 4, 12, 12);
        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = AssetDatabase.LoadMainAssetAtPath(editorUIPath + "node.png") as Texture2D;
        outPointStyle.active.background = AssetDatabase.LoadMainAssetAtPath(editorUIPath + "node.png") as Texture2D;
        //outPointStyle.border = new RectOffset(4, 4, 12, 12);
    }

    bool isRealyShow;
    string realyShowStr;
    GUIStyle realyShowGui;
    GUIStyle realyShowTxtGui;
    AIAgent selectAgent;
    string selectLoadPath;
    static List<AINode> RedNodes = new List<AINode>();
    private void OnGUI()
    {
        BeginWindows();
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        BeginWindows();
        DrawNodes();
        EndWindows();
        DrawConnections();
        DrawConnectionLine(Event.current);
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);
        EndWindows();

        GUILayout.BeginHorizontal();
        savePath = GUILayout.TextField(savePath);
        if (GUILayout.Button("打开目录",GUILayout.Width(200)))
        {
            string path = EditorUtility.OpenFilePanel("Load png Textures", "", "");
            string paths = "Assets" + path.Split("Assets")[1];
            savePath = paths;
        }
        if (GUILayout.Button("保存"))
        {
            SaveUtils.SetStringForKey("AISavePath", savePath);
            Save();
        }
        if (GUILayout.Button("载入"))
        {
            SaveUtils.SetStringForKey("AISavePath", savePath);
            Load();
        }
        if (isRealyShow)
        {
            realyShowStr = "关闭实时监控";
            realyShowGui = greenStyle;
            realyShowTxtGui = textGreenColorStyle;
        }
        else
        {
            realyShowStr = "开启实时监控";
            realyShowGui = redStyle;
            realyShowTxtGui = textWhiteColorStyle;
        }
        if (GUILayout.Button(realyShowStr, realyShowGui, GUILayout.Height(20), GUILayout.Width(100)))
        {
            isRealyShow = !isRealyShow;
        }
        GUILayout.EndHorizontal();
        RedNodes.Clear();
        if (isRealyShow&& Selection.activeGameObject != null)
        {
            selectAgent = Selection.activeGameObject.GetComponent<AIAgent>();
            if (selectAgent != null)
            {
                if (selectAgent._aiStates != null)
                {
                    savePath = selectAgent.agentData.ai_path+".txt";
                    if (selectLoadPath != savePath)
                    {
                        Load();
                        selectLoadPath = savePath;
                    }
                    foreach (var item in selectAgent._aiStates)
                    {
                        if(nodes!=null)
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            if(nodes[i].id == item.Value.stateData.id)
                            {
                               RedNodes.Add(nodes[i]);
                            }
                        }
                    }
                        if (nodes != null)
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            bool isRed = false;
                            for (int j = 0; j < RedNodes.Count; j++)
                            {
                                if (nodes[i].id == RedNodes[j].id)
                                {
                                    isRed = true;
                                    break;
                                }
                            }
                            if (isRed)
                            {
                                nodes[i].defaultNodeStyle.normal.textColor = Color.red;
                            }
                            else
                            {
                                nodes[i].defaultNodeStyle.normal.textColor = Color.white;
                            }
                        }
                }
            }
            else
            {
                if(nodes!=null)
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].defaultNodeStyle.normal.textColor = Color.white;
                }
            }
        }
        else
        {
            if (nodes != null)
                for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].defaultNodeStyle.normal.textColor = Color.white;
            }

        }
        GUI.changed = true;
        if (GUI.changed) Repaint();
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);
        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);
        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }
        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }
        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
            selectedInPoint.rect.center,
            e.mousePosition,
            selectedInPoint.rect.center + Vector2.left * 50f,
            e.mousePosition - Vector2.left * 50f,
            Color.white,
            null,
            2f
            );
            GUI.changed = true;
        }
        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
            selectedOutPoint.rect.center,
            e.mousePosition,
            selectedOutPoint.rect.center - Vector2.left * 50f,
            e.mousePosition + Vector2.left * 50f,
            Color.white,
            null,
            2f
            );
            GUI.changed = true;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);
                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    ClearConnectionSelection();
                }
                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;
            case EventType.MouseDrag:
                OnDrag(e.delta);
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Space)
                {
                    drag = Vector2.zero;
                }
                break;
        }
    }


    private void OnDrag(Vector2 delta)
    {
        drag = delta;
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }
        GUI.changed = true;
    }
    public static string[] NodeCNNames = new string[] {
     "根节点","组合节点","待机","寻路","攻击","条件","返回根节点","切换AI","索敌"
    };
    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();

        foreach (var item in Enum.GetValues(typeof(NodeType)))
        {
            NodeType curType = (NodeType)item;
            AIStateType[] aIStateTypes = GetAllStates(curType);

            for (int i = 0; i < aIStateTypes.Length; i++)
            {
                AIStateType tempStype = aIStateTypes[i];
                Vector2 pos = mousePosition;
                if(curType== NodeType.attack)
                {
                       string attackPath = "创建" + NodeCNNames[(int)item];
                       string[] prefixNames = aIStateTypes[i].ToString().Split('_');
                        for (int j = 0; j < prefixNames.Length; j++)
                        {
                           attackPath += "/" + prefixNames[j];
                        }

                       genericMenu.AddItem(new GUIContent(attackPath),
                       false, () => {
                           OnClickAddNode(tempStype, pos);
                       }
                       );
                }
                else
                {
                    if (aIStateTypes.Length > 1)
                    {
                        genericMenu.AddItem(new GUIContent("创建" + NodeCNNames[(int)item] + "/" + aIStateTypes[i].ToString()),
                        false, () => {
                            OnClickAddNode(tempStype, pos);
                        }
                        );
                    }
                    else
                    {
                        genericMenu.AddItem(new GUIContent("创建" + NodeCNNames[(int)item] + aIStateTypes[i].ToString()),
                        false, () => { OnClickAddNode(tempStype, pos); }
                        );
                    }
                }
            }
        }
        
        genericMenu.ShowAsContext();
    }

    private AIStateType[] GetAllStates(NodeType nodeType)
    {
        List<AIStateType> ais = new List<AIStateType>();
        int basic = ((int)nodeType+1)*1000;
        ais = ADDState(basic);
        return ais.ToArray();
    }

    private List<AIStateType> ADDState(int basic)
    {
        List<AIStateType> ais = new List<AIStateType>();
        foreach (var item in Enum.GetValues(typeof(AIStateType)))
        {
            int index = ((int)item);
            if (index>=basic-1000&&index<basic)
            {
                ais.Add((AIStateType)item);
            }
        }
        return ais;
    }

    private void OnClickAddNode(AIStateType aIState, Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<AINode>();
        }
        //if(aIState == AIStateType.Root)
        //{
        //    for (int i = 0; i < nodes.Count; i++)
        //    {
        //        if (nodes[i].aIState == AIStateType.Root)
        //        {
        //            Debug.LogError("一个状态机只允许一个根节点");
        //            return;
        //        }
        //    }
        //}


        saveID = saveID + 1;
        if (ContainId(saveID))
        {
            OnClickAddNode(aIState,mousePosition);
        }
        else
        {
            nodes.Add(new AINode(aIState, saveID, mousePosition, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
        }

    }

    private bool ContainId(int id)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i].id == id)
            {
                return true;
            }
        }
        return false;
    }

    private void OnClickRemoveNode(AINode node)
    {
        if (connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == node.inPoint ||node.outPoints.Contains(connections[i].outPoint))
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }
            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connectionsToRemove[i].inPoint.isConnection = false;
                connectionsToRemove[i].outPoint.isConnection = false;
                connections.Remove(connectionsToRemove[i]);
                connectionsToRemove[i].outPoint.node.outPoints.Remove(connectionsToRemove[i].outPoint);
            }
            connectionsToRemove = null;
        }
        nodes.Remove(node);
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;
        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                if (CreateConnection())
                {
                    if (selectedInPoint != null)
                        selectedInPoint.isConnection = true;
                    if (selectedOutPoint != null)
                    {
                        selectedOutPoint.isConnection = true;
                        selectedOutPoint.node.outPoints.Add(selectedOutPoint);
                    }
                  
                }
                ClearConnectionSelection();

            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }
    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;
        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                if (CreateConnection())
                {
                    if (selectedInPoint != null)
                        selectedInPoint.isConnection = true;
                    if (selectedOutPoint != null) {
                        selectedOutPoint.isConnection = true;
                        selectedOutPoint.node.outPoints.Add(selectedOutPoint);
                    }


                }
                ClearConnectionSelection();

            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }
    private void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
        bool tempBool = false;
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].inPoint.node == connection.inPoint.node)
            {
                tempBool = true;
            }
        }
        connection.inPoint.isConnection = tempBool;
        tempBool = false;
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].outPoint.node == connection.outPoint.node)
            {
                tempBool = true;
            }
        }
        connection.outPoint.node.outPoints.Remove(connection.outPoint);
        connection.outPoint.isConnection = tempBool;
      
    }
    private bool CreateConnection()
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }
       
       for (int i = 0; i < connections.Count; i++)
        {
            if(connections[i].inPoint.node == selectedInPoint.node&&connections[i].outPoint.node == selectedOutPoint.node)
            {
                return false;
            }
        }
        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
        return true;
    }
    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }
    private void DrawNodes()
    {
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }
    public static string savePath = "";
    public const string localPath = "Assets/Game/AssetDynamic/Config/AI/";
    private void Save()
    {
        AIDataJson aIDataJson = new AIDataJson();
        List<AINodeDataJson> aINodeDatas = new List<AINodeDataJson>();
        List<AIConectionDataJson> aIConectionDataJsons = new List<AIConectionDataJson>();
        for (int i = 0; i < nodes.Count; i++)
        {
            AINode aI = nodes[i];
            AINodeDataJson andJson = new AINodeDataJson();
            andJson.aIState = aI.aIState;
            andJson.condRects = aI.GetCondList();
            andJson.id = aI.id;
            andJson.pos = aI.rect.position;
            andJson.logStr = aI.tempAIStateData.logStr;
            int.TryParse(aI.tempAIStateData.changeID, out andJson.changeID);
            float.TryParse(aI.tempAIStateData.keepTime,out andJson.keepTime);
            float.TryParse(aI.tempAIStateData.findInterval, out andJson.findInterval);
            float.TryParse(aI.tempAIStateData.stopDis, out andJson.stopDis);
            float.TryParse(aI.tempAIStateData.cd, out andJson.cd);
            int.TryParse(aI.tempAIStateData.resetID, out andJson.resetID);
            int.TryParse(aI.tempAIStateData.shadreCDID, out andJson.shadreCDID);
            andJson.findCotainSky = aI.tempAIStateData.findCotainSky;
            andJson.serchType = aI.tempAIStateData.serchType;
            andJson.isFarSkill = aI.tempAIStateData.isFarSkill;
            andJson.followEnemyDir = aI.tempAIStateData.followEnemyDir;
            andJson.isKeepCDByChangeAI = aI.tempAIStateData.isKeepCDByChangeAI;
            andJson.isFinishCDByChangeAI = aI.tempAIStateData.isFinishCDByChangeAI;
            int.TryParse(aI.tempAIStateData.patrolTimes,out andJson.patrolTimes);
            int.TryParse(aI.tempAIStateData.meleeAttackID, out andJson.meleeAttackID);
            float.TryParse(aI.tempAIStateData.attackKeepTime, out andJson.attackKeepTime);
            int.TryParse(aI.tempAIStateData.remoteAttackID,out andJson.remoteAttackID);
            int.TryParse(aI.tempAIStateData.attackCount, out andJson.attackCount);
            int.TryParse(aI.tempAIStateData.sort, out andJson.sort);
            andJson.fixedPoint = aI.tempAIStateData.fixedPoint;
            andJson.followEnemyDir = aI.tempAIStateData.followEnemyDir;
            aINodeDatas.Add(andJson);
        }
        aIDataJson.aINodeDatas = aINodeDatas;
        if (connections.Count == 0)
        {
            Debug.LogError("不允许空连接");
            return;
        }
        for (int i = 0; i < connections.Count; i++)
        {
            AIConectionDataJson acjson = new AIConectionDataJson();
            AIConectionPointJson injson = new AIConectionPointJson();
            AIConectionPointJson outjson = new AIConectionPointJson();
            injson.inType = connections[i].inPoint.type;
            injson.isInConnection = connections[i].inPoint.isConnection;
            injson.isInEnable = connections[i].inPoint.isEnable;
            injson.nodeId = connections[i].inPoint.node.id;
            outjson.inType = connections[i].outPoint.type;
            outjson.isInConnection = connections[i].outPoint.isConnection;
            outjson.isInEnable = connections[i].outPoint.isEnable;
            outjson.nodeId = connections[i].outPoint.node.id;
            acjson.inCP = injson;
            acjson.outCP = outjson;

            aIConectionDataJsons.Add(acjson);

            AINodeDataJson curNode = aIDataJson.GetNodeDataById(outjson.nodeId);
            if (curNode.nextIds == null)
            {
                curNode.nextIds = new List<int>();
            }
            curNode.nextIds.Add(injson.nodeId);
        }
 
        aIDataJson.aIConectionDataJsons = aIConectionDataJsons;
        Debug.Log(aIDataJson.ToJsonStr());
        File.WriteAllText(savePath, aIDataJson.ToJsonStr());
        AssetDatabase.Refresh();
    }

    private void Load()
    {

        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(savePath);
        AIDataJson aIDataJson = JsonUtility.FromJson<AIDataJson>(textAsset.text);
        nodes = new List<AINode>();
        for (int i = 0; i < aIDataJson.aINodeDatas.Count; i++)
        {
            AINodeDataJson andJson = aIDataJson.aINodeDatas[i];
            AINode aINode = new AINode(andJson.aIState, andJson.id, andJson.pos, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
            aINode.SetCondList(andJson.condRects);
            aINode.tempAIStateData.keepTime =andJson.keepTime.ToString();
            aINode.tempAIStateData.findInterval = andJson.findInterval.ToString();
            aINode.tempAIStateData.stopDis = andJson.stopDis.ToString();
            aINode.tempAIStateData.sort = andJson.sort.ToString();
            aINode.tempAIStateData.resetID = andJson.resetID.ToString();
            aINode.tempAIStateData.cd = andJson.cd.ToString();
            aINode.tempAIStateData.shadreCDID = andJson.shadreCDID.ToString();
            aINode.tempAIStateData.isKeepCDByChangeAI = andJson.isKeepCDByChangeAI;
            aINode.tempAIStateData.isFinishCDByChangeAI = andJson.isFinishCDByChangeAI;
            aINode.tempAIStateData.serchType = andJson.serchType;
            aINode.tempAIStateData.isFarSkill = andJson.isFarSkill;
            aINode.tempAIStateData.isFarBackTarget = andJson.isFarBackTarget;
            aINode.tempAIStateData.findCotainSky = andJson.findCotainSky;
            aINode.tempAIStateData.followEnemyDir = andJson.followEnemyDir;
            aINode.tempAIStateData.patrolTimes = andJson.patrolTimes.ToString();
            aINode.tempAIStateData.logStr = andJson.logStr;
            aINode.tempAIStateData.changeID = andJson.changeID.ToString();
            aINode.tempAIStateData.remoteAttackID = andJson.remoteAttackID.ToString();
            aINode.tempAIStateData.attackCount = andJson.attackCount.ToString();
            aINode.tempAIStateData.meleeAttackID = andJson.meleeAttackID.ToString();
            aINode.tempAIStateData.attackKeepTime = andJson.attackKeepTime.ToString();
            aINode.tempAIStateData.fixedPoint = andJson.fixedPoint;
            nodes.Add(aINode);
        }
        Debug.Log(aIDataJson.ToJsonStr());
        if (nodes.Count > 0)
        {
            connections = new List<Connection>();
            for (int i = 0; i < aIDataJson.aIConectionDataJsons.Count; i++)
            {
                AIConectionPointJson aj = aIDataJson.aIConectionDataJsons[i].inCP;
                AINode tempNode = IsConnectionNode(aj.nodeId);
                ConnectionPoint inP = new ConnectionPoint(tempNode, aj.inType, tempNode.inOutDefaultStyle, tempNode.inOutSelectStyle, OnClickInPoint);
                inP.isEnable = aj.isInEnable;
                inP.isConnection = aj.isInConnection;
                tempNode.inPoint = inP;

                aj = aIDataJson.aIConectionDataJsons[i].outCP;
                tempNode = IsConnectionNode(aj.nodeId);
                ConnectionPoint outP = new ConnectionPoint(tempNode, aj.inType, tempNode.inOutDefaultStyle, tempNode.inOutSelectStyle, OnClickOutPoint);
                outP.isEnable = aj.isInEnable;
                outP.isConnection = aj.isInConnection;
                tempNode.outPoints.Add(outP);
                Connection connection = new Connection(inP, outP, OnClickRemoveConnection);
                connections.Add(connection);
            }
        }
        //var list = aIDataJson.GetHeadList();
        //ZLogUtil.Log(list.Count);
    }

    private AINode IsConnectionNode(int id)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i].id == id)
            {
                return nodes[i];
            }
        }

        return null;
    }
}
