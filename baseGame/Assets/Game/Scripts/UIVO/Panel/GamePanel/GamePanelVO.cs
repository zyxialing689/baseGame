public partial class GamePanel : BasePanel
{
   private UnityEngine.UI.Button boxgreen;
   private UnityEngine.UI.Button boxred;

   public override void AutoInit()
   {
        ServiceBinder.Instance.RegisterObj(this);
    this.boxgreen = panel.transform.Find("buttom/hor/boxgreen").GetComponent<UnityEngine.UI.Button>();
    this.boxred = panel.transform.Find("buttom/hor/boxred").GetComponent<UnityEngine.UI.Button>();
   }
}
