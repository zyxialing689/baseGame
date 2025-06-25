public partial class EnterPanel : BasePanel
{
   private UnityEngine.UI.Button enterBtn;

   public override void AutoInit()
   {
        ServiceBinder.Instance.RegisterObj(this);
    this.enterBtn = panel.transform.Find("enterBtn").GetComponent<UnityEngine.UI.Button>();
   }
}
