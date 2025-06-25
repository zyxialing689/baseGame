using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ScriptCreater
{
    

    public static string  CreatePanelClassName(string path,string name, UINodePanel uIPanel)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string adressPath = path.Replace(".prefab", "").Replace("Assets/Game/AssetDynamic/Prefab/UI/", "");
        string panelName = adressPath.Split('/')[0];
        string dir = path.Replace(".prefab", "").Replace("Assets/Game/AssetDynamic/Prefab/UI","Assets/Game/Scripts/UI")+"/";
        string voDir = dir.Replace("Assets/Game/Scripts/UI", "Assets/Game/Scripts/UIVO");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (!Directory.Exists(voDir))
        {
            Directory.CreateDirectory(voDir);
        }
        voDir = voDir + name + "VO.cs";
        dir = dir + name + ".cs";
        stringBuilder.Append("using System;\n");
        stringBuilder.Append("using System.Collections;\n");
        stringBuilder.Append("using System.Collections.Generic;\n");
        stringBuilder.Append("using UnityEngine;\n");
        stringBuilder.Append("using UnityEngine.UI;\n");
        stringBuilder.Append(string.Format("public partial class {0} : BasePanel\n", name));
        stringBuilder.Append("{\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    public override void Init(params object[] args)\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("        base.Init(args);\n");
        stringBuilder.Append($"        panelLayer = PanelLayer.{panelName};\n");
        stringBuilder.Append($"        adressPath = \"{adressPath}\";\n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("    public override void OnShowing()\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    public override void OnOpen()\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("        RefreshPanel();\n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    public override void OnHide()\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("     \n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    public override void OnClosing()\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("     \n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    private void RefreshPanel()\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("}");


        if (!File.Exists(dir))
        {
            File.WriteAllText(dir, stringBuilder.ToString());
            ZLogUtil.Log("创建"+name+"成功");
        }

        //VO
        stringBuilder.Clear();
        stringBuilder.Append(string.Format("public partial class {0} : BasePanel\n", name));
        stringBuilder.Append("{\n");
        for (int i = 0; i < uIPanel.nodes.Count; i++)
        {
            stringBuilder.Append("   private "+ uIPanel.nodes[i].type+" "+uIPanel.nodes[i].tag+";\n");
        }
        stringBuilder.Append("\n");
        stringBuilder.Append("   public override void AutoInit()\n");
        stringBuilder.Append("   {\n");
        stringBuilder.Append("        ServiceBinder.Instance.RegisterObj(this);\n");
        for (int i = 0; i < uIPanel.nodes.Count; i++)
        {
            Transform curTrans = uIPanel.nodes[i].transform;
            string componentPath = "/" + curTrans.name;
            int count = 0;
            while (curTrans.parent.name != name)
            {
                componentPath = componentPath.Insert(0,("/"+ curTrans.parent.name));
                count++;
                curTrans = curTrans.parent;
                if (count > 100) {
                    ZLogUtil.LogError("层级超过100,有毛病吧");
                    return null;
                }
            }
            componentPath = componentPath.Remove(0,1);
            string componentStr = uIPanel.nodes[i].type;
            if(componentStr == "UnityEngine.GameObject")
            {
                stringBuilder.Append($"    this.{uIPanel.nodes[i].tag} = panel.transform.Find(\"{componentPath}\").gameObject;\n");
            }
            else
            {
                stringBuilder.Append($"    this.{uIPanel.nodes[i].tag} = panel.transform.Find(\"{componentPath}\").GetComponent<{uIPanel.nodes[i].type}>();\n");
            }
          
        }
        stringBuilder.Append("   }\n");
        stringBuilder.Append("}\n");

 


        File.WriteAllText(voDir, stringBuilder.ToString());
        stringBuilder.Clear();
        AssetDatabase.Refresh();
        return dir;
    }


    public static void CreateScrollerClassName(string path,string panelName,string scrollName,UINodeScoller uINodeScoller)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string cellName = uINodeScoller.cellView.name;
        if (uINodeScoller.cellView.GetComponent(uINodeScoller.cellView.name) == null)
        {
            ZLogUtil.LogError($"请先完成预制体{uINodeScoller.cellView.name}");
            return;
        }
        string dir = path.Replace(".prefab", "").Replace("Assets/Game/AssetDynamic/Prefab/UI", "Assets/Game/Scripts/UI")+"/";
        string voDir = dir.Replace("Assets/Game/Scripts/UI", "Assets/Game/Scripts/UIVO");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (!Directory.Exists(voDir))
        {
            Directory.CreateDirectory(voDir);
        }
        string scrollerDir = dir + "/Scoller/";
        string voscrollerDir = voDir + "/ScollerVO/";
        if (!Directory.Exists(scrollerDir))
        {
            Directory.CreateDirectory(scrollerDir);
        }
        if (!Directory.Exists(voscrollerDir))
        {
            Directory.CreateDirectory(voscrollerDir);
        }
        string scrollerScript = scrollerDir + scrollName + ".cs";
        string voScrollerScript = voscrollerDir + scrollName + "VO.cs";

        stringBuilder.Append("using EnhancedUI.EnhancedScroller;\n");
        stringBuilder.Append("using UnityEngine;\n");
        stringBuilder.Append("using System;\n\n");
        stringBuilder.Append("[RequireComponent(typeof(EnhancedScroller))]\n");
        stringBuilder.Append($"public partial class {scrollName} : MonoBehaviour, IEnhancedScrollerDelegate\n");
        stringBuilder.Append("{\n");
        stringBuilder.Append("    public void Init()\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("     \n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append($"       EnhancedScrollerCellView cellView = scroller.GetCellView(cellViewPrefab);\n");
        stringBuilder.Append("        cellView.dataIndex = dataIndex;\n");
        stringBuilder.Append("        cellView.cellIndex = cellIndex;\n");
        stringBuilder.Append("        cellView.scroller = scroller;");
        stringBuilder.Append("        cellView.InitData(this);");
        stringBuilder.Append("        cellView.RefreshCellView();\n");
        stringBuilder.Append("        return cellView;\n");
        stringBuilder.Append("    }\n\n");
        stringBuilder.Append("    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("        return cellRectTransform.rect.height;\n");
        stringBuilder.Append("    }\n\n");
        stringBuilder.Append("    public int GetNumberOfCells(EnhancedScroller scroller)\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("        return 6;\n");
        stringBuilder.Append("    }\n\n");
        stringBuilder.Append("}\n");

        if (!File.Exists(scrollerScript))
        {
            File.WriteAllText(scrollerScript, stringBuilder.ToString());
            stringBuilder.Clear();
            ZLogUtil.Log("创建" + scrollerScript + "成功");
        }

        ///VO
        stringBuilder.Clear();
        stringBuilder.Append("using EnhancedUI.EnhancedScroller;\n");
        stringBuilder.Append("using UnityEngine;\n");
        stringBuilder.Append("using System;\n\n");
        stringBuilder.Append("[RequireComponent(typeof(EnhancedScroller))]\n");
        stringBuilder.Append($"public partial class {scrollName} : MonoBehaviour, IEnhancedScrollerDelegate\n");
        stringBuilder.Append("{\n");
        stringBuilder.Append("    [NonSerialized]\n");
        stringBuilder.Append("    public EnhancedScroller scroller;\n");
        stringBuilder.Append("    [NonSerialized]\n");
        stringBuilder.Append("    public RectTransform cellRectTransform;\n");
        stringBuilder.Append("    [NonSerialized]\n");
        stringBuilder.Append("    public RectTransform rectTransform;\n");
        stringBuilder.Append("    public EnhancedScrollerCellView cellViewPrefab;\n\n");
        for (int i = 0; i < uINodeScoller.nodes.Count; i++)
        {
            stringBuilder.Append("    private " + uINodeScoller.nodes[i].type + " " + uINodeScoller.nodes[i].tag + ";\n");
        }
        stringBuilder.Append("    void Start()\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("        ServiceBinder.Instance.RegisterObj(this);\n");
        stringBuilder.Append("        scroller = GetComponent<EnhancedScroller>();\n");
        stringBuilder.Append("        rectTransform = GetComponent<RectTransform>();\n");
        stringBuilder.Append("        cellRectTransform = cellViewPrefab.GetComponent<RectTransform>();\n");
        stringBuilder.Append("        scroller.Delegate = this;\n");
        for (int i = 0; i < uINodeScoller.nodes.Count; i++)
        {
            Transform curTrans = uINodeScoller.nodes[i].transform;
            string componentPath = "/" + curTrans.name;
            int count = 0;
            while (curTrans.parent.name != scrollName)
            {
                componentPath = componentPath.Insert(0, ("/" + curTrans.parent.name));
                count++;
                curTrans = curTrans.parent;
                if (count > 100)
                {
                    ZLogUtil.LogError("层级超过100,有毛病吧");
                    return;
                }
            }
            componentPath = componentPath.Remove(0, 1);
            string componentStr = uINodeScoller.nodes[i].type;
            if (componentStr == "UnityEngine.GameObject")
            {
                stringBuilder.Append($"        this.{uINodeScoller.nodes[i].tag} = transform.Find(\"{componentPath}\").gameObject;\n");
            }
            else
            {
                stringBuilder.Append($"        this.{uINodeScoller.nodes[i].tag} = transform.Find(\"{componentPath}\").GetComponent<{uINodeScoller.nodes[i].type}>();\n");
            }
           
        }
        stringBuilder.Append("        Init();\n");
        stringBuilder.Append("     }\n");
        stringBuilder.Append("}\n");

        File.WriteAllText(voScrollerScript, stringBuilder.ToString());
        stringBuilder.Clear();
        AssetDatabase.Refresh();
    }

    public static void CreateScrollerCellClassName(string path, string panelName, string cellName, UINodeScollerCell uINodeScoller)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string dir = path.Replace(".prefab", "").Replace("Assets/Game/AssetDynamic/Prefab/UI", "Assets/Game/Scripts/UI")+"/";
        string voDir = dir.Replace("Assets/Game/Scripts/UI", "Assets/Game/Scripts/UIVO");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
 


        string scrollerDir = dir + "/Scoller/";
        string voscrollerDir = voDir + "/ScollerVO/";
        if (!Directory.Exists(scrollerDir))
        {
            Directory.CreateDirectory(scrollerDir);
        }
        string scrollerCell = scrollerDir + "Cell/";
        string voscrollerCellDir = voscrollerDir + "CellVO/";
        if (!Directory.Exists(scrollerCell))
        {
            Directory.CreateDirectory(scrollerCell);
        }
        if (!Directory.Exists(voscrollerCellDir))
        {
            Directory.CreateDirectory(voscrollerCellDir);
        }
        string scrollerCellScript = scrollerCell + cellName + ".cs";
        string scrollerCellVOScript = voscrollerCellDir + cellName + "VO.cs";
        stringBuilder.Append("using EnhancedUI.EnhancedScroller;\n");
        stringBuilder.Append("using UnityEngine.UI;\n\n");
        stringBuilder.Append($"public partial class {cellName} : EnhancedScrollerCellView\n");
        stringBuilder.Append("{\n");
        stringBuilder.Append("    public override void RefreshCellView()\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("        AutoInit();\n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("}");


        if (!File.Exists(scrollerCellScript))
        {
            File.WriteAllText(scrollerCellScript, stringBuilder.ToString());
            stringBuilder.Clear();
            ZLogUtil.Log("创建" + scrollerCellScript + "成功");
        }

        //VO

        stringBuilder.Clear();
        stringBuilder.Append("using EnhancedUI.EnhancedScroller;\n\n");
        stringBuilder.Append(string.Format("public partial class {0} : EnhancedScrollerCellView\n", cellName));
        stringBuilder.Append("{\n");
        stringBuilder.Append("   private bool onceInit = true;\n");

        for (int i = 0; i < uINodeScoller.nodes.Count; i++)
        {
            stringBuilder.Append("   private " + uINodeScoller.nodes[i].type + " " + uINodeScoller.nodes[i].tag + ";\n");
        }
        stringBuilder.Append("\n");
        stringBuilder.Append("   public override void AutoInit()\n");
        stringBuilder.Append("   {\n");
        stringBuilder.Append("       ServiceBinder.Instance.RegisterObj(this);\n");
        for (int i = 0; i < uINodeScoller.nodes.Count; i++)
        {
            Transform curTrans = uINodeScoller.nodes[i].transform;
            string componentPath = "/" + curTrans.name;
            int count = 0;
            while (curTrans.parent.name != cellName)
            {
                componentPath = componentPath.Insert(0, ("/" + curTrans.parent.name));
                count++;
                curTrans = curTrans.parent;
                if (count > 100)
                {
                    ZLogUtil.LogError("层级超过100,有毛病吧");
                    return;
                }
            }
            componentPath = componentPath.Remove(0, 1);
            string componentStr = uINodeScoller.nodes[i].type;
            if (componentStr == "UnityEngine.GameObject")
            {
                stringBuilder.Append($"        this.{uINodeScoller.nodes[i].tag} = transform.Find(\"{componentPath}\").gameObject;\n");
            }
            else
            {
                stringBuilder.Append($"        this.{uINodeScoller.nodes[i].tag} = transform.Find(\"{componentPath}\").GetComponent<{uINodeScoller.nodes[i].type}>();\n");
            }
          
        }
        stringBuilder.Append("   }\n");
        stringBuilder.Append("}\n");

        File.WriteAllText(scrollerCellVOScript, stringBuilder.ToString());
        stringBuilder.Clear();
        AssetDatabase.Refresh();

        }

    public static string CreateItemClassName(string path, string name, UINodeItem uIPanel)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string adressPath = path.Replace(".prefab", "").Replace("Assets/Game/AssetDynamic/Prefab/UI/", "");
        string panelName = adressPath.Split('/')[0];
        string dir = path.Replace(".prefab", "").Replace("Assets/Game/AssetDynamic/Prefab/UI", "Assets/Game/Scripts/UI") + "/others/";
        string voDir = dir.Replace("Assets/Game/Scripts/UI", "Assets/Game/Scripts/UIVO");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (!Directory.Exists(voDir))
        {
            Directory.CreateDirectory(voDir);
        }
        voDir = voDir + name + "VO.cs";
        dir = dir + name + ".cs";
        stringBuilder.Append("using System;\n");
        stringBuilder.Append("using System.Collections;\n");
        stringBuilder.Append("using System.Collections.Generic;\n");
        stringBuilder.Append("using UnityEngine;\n");
        stringBuilder.Append("using UnityEngine.UI;\n");
        stringBuilder.Append(string.Format("public partial class {0} : BaseItemMono\n", name));
        stringBuilder.Append("{\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    public override void show(params object[] args)\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("}");


        if (!File.Exists(dir))
        {
            File.WriteAllText(dir, stringBuilder.ToString());
            ZLogUtil.Log("创建" + name + "成功");
        }

        //VO
        stringBuilder.Clear();
        stringBuilder.Append(string.Format("public partial class {0} : BaseItemMono\n", name));
        stringBuilder.Append("{\n");
        for (int i = 0; i < uIPanel.nodes.Count; i++)
        {
            stringBuilder.Append("   private " + uIPanel.nodes[i].type + " " + uIPanel.nodes[i].tag + ";\n");
        }
        stringBuilder.Append("\n");
        stringBuilder.Append("   public override void InitNode()\n");
        stringBuilder.Append("   {\n");
        stringBuilder.Append("        ServiceBinder.Instance.RegisterObj(this);\n");
        for (int i = 0; i < uIPanel.nodes.Count; i++)
        {
            Transform curTrans = uIPanel.nodes[i].transform;
            string componentPath = "/" + curTrans.name;
            int count = 0;
            while (curTrans.parent.name != name)
            {
                componentPath = componentPath.Insert(0, ("/" + curTrans.parent.name));
                count++;
                curTrans = curTrans.parent;
                if (count > 100)
                {
                    ZLogUtil.LogError("层级超过100,有毛病吧");
                    return null;
                }
            }
            componentPath = componentPath.Remove(0, 1);
            string componentStr = uIPanel.nodes[i].type;
            if (componentStr == "UnityEngine.GameObject")
            {
                stringBuilder.Append($"    this.{uIPanel.nodes[i].tag} = transform.Find(\"{componentPath}\").gameObject;\n");
            }
            else
            {
                stringBuilder.Append($"    this.{uIPanel.nodes[i].tag} = transform.Find(\"{componentPath}\").GetComponent<{uIPanel.nodes[i].type}>();\n");
            }

        }
        stringBuilder.Append("   }\n");
        stringBuilder.Append("}\n");




        File.WriteAllText(voDir, stringBuilder.ToString());
        stringBuilder.Clear();
        AssetDatabase.Refresh();
        return dir;
    }
    public static string CreateItem_InitClassName(string path, string name, UINodeItem_Init uIPanel)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string adressPath = path.Replace(".prefab", "").Replace("Assets/Game/AssetDynamic/Prefab/UI/", "");
        string panelName = adressPath.Split('/')[0];
        string dir = path.Replace(".prefab", "").Replace("Assets/Game/AssetDynamic/Prefab/UI", "Assets/Game/Scripts/UI") + "/others/";
        string voDir = dir.Replace("Assets/Game/Scripts/UI", "Assets/Game/Scripts/UIVO");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (!Directory.Exists(voDir))
        {
            Directory.CreateDirectory(voDir);
        }
        voDir = voDir + name + "VO.cs";
        dir = dir + name + ".cs";
        stringBuilder.Append("using System;\n");
        stringBuilder.Append("using System.Collections;\n");
        stringBuilder.Append("using System.Collections.Generic;\n");
        stringBuilder.Append("using UnityEngine;\n");
        stringBuilder.Append("using UnityEngine.UI;\n");
        stringBuilder.Append(string.Format("public partial class {0} : BaseItemMono\n", name));
        stringBuilder.Append("{\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    public override void show(params object[] args)\n");
        stringBuilder.Append("    {\n");
        stringBuilder.Append("    }\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("}");


        if (!File.Exists(dir))
        {
            File.WriteAllText(dir, stringBuilder.ToString());
            ZLogUtil.Log("创建" + name + "成功");
        }

        //VO
        stringBuilder.Clear();
        stringBuilder.Append(string.Format("public partial class {0} : BaseItemMono\n", name));
        stringBuilder.Append("{\n");
        for (int i = 0; i < uIPanel.nodes.Count; i++)
        {
            stringBuilder.Append("   private " + uIPanel.nodes[i].type + " " + uIPanel.nodes[i].tag + ";\n");
        }
        stringBuilder.Append("\n");
        stringBuilder.Append("   public override void customInitNode()\n");
        stringBuilder.Append("   {\n");
        stringBuilder.Append("        ServiceBinder.Instance.RegisterObj(this);\n");
        for (int i = 0; i < uIPanel.nodes.Count; i++)
        {
            Transform curTrans = uIPanel.nodes[i].transform;
            string componentPath = "/" + curTrans.name;
            int count = 0;
            while (curTrans.parent.name != name)
            {
                componentPath = componentPath.Insert(0, ("/" + curTrans.parent.name));
                count++;
                curTrans = curTrans.parent;
                if (count > 100)
                {
                    ZLogUtil.LogError("层级超过100,有毛病吧");
                    return null;
                }
            }
            componentPath = componentPath.Remove(0, 1);
            string componentStr = uIPanel.nodes[i].type;
            if (componentStr == "UnityEngine.GameObject")
            {
                stringBuilder.Append($"    this.{uIPanel.nodes[i].tag} = transform.Find(\"{componentPath}\").gameObject;\n");
            }
            else
            {
                stringBuilder.Append($"    this.{uIPanel.nodes[i].tag} = transform.Find(\"{componentPath}\").GetComponent<{uIPanel.nodes[i].type}>();\n");
            }

        }
        stringBuilder.Append("   }\n");
        stringBuilder.Append("}\n");




        File.WriteAllText(voDir, stringBuilder.ToString());
        stringBuilder.Clear();
        AssetDatabase.Refresh();
        return dir;
    }


    #region uiscript
        public static void CreateServiceBinder()
        {
                string dir = "Assets/Game/Scripts/Core/";
                string name = "ServiceBinder";

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                StringBuilder stringBuilder = new StringBuilder();

                dir = dir + name + ".cs";
                stringBuilder.Append("public class ServiceBinder : BaseServiceBinder\n");
                stringBuilder.Append("{\n");
                stringBuilder.Append("     protected  ServiceBinder()\n");
                stringBuilder.Append("     {\n");
                stringBuilder.Append("     }\n");
                stringBuilder.Append("     public override void Binder()\n");
                stringBuilder.Append("     {\n");
                stringBuilder.Append("        container.RegisterInstance<ITestMgr>(new TestMgr());\n");
                stringBuilder.Append("     }\n");
                stringBuilder.Append("}\n");
                stringBuilder.Append("//使用方法 在panel 中  [Inject] public ITestMgr testMgr; 注册后直接使用");

                if (!File.Exists(dir))
                {
                    File.WriteAllText(dir, stringBuilder.ToString());
                    ZLogUtil.Log("创建"+name+"成功");
                }

                File.WriteAllText(dir, stringBuilder.ToString());
                stringBuilder.Clear();
                AssetDatabase.Refresh();
         }
            public static void CreateTestData()
        {
                string dir = "Assets/Game/Scripts/Model/Test/Data/";
                string name = "TestData";

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                StringBuilder stringBuilder = new StringBuilder();

                dir = dir + name + ".cs";
                stringBuilder.Append("public class TestData\n");
                stringBuilder.Append("{\n");
                stringBuilder.Append("}\n");

                if (!File.Exists(dir))
                {
                    File.WriteAllText(dir, stringBuilder.ToString());
                    ZLogUtil.Log("创建"+name+"成功");
                }

                File.WriteAllText(dir, stringBuilder.ToString());
                stringBuilder.Clear();
                AssetDatabase.Refresh();
         }
         public static void CreateEventTest()
        {
                string dir = "Assets/Game/Scripts/Model/Test/Event/";
                string name = "EventTest";

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                StringBuilder stringBuilder = new StringBuilder();

                dir = dir + name + ".cs";
                stringBuilder.Append("public class Event_Test_SelectTestMonster : Event<Event_Test_SelectTestMonster> {\n");
                stringBuilder.Append("    public override void Clear() { }\n");
                stringBuilder.Append("}\n");

                if (!File.Exists(dir))
                {
                    File.WriteAllText(dir, stringBuilder.ToString());
                    ZLogUtil.Log("创建"+name+"成功");
                }

                File.WriteAllText(dir, stringBuilder.ToString());
                stringBuilder.Clear();
                AssetDatabase.Refresh();
         }
         public static void CreateITestMgr()
        {
                string dir = "Assets/Game/Scripts/Model/Test/Mgr/";
                string name = "ITestMgr";

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                StringBuilder stringBuilder = new StringBuilder();

                dir = dir + name + ".cs";
                stringBuilder.Append("public interface ITestMgr\n");
                stringBuilder.Append("{\n");
                stringBuilder.Append("    public string GetTestStr();\n");
                stringBuilder.Append("}\n");

                if (!File.Exists(dir))
                {
                    File.WriteAllText(dir, stringBuilder.ToString());
                    ZLogUtil.Log("创建"+name+"成功");
                }

                File.WriteAllText(dir, stringBuilder.ToString());
                stringBuilder.Clear();
                AssetDatabase.Refresh();
         }
        public static void CreateTestMgr()
        {
                string dir = "Assets/Game/Scripts/Model/Test/Mgr/";
                string name = "TestMgr";

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                StringBuilder stringBuilder = new StringBuilder();

                dir = dir + name + ".cs";
                stringBuilder.Append("public class TestMgr : ITestMgr\n");
                stringBuilder.Append("{\n");
                stringBuilder.Append("   private string testStr = \"test text\";\n");
                stringBuilder.Append("    public string GetTestStr()\n");
                stringBuilder.Append("    {\n");
                stringBuilder.Append("        return testStr;\n");
                stringBuilder.Append("    }\n");
                stringBuilder.Append("}\n");

                if (!File.Exists(dir))
                {
                    File.WriteAllText(dir, stringBuilder.ToString());
                    ZLogUtil.Log("创建"+name+"成功");
                }

                File.WriteAllText(dir, stringBuilder.ToString());
                stringBuilder.Clear();
                AssetDatabase.Refresh();
         }
    #endregion
}
