using System;
using System.Collections.Generic;
using UnityEngine;

public partial class World  
{
    // 实例级字典，每个 World 独立维护自己的层对象，互不污染
    private Dictionary<Type, ILogicBehaviour> mLogicBehaviourDic = new Dictionary<Type, ILogicBehaviour>();
    private Dictionary<Type, IDataBehaviour> mDataBehaviourDic = new Dictionary<Type, IDataBehaviour>();
    private Dictionary<Type, IMsgBehaviour> mMsgBehaviourDic = new Dictionary<Type, IMsgBehaviour>();
  
    /// <summary>
    /// 世界构建时触发
    /// </summary>
    public virtual void OnCreate() { }

    public virtual void OnUpdate() { }

    /// <summary>
    /// 世界销毁时触发
    /// </summary>
    public virtual void OnDestroy() { }

    /// <summary>
    /// 返回本世界的脚本执行顺序。子类重写此方法来指定初始化优先级，不重写则按反射默认顺序
    /// </summary>
    public virtual IBehaviourExecution GetBehaviourExecution() => null;

    /// <summary>
    /// 销毁游戏世界，释放本世界所有层对象
    /// </summary>
    public void DestroyWorld(object pars = null)
    {
        foreach (var item in mLogicBehaviourDic.Values) item.OnDestroy();
        foreach (var item in mDataBehaviourDic.Values) item.OnDestroy();
        foreach (var item in mMsgBehaviourDic.Values) item.OnDestroy();

        mLogicBehaviourDic.Clear();
        mDataBehaviourDic.Clear();
        mMsgBehaviourDic.Clear();

        OnDestroy();
    }

    /// <summary>
    /// 世界销毁完成后触发，可在此切换到其他世界
    /// </summary>
    public virtual void OnDestroyPostProcess(object args) { }

    // -----------------------------------------------------------------------
    // 静态快捷访问：World.GetLogicLayer<T>() 
    // 内部委托给 WorldManager，自动在所有活跃 World 中查找持有该类型的实例
    // 无需在每个 World 子类定义 Instance 单例
    // -----------------------------------------------------------------------

    /// <summary>
    /// 从任意活跃 World 中获取逻辑层控制器
    /// </summary>
    public static T GetLogicLayer<T>() where T : ILogicBehaviour
        => WorldManager.GetLogicLayer<T>();

    /// <summary>
    /// 从任意活跃 World 中获取数据管理器
    /// </summary>
    public static T GetDataLayer<T>() where T : IDataBehaviour
        => WorldManager.GetDataLayer<T>();

    /// <summary>
    /// 从任意活跃 World 中获取消息层管理器
    /// </summary>
    public static T GetMsgLayer<T>() where T : IMsgBehaviour
        => WorldManager.GetMsgLayer<T>();

    // -----------------------------------------------------------------------
    // 实例方法：在已知 World 实例时（如 WorldManager.GetWorld<T>()）直接访问
    // -----------------------------------------------------------------------

    internal T GetLogicLayerInternal<T>() where T : ILogicBehaviour
    {
        mLogicBehaviourDic.TryGetValue(typeof(T), out var logic);
        return logic != null ? (T)logic : default;
    }

    internal T GetDataLayerInternal<T>() where T : IDataBehaviour
    {
        mDataBehaviourDic.TryGetValue(typeof(T), out var data);
        return data != null ? (T)data : default;
    }

    internal T GetMsgLayerInternal<T>() where T : IMsgBehaviour
    {
        mMsgBehaviourDic.TryGetValue(typeof(T), out var msg);
        return msg != null ? (T)msg : default;
    }
}

