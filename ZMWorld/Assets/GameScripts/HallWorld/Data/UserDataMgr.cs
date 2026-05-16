namespace HallWorld
{
    /// <summary>
    /// 用户数据管理器（数据层）
    /// 只负责存数据，不写业务逻辑
    /// </summary>
    public class UserDataMgr : IDataBehaviour
    {
        public string UserName { get; private set; }
        public int Level { get; private set; }
        public int Gold { get; private set; }

        public void OnCreate()
        {
            // 初始化默认数据
            UserName = "Player";
            Level = 1;
            Gold = 100;
            UnityEngine.Debug.Log("[UserDataMgr] OnCreate");
        }

        public void OnDestroy()
        {
            UnityEngine.Debug.Log("[UserDataMgr] OnDestroy");
        }

        public void SetUserName(string name) => UserName = name;
        public void AddGold(int amount) => Gold += amount;
        public void SetLevel(int level) => Level = level;
    }
}
