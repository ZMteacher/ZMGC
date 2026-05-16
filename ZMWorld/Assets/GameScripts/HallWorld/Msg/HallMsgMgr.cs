using System;

namespace HallWorld
{
    /// <summary>
    /// 大厅消息管理器（消息层）
    /// 负责定义和分发消息事件，解耦逻辑层之间的通信
    /// </summary>
    public class HallMsgMgr : IMsgBehaviour
    {
 
        public void OnCreate()
        {
            UnityEngine.Debug.Log("[HallMsgMgr] OnCreate");
        }

        public void SendCompleteTaskRequest(int taskId)
        {
            UnityEngine.Debug.Log("[HallMsgMgr] SendCompleteTaskRequest");
            OnTaskCompleteResponse();
        }

        private void OnTaskCompleteResponse()
        {
            UnityEngine.Debug.Log("[HallMsgMgr] CompleteTaskResponse");
            HallWorld.GetLogicLayer<HallLogicCtrl>().OnTaskCompleteHandle(1001);
        }

        public void OnDestroy()
        {
            // 清理所有监听，防止内存泄漏
            UnityEngine.Debug.Log("[HallMsgMgr] OnDestroy");
        }
 
    }
}
