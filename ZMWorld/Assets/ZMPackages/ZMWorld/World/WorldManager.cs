using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// WorldEnum 属于业务层，不应放在框架核心。
// 请在你的项目中自行定义世界枚举，框架内部改用 Type 追踪当前世界。

/// <summary>
/// 世界管理器
/// </summary>
public class WorldManager
{
    #region 属性
    /// <summary>   
    /// 构建状态
    /// </summary>
    public static bool Builder { get; private set; }
    /// <summary>
    /// 所有已构建出的世界列表
    /// </summary>
    private static List<World> mWorldList = new List<World>();
    /// <summary>
    /// 世界更新程序
    /// </summary>
    public static WorldUpdater WorldUpdater { get; private set; }
    /// <summary>
    /// 默认游戏世界（首个创建的世界）
    /// </summary>
    public static World DefaultGameWorld { get; private set; }
    /// <summary>
    /// 当前游戏世界类型
    /// </summary>
    public static Type CurWorldType { get; private set; }
    
    /// <summary>
    /// 构建世界成功回调，参数为新创建的 World 实例
    /// </summary>
    public static Action<World> OnCreateWorldSuccessListener;
    #endregion
    
    #region 构建游戏世界
    /// <summary>
    /// 构建一个游戏世界
    /// </summary>
    public static void CreateWorld<T>() where T : World, new()
    {
        Type worldType = typeof(T);
        if (CurWorldType == worldType)
        {
            Debug.LogError($"重复构建游戏世界: {worldType.Name}");
            return;
        }
        T world = new T();
        if (DefaultGameWorld == null)
            DefaultGameWorld = world;

        CurWorldType = worldType;

        // Fix #3: 通过虚方法获取执行顺序，不再在 WorldManager 里硬编码世界名
        WorldTypeManager.InitializeWorldAssemblies(world, world.GetBehaviourExecution());
        world.OnCreate();
        mWorldList.Add(world);
        OnCreateWorldSuccessListener?.Invoke(world);

        if (!Builder)
            InitWorldUpdater();
        Builder = true;
    }
    
    /// <summary>
    /// 支持 HybridCLR，通过反射构建对应世界
    /// </summary>
    public static void CreateWorldByReflection(Assembly assembly, string worldFullName)
    {
        Debug.Log($"Start CreateWorldByReflection worldFullName:{worldFullName}");
        
        // Fix #4: 完整的空值防御
        Type worldType = assembly.GetType(worldFullName);
        if (worldType == null)
        {
            Debug.LogError($"CreateWorldByReflection: 找不到类型 '{worldFullName}'，请检查命名空间和程序集名称是否正确");
            return;
        }

        Type worldManagerType = typeof(WorldManager);
        MethodInfo createWorldMethod = worldManagerType.GetMethod("CreateWorld");
        if (createWorldMethod == null)
        {
            Debug.LogError("CreateWorldByReflection: 找不到 WorldManager.CreateWorld 方法");
            return;
        }

        MethodInfo genericMethod = createWorldMethod.MakeGenericMethod(worldType);
        genericMethod.Invoke(null, null);
        Debug.Log($"成功创建 World<{worldType.Name}>");
    }
    #endregion

    #region 游戏世界更新程序
    /// <summary>
    /// 初始化世界更新程序
    /// </summary>
    public static void InitWorldUpdater()
    {
        GameObject worldObj = new GameObject("WorldUpdater");
        WorldUpdater = worldObj.AddComponent<WorldUpdater>();
        GameObject.DontDestroyOnLoad(worldObj);
    }
    /// <summary>
    /// 渲染帧更新。若需要 Update，请在对应 World 的 OnUpdate 中手动调用各层更新接口，
    /// 框架不做自动化是为了防止 Update 滥用影响性能
    /// </summary>
    public static void Update()
    {
        for (int i = 0; i < mWorldList.Count; i++)
            mWorldList[i].OnUpdate();
    }
    /// <summary>
    /// 销毁世界更新程序
    /// </summary>
    private static void DestroyWorldUpdater()
    {
        GameObject.Destroy(WorldUpdater.gameObject);
    }
    #endregion

    #region 销毁释放游戏世界
    /// <summary>
    /// 销毁指定游戏世界
    /// </summary>
    /// <typeparam name="T">要销毁的世界类型</typeparam>
    /// <param name="args">销毁后传出的参数，建议自定义 class 统一传出</param>
    public static void DestroyWorld<T>(object args = null) where T : World
    {
        for (int i = 0; i < mWorldList.Count; i++)
        {
            World world = mWorldList[i];
            // Fix #2: 用 Type 比较，不用字符串
            if (world.GetType() == typeof(T))
            {
                world.DestroyWorld(args);
                mWorldList.RemoveAt(i);
                // 重设当前世界为默认世界
                CurWorldType = DefaultGameWorld?.GetType();
                world.OnDestroyPostProcess(args);
                break;
            }
        }
    }
    /// <summary>
    /// 清理所有世界，重置框架状态
    /// </summary>
    public static void OnRelease()
    {
        foreach (World world in mWorldList)
            world.OnDestroy();

        CurWorldType = null;
        mWorldList.Clear();
        DestroyWorldUpdater();
        Builder = false;
        DefaultGameWorld = null;
        OnCreateWorldSuccessListener = null;
    }
    #endregion

    #region 层对象访问

    /// <summary>
    /// 获取指定 World 实例（多 World 并存时使用）
    /// </summary>
    public static T GetWorld<T>() where T : World
    {
        for (int i = 0; i < mWorldList.Count; i++)
        {
            if (mWorldList[i] is T world) return world;
        }
        UnityEngine.Debug.LogError($"World<{typeof(T).Name}> is not active.");
        return null;
    }

    /// <summary>
    /// 从任意活跃 World 中获取逻辑层控制器（自动匹配持有该类型的 World）
    /// </summary>
    public static T GetLogicLayer<T>() where T : ILogicBehaviour
    {
        for (int i = 0; i < mWorldList.Count; i++)
        {
            var result = mWorldList[i].GetLogicLayerInternal<T>();
            if (result != null) return result;
        }
        UnityEngine.Debug.LogError($"{typeof(T).Name} not found in any active World.");
        return default;
    }

    /// <summary>
    /// 从任意活跃 World 中获取数据管理器（自动匹配持有该类型的 World）
    /// </summary>
    public static T GetDataLayer<T>() where T : IDataBehaviour
    {
        for (int i = 0; i < mWorldList.Count; i++)
        {
            var result = mWorldList[i].GetDataLayerInternal<T>();
            if (result != null) return result;
        }
        UnityEngine.Debug.LogError($"{typeof(T).Name} not found in any active World.");
        return default;
    }

    /// <summary>
    /// 从任意活跃 World 中获取消息层管理器（自动匹配持有该类型的 World）
    /// </summary>
    public static T GetMsgLayer<T>() where T : IMsgBehaviour
    {
        for (int i = 0; i < mWorldList.Count; i++)
        {
            var result = mWorldList[i].GetMsgLayerInternal<T>();
            if (result != null) return result;
        }
        UnityEngine.Debug.LogError($"{typeof(T).Name} not found in any active World.");
        return default;
    }

    #endregion
}
