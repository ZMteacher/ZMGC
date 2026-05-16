namespace HallWorld
{
    /// <summary>
    /// 大厅主逻辑控制器（逻辑层）
    /// 调用 Data 层读写数据，通过 Msg 层广播变化，不直接操作 UI
    /// </summary>
    public class HallLogicCtrl : ILogicBehaviour
    {
        private UserDataMgr mUserData;
        private TaskDataMgr mTaskData;
        private HallMsgMgr mHallMsg;

        public void OnCreate()
        {
            // Data/Msg 在 Logic 之前已初始化完毕，可以直接通过静态方法取
            mUserData = World.GetDataLayer<UserDataMgr>();
            mTaskData = World.GetDataLayer<TaskDataMgr>();
            mHallMsg = World.GetMsgLayer<HallMsgMgr>();

            UnityEngine.Debug.Log($"[HallLogicCtrl] OnCreate — 欢迎, {mUserData.UserName}!");
        }

        public void OnDestroy()
        {
            UnityEngine.Debug.Log("[HallLogicCtrl] OnDestroy");
        }

        /// <summary>
        /// 示例：玩家完成任务
        /// </summary>
        public void CompleteTask(int taskId)
        {
            if (mTaskData.IsTaskCompleted(taskId))
            {
                UnityEngine.Debug.LogWarning($"任务 {taskId} 已完成，勿重复提交");
                return;
            }

            mTaskData.CompleteTask(taskId);
            mUserData.AddGold(50); // 奖励金币

            // 通过消息层发送完成任务消息，不直接引用 UI
            mHallMsg.SendCompleteTaskRequest(taskId);


            UnityEngine.Debug.Log($"[HallLogicCtrl] 任务 {taskId} 完成！当前金币: {mUserData.Gold}");
        }
        /// <summary>
        /// 任务完成处理逻辑
        /// </summary>
        /// <param name="taskId"></param>
        public void OnTaskCompleteHandle(int taskId)
        {
            //1.刷新数据
            
            //2.通过事件派发到UI层进行更新
        }

        /// <summary>
        /// 示例：修改用户名
        /// </summary>
        public void RenameUser(string newName)
        {
            mUserData.SetUserName(newName);
            UnityEngine.Debug.Log($"[HallLogicCtrl] 用户名已改为: {newName}");
        }
    }
}
