public partial class GamePanel : BasePanel
{
   private BtnTreeNode btnTree;
   private BtnHouseNode btnHouse;
   private BtnHouse1Node btnHouse1;
   private DelBtnNode delBtn;
   private DelcBtnNode delcBtn;
   private ExitBtnNode exitBtn;
   private SaveBtnNode saveBtn;

   public override void AutoInit()
   {
        ServiceBinder.Instance.RegisterObj(this);
    this.btnTree = new BtnTreeNode(panel.transform.Find("bottom/btnTree"));
    this.btnHouse = new BtnHouseNode(panel.transform.Find("bottom/btnHouse"));
    this.btnHouse1 = new BtnHouse1Node(panel.transform.Find("bottom/btnHouse1"));
    this.delBtn = new DelBtnNode(panel.transform.Find("bottom/delBtn"));
    this.delcBtn = new DelcBtnNode(panel.transform.Find("bottom/delcBtn"));
    this.exitBtn = new ExitBtnNode(panel.transform.Find("bottom/exitBtn"));
    this.saveBtn = new SaveBtnNode(panel.transform.Find("saveBtn"));
   }

   private class UINode
   {
       public UnityEngine.GameObject zobj;
       public UnityEngine.Transform ztrans;

       public UINode(UnityEngine.Transform root)
       {
           ztrans = root;
           zobj = root.gameObject;
       }

       public void SetActive(bool value)
       {
           zobj.SetActive(value);
       }
   }

   private class BtnTreeNode : UINode
   {
       public UnityEngine.UI.Button zbtn;
       public UnityEngine.UI.Image zimg;
       public UnityEngine.RectTransform zrect;

       public BtnTreeNode(UnityEngine.Transform root) : base(root)
       {
           zbtn = root.GetComponent<UnityEngine.UI.Button>();
           zimg = root.GetComponent<UnityEngine.UI.Image>();
           zrect = root.GetComponent<UnityEngine.RectTransform>();
       }
   }

   private class BtnHouseNode : UINode
   {
       public UnityEngine.UI.Button zbtn;
       public UnityEngine.UI.Image zimg;
       public UnityEngine.RectTransform zrect;

       public BtnHouseNode(UnityEngine.Transform root) : base(root)
       {
           zbtn = root.GetComponent<UnityEngine.UI.Button>();
           zimg = root.GetComponent<UnityEngine.UI.Image>();
           zrect = root.GetComponent<UnityEngine.RectTransform>();
       }
   }

   private class BtnHouse1Node : UINode
   {
       public UnityEngine.UI.Button zbtn;
       public UnityEngine.UI.Image zimg;
       public UnityEngine.RectTransform zrect;

       public BtnHouse1Node(UnityEngine.Transform root) : base(root)
       {
           zbtn = root.GetComponent<UnityEngine.UI.Button>();
           zimg = root.GetComponent<UnityEngine.UI.Image>();
           zrect = root.GetComponent<UnityEngine.RectTransform>();
       }
   }

   private class DelBtnNode : UINode
   {
       public UnityEngine.UI.Button zbtn;
       public UnityEngine.UI.Image zimg;
       public UnityEngine.RectTransform zrect;

       public DelBtnNode(UnityEngine.Transform root) : base(root)
       {
           zbtn = root.GetComponent<UnityEngine.UI.Button>();
           zimg = root.GetComponent<UnityEngine.UI.Image>();
           zrect = root.GetComponent<UnityEngine.RectTransform>();
       }
   }

   private class DelcBtnNode : UINode
   {
       public UnityEngine.UI.Button zbtn;
       public UnityEngine.UI.Image zimg;
       public UnityEngine.RectTransform zrect;

       public DelcBtnNode(UnityEngine.Transform root) : base(root)
       {
           zbtn = root.GetComponent<UnityEngine.UI.Button>();
           zimg = root.GetComponent<UnityEngine.UI.Image>();
           zrect = root.GetComponent<UnityEngine.RectTransform>();
       }
   }

   private class ExitBtnNode : UINode
   {
       public UnityEngine.UI.Button zbtn;
       public UnityEngine.UI.Image zimg;
       public UnityEngine.RectTransform zrect;

       public ExitBtnNode(UnityEngine.Transform root) : base(root)
       {
           zbtn = root.GetComponent<UnityEngine.UI.Button>();
           zimg = root.GetComponent<UnityEngine.UI.Image>();
           zrect = root.GetComponent<UnityEngine.RectTransform>();
       }
   }

   private class SaveBtnNode : UINode
   {
       public UnityEngine.UI.Button zbtn;
       public UnityEngine.UI.Image zimg;
       public UnityEngine.RectTransform zrect;

       public SaveBtnNode(UnityEngine.Transform root) : base(root)
       {
           zbtn = root.GetComponent<UnityEngine.UI.Button>();
           zimg = root.GetComponent<UnityEngine.UI.Image>();
           zrect = root.GetComponent<UnityEngine.RectTransform>();
       }
   }
}
