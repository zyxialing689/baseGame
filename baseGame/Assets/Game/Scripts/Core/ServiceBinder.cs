public class ServiceBinder : BaseServiceBinder
{
     protected  ServiceBinder()
     {
     }
     public override void Binder()
     {
        container.RegisterInstance<ITestMgr>(new TestMgr());
     }
}
//使用方法 在panel 中  [Inject] public ITestMgr testMgr; 注册后直接使用