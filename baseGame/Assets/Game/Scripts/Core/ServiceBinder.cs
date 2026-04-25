public class ServiceBinder : BaseServiceBinder
{
     protected  ServiceBinder()
     {
     }
     public override void Binder()
     {
        container.RegisterInstance<IGPMgr>(new GPMgr());
     }
}
//使用方法 在panel 中  [Inject] public ITestMgr testMgr; 注册后直接使用