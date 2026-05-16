using UnityEngine;

/// <summary>
/// 游戏启动入口，挂载到场景中任意 GameObject 上
/// </summary>
public class GameLauncher : MonoBehaviour
{
    private void Start()
    {
        // 1. 构建 HallWorld，框架自动反射创建所有 Data/Msg/Logic 层对象
        WorldManager.CreateWorld<HallWorld.HallWorld>();

        // 2. 通过静态方法访问逻辑层，无需 Instance
        var logic = World.GetLogicLayer<HallWorld.HallLogicCtrl>();
        logic.RenameUser("ZM");
        logic.CompleteTask(1001);
        logic.CompleteTask(1001); // 重复提交，会有警告日志

        // 3. 直接读数据层（UI 刷新等只读场景）
        var userData = World.GetDataLayer<HallWorld.UserDataMgr>();
        Debug.Log($"玩家: {userData.UserName}, 等级: {userData.Level}, 金币: {userData.Gold}");

        // 4. 直接访问消息层发送消息
        var msg = World.GetMsgLayer<HallWorld.HallMsgMgr>();
        msg.SendCompleteTaskRequest(1001);
    }

    private void OnUserInfoRefresh()
    {
        Debug.Log("[UI] 用户信息已刷新，更新面板");
    }

    private void OnTaskCompleted(int taskId)
    {
        Debug.Log($"[UI] 任务 {taskId} 完成动画播放");
    }

    private void OnDestroy()
    {
        // 销毁世界，释放所有层对象
        WorldManager.DestroyWorld<HallWorld.HallWorld>();
    }
}

