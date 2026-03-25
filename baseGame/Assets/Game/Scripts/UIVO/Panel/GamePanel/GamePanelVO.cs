public partial class GamePanel : BasePanel
{

   public override void AutoInit()
   {
        ServiceBinder.Instance.RegisterObj(this);
   }
}
