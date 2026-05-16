using System.Collections.Generic;

namespace HallWorld
{
    /// <summary>
    /// 任务数据管理器（数据层）
    /// </summary>
    public class TaskDataMgr : IDataBehaviour
    {
        private List<int> mCompletedTaskIds = new List<int>();

        public void OnCreate()
        {
            UnityEngine.Debug.Log("[TaskDataMgr] OnCreate");
        }

        public void OnDestroy()
        {
            mCompletedTaskIds.Clear();
            UnityEngine.Debug.Log("[TaskDataMgr] OnDestroy");
        }

        public void CompleteTask(int taskId)
        {
            if (!mCompletedTaskIds.Contains(taskId))
                mCompletedTaskIds.Add(taskId);
        }

        public bool IsTaskCompleted(int taskId) => mCompletedTaskIds.Contains(taskId);
    }
}
