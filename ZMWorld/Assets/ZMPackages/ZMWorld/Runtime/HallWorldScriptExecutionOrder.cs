using System;

/// <summary>
/// HallWorld 的脚本初始化顺序配置示例。
/// 使用方式：在你的 HallWorld 类中重写 GetBehaviourExecution()：
/// <code>
/// public class HallWorld : World {
///     public override IBehaviourExecution GetBehaviourExecution()
///         => new HallWorldScriptExecutionOrder();
/// }
/// </code>
/// </summary>
public class HallWorldScriptExecutionOrder : IBehaviourExecution
{
    private static readonly string[] LogicBehaviorExecutions = new string[] {
       "HallLogicCtrl",
     };

    private static readonly string[] DataBehaviorExecutions = new string[] {
       "UserDataMgr",
       "TaskDataMgr",
     };

    private static readonly string[] MsgBehaviorExecutions = new string[] {
       "HallMsgMgr",
     };

    public string[] GetDataBehaviourExecution()
    {
        return DataBehaviorExecutions;
    }

    public string[] GetLogicBehaviourExecution()
    {
        return LogicBehaviorExecutions;
    }

    public string[] GetMsgBehaviourExecution()
    {
        return MsgBehaviorExecutions;
    }
}
