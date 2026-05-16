# ZMGC —— Unity 游戏框架

> 基于 **World（游戏世界）** 架构的 Unity 游戏开发框架，采用三层分离设计（逻辑层 / 数据层 / 消息层），支持多世界管理、脚本初始化顺序控制、HybridCLR 热更新，以及 Editor 自动化代码生成工具。

---

## 目录

- [架构概览](#架构概览)
- [核心概念](#核心概念)
  - [World（游戏世界）](#world游戏世界)
  - [三层分离](#三层分离)
  - [WorldManager](#worldmanager)
  - [脚本执行顺序](#脚本执行顺序)
- [快速开始](#快速开始)
  - [环境要求](#环境要求)
  - [安装](#安装)
  - [创建一个游戏世界](#创建一个游戏世界)
  - [实现三层脚本](#实现三层脚本)
  - [配置脚本执行顺序](#配置脚本执行顺序)
- [Editor 代码生成工具](#editor-代码生成工具)
- [HybridCLR 热更新支持](#hybridclr-热更新支持)
- [项目结构](#项目结构)
- [API 参考](#api-参考)

---

## 架构概览

```
┌──────────────────────────────────────────────────────┐
│                    WorldManager                      │
│   管理所有 World 的生命周期、更新与销毁               │
├──────────────┬──────────────┬───────────────────────┤
│  LoginWorld  │  HallWorld   │  SKWorld / WZWorld ... │
│  ┌─────────┐ │ ┌──────────┐ │ ┌─────────────────┐   │
│  │ Logic层  │ │ │ Logic层  │ │ │    Logic 层      │   │
│  │ Data 层  │ │ │ Data 层  │ │ │    Data  层      │   │
│  │ Msg  层  │ │ │ Msg  层  │ │ │    Msg   层      │   │
│  └─────────┘ │ └──────────┘ │ └─────────────────┘   │
└──────────────┴──────────────┴───────────────────────┘
```

- **每个 World 拥有独立的逻辑、数据、消息三层脚本**，通过命名空间隔离。
- **WorldManager** 统一负责世界的构建、帧更新与销毁。
- 脚本由框架通过 **反射自动发现并初始化**，无需手动注册。

---

## 核心概念

### World（游戏世界）

`World` 是框架的基本单元，对应游戏中的一个功能模块（如登录、大厅、战斗等）。每个 World 对应一个独立的 C# 命名空间，框架会自动扫描该命名空间下所有实现了接口的脚本并完成初始化。

```csharp
// 定义一个游戏世界
namespace ZMGC.Hall
{
    public class HallWorld : World
    {
        public override void OnCreate()
        {
            // World 构建完成后触发
        }

        public override void OnUpdate()
        {
            // 每帧更新（尽量少用，防止滥用影响性能）
        }

        public override void OnDestroy()
        {
            // World 销毁时触发
        }

        public override void OnDestroyPostProcess(object args)
        {
            // 销毁完成后的后处理，可在此跳转到其他 World
        }
    }
}
```

### 三层分离

| 层级 | 接口 | 命名规范 | 职责 |
|------|------|----------|------|
| 逻辑层 | `ILogicBehaviour` | `XxxLogicCtrl` | 处理游戏业务逻辑 |
| 数据层 | `IDataBehaviour` | `XxxDataMgr` | 存储、更新和获取游戏数据 |
| 消息层 | `IMsgBehaviour` | `XxxMsgMgr` | 处理网络消息或模块间通信 |

三层接口均只需实现 `OnCreate()` 和 `OnDestroy()` 两个生命周期方法：

```csharp
namespace ZMGC.Hall
{
    // 逻辑层
    public class TaskLogicCtrl : ILogicBehaviour
    {
        public void OnCreate() { /* 初始化逻辑 */ }
        public void OnDestroy() { /* 清理资源 */ }
    }

    // 数据层
    public class UserDataMgr : IDataBehaviour
    {
        public string UserId { get; private set; }
        public void OnCreate() { /* 初始化数据 */ }
        public void OnDestroy() { }
    }

    // 消息层
    public class TaskMsgMgr : IMsgBehaviour
    {
        public void OnCreate() { /* 注册消息 */ }
        public void OnDestroy() { /* 注销消息 */ }
    }
}
```

### WorldManager

`WorldManager` 提供全局静态 API，负责 World 的完整生命周期管理：

```csharp
// 构建世界
WorldManager.CreateWorld<HallWorld>();

// 获取逻辑层控制器
var taskLogic = World.GetLogicLayer<TaskLogicCtrl>();

// 获取数据管理器
var userData = World.GetDataLayer<UserDataMgr>();

// 获取消息管理器
var taskMsg = World.GetMsgLayer<TaskMsgMgr>();

// 销毁指定世界
WorldManager.DestroyWorld<SKWorld>();

// 清理所有世界
WorldManager.OnRlease();
```

框架会自动创建一个常驻场景的 `WorldUpdater` GameObject，用于驱动所有 World 的 `OnUpdate()`，无需手动维护。

### 脚本执行顺序

通过实现 `IBehaviourExecution` 接口，可以精确控制某个 World 下三层脚本的**初始化顺序**。未在列表中的脚本默认排在最后（order = 999）。

```csharp
public class HallWorldScriptExecutionOrder : IBehaviourExecution
{
    // 逻辑层初始化顺序
    public string[] GetLogicBehaviourExecution() => new[] { "TaskLogicCtrl" };

    // 数据层初始化顺序
    public string[] GetDataBehaviourExecution() => new[] { "RankDataMgr", "UserDataMgr" };

    // 消息层初始化顺序
    public string[] GetMsgBehaviourExecution() => new[] { "TaskMsgMgr" };
}
```

在 `WorldManager.GetBehaviourExecution()` 中为对应 World 返回该实例即可生效。

---

## 快速开始

### 环境要求

- Unity **2019.1** 或更高版本
- .NET Standard 2.0 / .NET 4.x

### 安装

在 `Packages/manifest.json` 中添加依赖（通过 Git URL 引入）：

```json
{
  "dependencies": {
    "com.zm.import": "https://github.com/ZMteacher/ZM_Import.git"
  }
}
```

或将 `Assets/ZMPackages/ZMGC` 目录直接复制到项目中。

### 创建一个游戏世界

**Step 1**：继承 `World`，定义新世界类，并置于独立命名空间下。

```csharp
// Assets/HotScripts/BattleWorld/BattleWorld.cs
namespace ZMGC.Battle
{
    public class BattleWorld : World
    {
        public override void OnCreate()
        {
            Debug.Log("BattleWorld 已创建");
        }
    }
}
```

**Step 2**：在游戏入口处调用构建方法。

```csharp
WorldManager.CreateWorld<BattleWorld>();
```

**Step 3**：框架自动发现并初始化 `ZMGC.Battle` 命名空间下所有实现了三层接口的脚本。

### 实现三层脚本

```csharp
namespace ZMGC.Battle
{
    public class BattleLogicCtrl : ILogicBehaviour
    {
        public void OnCreate()
        {
            var data = World.GetDataLayer<BattleDataMgr>();
            Debug.Log($"当前战斗玩家 HP: {data.PlayerHp}");
        }
        public void OnDestroy() { }
    }

    public class BattleDataMgr : IDataBehaviour
    {
        public int PlayerHp { get; private set; } = 100;
        public void OnCreate() { }
        public void OnDestroy() { }
    }
}
```

### 配置脚本执行顺序

```csharp
public class BattleWorldScriptExecutionOrder : IBehaviourExecution
{
    public string[] GetDataBehaviourExecution()  => new[] { "BattleDataMgr" };
    public string[] GetLogicBehaviourExecution() => new[] { "BattleLogicCtrl" };
    public string[] GetMsgBehaviourExecution()   => System.Array.Empty<string>();
}
```

然后在 `WorldManager.GetBehaviourExecution()` 中注册：

```csharp
if (world.GetType().Name == "BattleWorld")
{
    CurWorldEnum = WorldEnum.BattleWorld;
    return new BattleWorldScriptExecutionOrder();
}
```

---

## Editor 代码生成工具

框架内置了 Unity Editor 扩展工具，可一键生成规范的三层脚本模板，避免手写重复代码。

### 使用方式

1. 在 Hierarchy 或 Project 窗口中 **选中一个 GameObject 或资源**。
2. 通过菜单快捷键触发生成：

| 快捷键 | 菜单路径 | 生成内容 |
|--------|----------|----------|
| `Shift + L` | `GameObject / 生成业务逻辑层脚本` | `XxxLogicCtrl.cs` |
| `Shift + D` | `GameObject / 生成数据层脚本` | `XxxDataMgr.cs` |
| `Shift + M` | `GameObject / 生成消息层脚本` | `XxxMsgMgr.cs` |

3. 在弹出的 **MVC 脚本生成检查器** 窗口中，选择目标模块，确认生成路径后点击「生成脚本」。

### 配置

通过 `GeneratorModuleConfig`（ScriptableObject）统一管理模块信息：

- **savePath**：脚本保存的根目录（默认 `HotScripts`）
- **modules**：模块列表，每条记录包含 `moduleName`（模块名）和 `moduleNamespace`（命名空间）

在菜单 `Assets / Create / ScriptableObjects / GeneratorModuleConfig` 创建配置资产。

---

## HybridCLR 热更新支持

框架提供 `WorldManager.CreateWorldByReflection()` 接口，支持通过反射从热更新程序集中构建 World，适配 **HybridCLR**（原 Huatuo）热更新方案：

```csharp
// 加载热更程序集后调用
Assembly hotUpdateAssembly = /* 加载热更程序集 */;
WorldManager.CreateWorldByReflection(hotUpdateAssembly, "ZMGC.SK.SKWorld");
```

---

## 项目结构

```
Assets/
└── ZMPackages/
    └── ZMGC/
        ├── Base/                          # 核心接口定义
        │   ├── IBehaviourExecution.cs     # 脚本执行顺序接口
        │   ├── ILogicBehaviour.cs         # 逻辑层接口
        │   ├── IDataBehaviour.cs          # 数据层接口
        │   └── IMsgBehaviour.cs           # 消息层接口
        ├── World/                         # World 核心实现
        │   ├── World.cs                   # World 主体（字典管理 + 获取接口）
        │   ├── WorldAssembly.cs           # World 注册方法（partial）
        │   ├── WorldManager.cs            # 世界管理器
        │   └── WorldUpdater.cs            # MonoBehaviour 帧驱动器
        ├── Runtime/                       # 运行时工具
        │   ├── WorldTypeManager.cs        # 反射扫描 & 自动初始化
        │   ├── HallWorldScriptExecutionOrder.cs  # HallWorld 执行顺序示例
        │   └── TypeOrder.cs               # 排序辅助类
        └── Editor/                        # 编辑器工具
            └── GeneratorEditor/
                ├── GeneratorLogicCtrl.cs          # 逻辑层脚本生成器
                ├── GeneratorDataMgr.cs            # 数据层脚本生成器
                ├── GeneratorMsgMgr.cs             # 消息层脚本生成器
                ├── GeneratorModuleWindow.cs        # 脚本生成预览窗口
                ├── GeneratorModuleConfig.cs        # 模块配置 ScriptableObject
                └── GeneratorModuleConfigEditorWindow.cs  # 配置管理窗口
```

---

## API 参考

### WorldManager

| 方法 | 说明 |
|------|------|
| `CreateWorld<T>()` | 构建指定类型的游戏世界 |
| `CreateWorldByReflection(assembly, fullName)` | 通过反射构建世界（支持热更新） |
| `DestroyWorld<T>(args)` | 销毁指定世界，并传出参数 |
| `OnRlease()` | 清理所有世界，重置状态 |
| `DefaultGameWorld` | 获取默认（首个构建）的世界 |
| `CurWorldEnum` | 获取当前所处的世界枚举 |

### World（静态方法）

| 方法 | 说明 |
|------|------|
| `GetLogicLayer<T>()` | 获取逻辑层控制器实例 |
| `GetDataLayer<T>()` | 获取数据层管理器实例 |
| `GetMsgLayer<T>()` | 获取消息层管理器实例 |
| `ClearWorldBehaviours()` | 清空所有层的字典 |

---

## License

本项目由 **ZM** 开发，具体授权条款请联系作者。
