namespace HallWorld
{
    /// <summary>
    /// 大厅世界
    ///
    /// 使用方式：
    ///   WorldManager.CreateWorld&lt;HallWorld&gt;();
    ///
    /// 访问各层（无需 Instance，框架自动路由到正确的 World）：
    ///   World.GetLogicLayer&lt;HallLogicCtrl&gt;()
    ///   World.GetDataLayer&lt;UserDataMgr&gt;()
    ///   World.GetMsgLayer&lt;HallMsgMgr&gt;()
    ///
    /// 多 World 并存时，也可指定 World 查找：
    ///   WorldManager.GetWorld&lt;HallWorld&gt;()
    /// </summary>
    public class HallWorld : World
    {
        public override IBehaviourExecution GetBehaviourExecution() => new HallWorldScriptExecutionOrder();

        public override void OnCreate()
        {
            UnityEngine.Debug.Log("[HallWorld] OnCreate");
        }

        public override void OnDestroy()
        {
            UnityEngine.Debug.Log("[HallWorld] OnDestroy");
        }
    }
}

