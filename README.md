# <div align="center">ZM World Framework</div>

<div align="center">

**基于 World 架构的 Unity 游戏框架**

聚焦 **模块化游戏世界管理**、**逻辑 / 数据 / 消息三层分离**、**Editor 自动化脚本生成** 与 **HybridCLR 热更新接入**。

[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2021.3.38f1+-black.svg)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-8.0+-brightgreen.svg)](https://learn.microsoft.com/dotnet/csharp/)
[![Architecture](https://img.shields.io/badge/Architecture-World%20%2B%20Logic%2FData%2FMsg-orange.svg)](#核心特性)

**[快速开始](#快速开始)** | **[核心特性](#核心特性)** | **[项目结构](#项目结构)** | **[API 概览](#api-概览)**

</div>

---

## ⭐ ZM World Framework 是什么？

ZM World Framework 是一个面向 Unity 项目的轻量级游戏框架。它以 **World（游戏世界）** 为核心组织单位，将模块拆分为 **逻辑层（Logic）**、**数据层（Data）**、**消息层（Msg）**，并通过统一的生命周期管理、自动发现与 Editor 工具降低项目结构失控和重复代码问题。

它适合用于以下场景：

- 登录、大厅、玩法、战斗等模块化业务拆分
- 需要多 World 切换与常驻 World 管理的项目
- 需要规范脚本目录与命名、减少样板代码的团队项目
- 需要结合 HybridCLR 进行热更新扩展的 Unity 项目

---

## 核心特性

### 🌍 World 驱动的模块化架构

每个功能域都可以定义为一个独立 World，例如 `LoginWorld`、`HallWorld`、`SKWorld`、`WZWorld`。`WorldManager` 统一负责创建、更新、销毁和切换。

```csharp
WorldManager.CreateWorld<HallWorld>();
WorldManager.DestroyWorld<SKWorld>();
```

### 🧩 逻辑 / 数据 / 消息三层分离

ZM World Framework 约定每个 World 下的脚本按职责划分为三层：

| 层级 | 接口 | 典型命名 | 职责 |
|------|------|----------|------|
| Logic | `ILogicBehaviour` | `XxxLogicCtrl` | 业务流程与模块控制 |
| Data | `IDataBehaviour` | `XxxDataMgr` | 数据缓存、状态维护、读写封装 |
| Msg | `IMsgBehaviour` | `XxxMsgMgr` | 网络消息与模块间通信处理 |

```csharp
public class TaskLogicCtrl : ILogicBehaviour
{
    public void OnCreate() { }
    public void OnDestroy() { }
}
```

### ⚙️ 按命名空间自动发现并初始化

框架会根据当前 `World` 所在命名空间，自动扫描同命名空间下实现了三层接口的脚本并完成实例化与生命周期调用，减少手动注册成本。

### 📌 可控的脚本初始化顺序

通过实现 `IBehaviourExecution`，可以为某个 World 声明逻辑层、数据层、消息层的初始化优先级。

```csharp
public class HallWorldScriptExecutionOrder : IBehaviourExecution
{
    public string[] GetLogicBehaviourExecution() => new[] { "TaskLogicCtrl" };
    public string[] GetDataBehaviourExecution() => new[] { "RankDataMgr", "UserDataMgr" };
    public string[] GetMsgBehaviourExecution() => new[] { "TaskMsgMgr" };
}
```

### 🛠️ Editor 自动化脚本生成

内置 Unity Editor 工具，可按约定快速生成：

- `XxxLogicCtrl.cs`
- `XxxDataMgr.cs`
- `XxxMsgMgr.cs`

并通过 `GeneratorModuleConfig` 管理模块名、命名空间与保存路径。

### 🔥 HybridCLR 热更新接入

框架提供 `CreateWorldByReflection`，可在加载热更新程序集后通过反射创建目标 World。

```csharp
Assembly hotUpdateAssembly = /* 已加载的热更程序集 */;
WorldManager.CreateWorldByReflection(hotUpdateAssembly, "YourGame.SK.SKWorld");
```

---

## 架构概览

```text
┌─────────────────────────────────────────────────────────────┐
│                        WorldManager                         │
│      负责所有 World 的创建、更新、销毁与默认世界管理         │
├────────────────┬────────────────┬───────────────────────────┤
│   LoginWorld   │   HallWorld    │   Battle / SK / WZ ...   │
│ ┌────────────┐ │ ┌────────────┐ │ ┌───────────────────────┐ │
│ │ Logic 层    │ │ │ Logic 层    │ │ │ Logic 层              │ │
│ │ Data  层    │ │ │ Data  层    │ │ │ Data  层              │ │
│ │ Msg   层    │ │ │ Msg   层    │ │ │ Msg   层              │ │
│ └────────────┘ │ └────────────┘ │ └───────────────────────┘ │
└────────────────┴────────────────┴───────────────────────────┘
```

`WorldUpdater` 会作为常驻对象驱动 `WorldManager.Update()`，统一处理所有已构建 World 的帧更新。

---

## 快速开始

### 环境要求

| 组件 | 版本 |
|------|------|
| Unity | 2021.3.38f1+ |
| .NET Profile | Unity 默认运行时环境即可 |
| 平台 | Windows / macOS / Linux（Unity 编辑器支持范围内） |

### 运行示例工程

本仓库中的 Unity 工程位于：

```text
D:\GitHub Project\ZMGC\ZMGC
```

使用 Unity Hub 打开该目录即可。

### 集成到现有项目

你可以任选一种方式接入：

1. 直接复制 `Assets\ZMPackages\ZMGC` 到你的 Unity 项目（当前目录名仍保留为 `ZMGC` 以兼容现有工程）。
2. 按项目需要同步 `Packages\manifest.json` 中相关依赖，例如：

```json
{
  "dependencies": {
    "com.zm.import": "https://github.com/ZMteacher/ZM_Import.git"
  }
}
```

### 创建一个 World

```csharp
namespace YourGame.Hall
{
    public class HallWorld : World
    {
        public override void OnCreate()
        {
            UnityEngine.Debug.Log("HallWorld 已创建");
        }

        public override void OnUpdate()
        {
        }

        public override void OnDestroy()
        {
        }
    }
}
```

在入口处创建：

```csharp
WorldManager.CreateWorld<HallWorld>();
```

### 实现三层脚本

```csharp
namespace YourGame.Hall
{
    public class UserDataMgr : IDataBehaviour
    {
        public string UserId { get; private set; }

        public void OnCreate()
        {
            UserId = "10001";
        }

        public void OnDestroy()
        {
        }
    }

    public class TaskLogicCtrl : ILogicBehaviour
    {
        public void OnCreate()
        {
            var userData = World.GetDataLayer<UserDataMgr>();
            UnityEngine.Debug.Log(userData.UserId);
        }

        public void OnDestroy()
        {
        }
    }

    public class TaskMsgMgr : IMsgBehaviour
    {
        public void OnCreate()
        {
        }

        public void OnDestroy()
        {
        }
    }
}
```

### 获取 World 内对象

```csharp
var logic = World.GetLogicLayer<TaskLogicCtrl>();
var data = World.GetDataLayer<UserDataMgr>();
var msg = World.GetMsgLayer<TaskMsgMgr>();
```

---

## Editor 自动化工具

### 菜单与快捷键

| 功能 | 菜单路径 | 快捷键 |
|------|----------|--------|
| 生成逻辑层脚本 | `GameObject/生成业务逻辑层脚本` | `Shift + L` |
| 生成数据层脚本 | `GameObject/生成数据层脚本` | `Shift + D` |
| 生成消息层脚本 | `GameObject/生成网络层脚本` | `Shift + N` |
| 打开生成配置面板 | `ZM World Framework/Generator Tools` | - |

### 配置方式

`GeneratorModuleConfig` 用于管理脚本生成的目标模块：

- `savePath`：脚本保存根目录
- `modules[].moduleName`：模块名称
- `modules[].moduleNamespace`：模块命名空间

默认配置资源路径：

```text
Assets/ZMPackages/ZMGC/Editor/GeneratorEditor/GeneratorModuleConfig.asset
```

### 典型流程

1. 在配置面板中维护模块列表和命名空间。
2. 输入模块名，生成 Logic / Data / Msg 模板脚本。
3. 框架根据 World 命名空间自动发现并初始化这些脚本。

---

## HybridCLR 热更新支持

当 World 定义在热更新程序集时，可以通过反射方式创建：

```csharp
Assembly hotUpdateAssembly = /* 加载后的程序集 */;
WorldManager.CreateWorldByReflection(hotUpdateAssembly, "YourGame.SK.SKWorld");
```

适用于：

- 动态加载玩法 World
- 热更模块独立程序集拆分
- 主工程与热更逻辑解耦

---

## 项目结构

```text
D:\GitHub Project\ZMGC
├── README.md
├── LICENSE
└── ZMGC
    ├── Assets
    │   ├── Scenes
    │   └── ZMPackages
    │       └── ZMGC
    │           ├── Base
    │           │   ├── IBehaviourExecution.cs
    │           │   ├── IDataBehaviour.cs
    │           │   ├── ILogicBehaviour.cs
    │           │   └── IMsgBehaviour.cs
    │           ├── Editor
    │           │   └── GeneratorEditor
    │           ├── Runtime
    │           │   ├── HallWorldScriptExecutionOrder.cs
    │           │   ├── TypeOrder.cs
    │           │   └── WorldTypeManager.cs
    │           └── World
    │               ├── World.cs
    │               ├── WorldAssembly.cs
    │               ├── WorldManager.cs
    │               └── WorldUpdater.cs
    ├── Packages
    └── ProjectSettings
```

---

## API 概览

### WorldManager

| API | 说明 |
|-----|------|
| `CreateWorld<T>()` | 创建指定类型的 World |
| `CreateWorldByReflection(assembly, fullName)` | 从指定程序集反射创建 World |
| `DestroyWorld<T>(args)` | 销毁指定 World |
| `OnRlease()` | 清空全部 World 并重置运行状态 |
| `DefaultGameWorld` | 首个创建的默认 World |
| `CurWorldEnum` | 当前世界枚举状态 |

### World

| API | 说明 |
|-----|------|
| `GetLogicLayer<T>()` | 获取逻辑层对象 |
| `GetDataLayer<T>()` | 获取数据层对象 |
| `GetMsgLayer<T>()` | 获取消息层对象 |
| `ClearWorldBehaviours()` | 清理三层缓存字典 |

---

## 适用场景

- 登录 / 大厅 / 战斗 / 玩法模块拆分
- 中小型项目的业务结构规范化
- 希望降低脚本依赖耦合的 Unity 项目
- 需要保留热更新扩展能力的客户端框架

---

## License

本项目对外名称为 **ZM World Framework**，当前工程中的部分目录与示例兼容命名仍保留 `ZMGC`。本项目使用 **Apache License 2.0**，详见 [LICENSE](LICENSE)。
