# AGENTS.md — 餐车杀手 (Food Truck Killer)

> 本文件是 AI agent 协作开发指南，记录项目概况、团队阵容、技术架构、协作规范与各阶段成果。
> 任何 agent 接手本项目前应先阅读此文件。

---

## 一、项目概况

| 项 | 内容 |
|:---|:---|
| **游戏名称** | 餐车杀手 (Food Truck Killer) |
| **类型** | 2D 俯视角 · 动作 + 解谜 + 经营 |
| **核心概念** | "烹饪是伪装，杀戮是主业"——玩家扮演杀手帮派的餐车厨师 |
| **核心循环** | 白天：经营伪装 → 诱杀目标 → 处理尸体 → 躲避警察；夜晚：帮派地盘战（M4+） |
| **美术风格** | 高精度像素风 (Hi-Bit Pixel Art)，黑色幽默 + 轻度赛博朋克 |
| **参考作品** | Hotline Miami、GTA 2 |
| **引擎** | Unity 6 LTS / 团结引擎 2.x（URP 2D + Pixel Perfect Camera, PPU=32） |
| **目标平台** | PC（MVP），后续扩展 Console / Mobile |
| **项目阶段** | M1 经营循环 ✅ | M2 暗杀循环 ✅ | M3 尸体检测/警察AI 待推进 |

---

## 二、团队阵容（6 角色 Agent）

| 代号 | 角色 | Agent 名 | 职责定位 | 主要工作目录 |
|:---:|:---|:---|:---|:---|
| **A1** | 主程/架构师 | `architect` | Core 框架、GameManager、Player、技术决策、Editor 工具 | `Scripts/Core/` `Scripts/GameManager/` `Scripts/Player/` `Editor/` |
| **A2** | 玩法程序 | `gameplay-dev` | 烹饪、顾客AI、暗杀、尸体、检测等全部玩法逻辑 | `Scripts/Cooking/` `Scripts/Customer/` `Scripts/Assassination/` `Scripts/Corpse/` `Scripts/Detection/` `Scripts/Economy/` |
| **A3** | 关卡/数值设计 | `level-designer` | JSON 数据配置、关卡布局、数值平衡 | `Data/` `Design/` |
| **A4** | 像素美术 | `pixel-artist` | 角色/Tileset/Props/UI/FX 像素资源 | `Art/Sprites/` `Design/generate_art.py` |
| **A5** | 音效/手感反馈 | `audio-eng` | SFX/BGM、屏幕震动、命中停顿、游戏手感 | `Art/Audio/` `Scripts/Audio/` `Design/generate_audio.py` |
| **A6** | QA/测试 | `qa-tester` | EditMode/PlayMode 测试、用例文档、Bug 追踪 | `Tests/` `Design/TestCases.md` |
| **Lead** | 团队协调 | 主 agent | 任务分配、集成修复、场景引导、Web Demo | 全局 |

### 协作方式

- **并行开发**：各角色分管不同子目录，文件级无冲突
- **接口先行**：A1 定义 Core 接口（GameEvents/StateMachine/Singleton/IInteractable），其他角色基于约定并行开发
- **事件解耦**：跨系统通信必须通过 `GameEvents` 静态类（`Raise()`/`Register()`），禁止跨系统 `GetComponent`
- **数据驱动**：数值配置在 `Data/*.json`，运行时由 `JsonDataLoader` 加载，策划改 JSON 不动代码
- **沙箱兼容**：无 Unity 编辑器时，`SceneBootstrapper` 运行时创建全部 GameObject；`Resources/` 目录供运行时加载

---

## 三、技术架构

### 核心设计模式

| 模式 | 实现 | 用途 |
|:---|:---|:---|
| ScriptableObject 事件系统 | `GameEvent` + `GameEvents` 静态聚合类（16 事件） | 跨系统松耦合通信 |
| 有限状态机 (FSM) | `StateMachine<T>` + `IState` | 顾客AI、警察AI、玩家状态 |
| 泛型单例 | `SingletonMono<T>` + `OnSingletonAwake()` | GameManager/EconomyManager/AudioManager 等 |
| 数据驱动 | `JsonDataLoader` + POCO 中转 + `CreateInstance<SO>` | 运行时从 JSON 加载到 ScriptableObject |
| 对象池 | AudioManager 的 AudioSource 池 | 避免频繁创建 |

### GameEvents 事件清单（16 个）

| 事件 | 触发时机 | 触发者 → 监听者 |
|:---|:---|:---|
| OnOrderServed | 出餐成功 | CookingController → EconomyManager/CoverSystem/Audio |
| OnCustomerDied | 顾客死亡 | KillExecutor → CoverSystem/Audio |
| OnTargetKilled | 暗杀目标击杀 | KillExecutor → AssassinationManager/ObjectiveTracker |
| OnWanted | 通缉等级变化 | AlertSystem → WantedSystem/Audio |
| OnCorpseFound | 尸体被发现 | Detection → WantedSystem/Police |
| OnAlertChanged | 警戒值变化 | AlertSystem → UI/Police |
| OnDayEnd | 白天结束 | DayTimeController → GameManager |
| OnOrderIn/OnServe/OnCash/OnChop/OnSizzle/OnFootstep/OnGrind/OnScream/OnExplosion | 细粒度音效 | 各系统 → AudioManager |

### 项目目录结构

```
FoodTruckKiller/
├── Assets/
│   ├── _Project/
│   │   ├── Scripts/              # 70 个 C# 脚本
│   │   │   ├── Core/             # Events/StateMachine/Singleton/DataLoader
│   │   │   ├── GameManager/      # GameManager/LevelManager/DayTimeController/SceneBootstrapper
│   │   │   ├── Player/           # PlayerController/Motor/Interactor/Carry
│   │   │   ├── Interaction/      # IInteractable 接口
│   │   │   ├── Cooking/          # 烹饪系统
│   │   │   ├── Customer/         # 顾客 AI (FSM)
│   │   │   ├── Economy/          # 经济/伪装度
│   │   │   ├── Assassination/    # 暗杀系统
│   │   │   ├── Corpse/           # 尸体处理
│   │   │   ├── Detection/        # 警戒/通缉/警察AI
│   │   │   ├── Inventory/        # 物品栏
│   │   │   ├── Audio/            # AudioManager/ScreenShake/HitStop
│   │   │   └── UI/               # UI 控制器
│   │   ├── Art/                  # 56 PNG + 26 WAV
│   │   ├── Tests/                # EditMode + PlayMode 测试
│   │   ├── Editor/               # SceneSetupWizard 一键搭建
│   │   └── Settings/             # 配置说明文档
│   └── Resources/                # 运行时加载（JSON/音效/sprite）
├── Data/                         # JSON 数据源
├── Design/                       # 设计文档 + 生成脚本
├── WebDemo/                      # 浏览器试玩 Demo
└── README_Unity.md               # 本地运行指南
```

---

## 四、里程碑与成果

### M0 准备期 ✅ 已完成

**目标**：项目骨架 + 占位资源

**产出**：
- 64 个 C# 脚本（Core 框架 + 全部玩法骨架）
- 43 个像素美术 PNG（色块占位）
- 13 个 WAV 音效 + 1 首 BGM
- 6 份 JSON 数据配置
- 7 个测试文件 + 测试用例文档
- 项目配置（.gitignore / manifest.json / ProjectVersion）

**交付**：项目可运行，占位角色能移动，Git 协作就绪

### M1 经营循环 ✅ 已完成并验证

**目标**：出餐 → 顾客付款 → 赚钱，经营循环可玩

**关键修复**：
- 创建 `GameEvents` 静态聚合类（16 事件），统一事件访问
- 创建 `JsonDataLoader`，运行时从 JSON 加载数据到 SO
- 创建 `SceneBootstrapper`，运行时一键初始化全部子系统
- 迁移所有 `[SerializeField] GameEvent` 为 `GameEvents` 静态访问
- 修复顾客视觉表现（SpriteRenderer + sprite 加载 + fallback）
- 修复玩家移动输入（旧版 Input.GetAxisRaw 兜底）
- 修复摄像机配置（orthographicSize / 背景色 / clearFlags）
- 修复 KillExecutor 命名空间冲突、RecipeData 字段隐藏、InventorySystem 未使用字段

**产出**：
- 新建 5 文件（GameEvents/JsonDataLoader/GameConfig/IngredientData/SceneBootstrapper）
- 修改 17 文件（事件迁移 + 联调 + 视觉修复）
- 新增 BusinessLoopTests（5 个 PlayMode 集成测试）
- scene_layout.json 场景布局数据
- 56 个精化 PNG（M0 保留 + M1 新增 13）
- Web Demo（浏览器可玩经营循环）
- README_Unity.md + Editor/SceneSetupWizard.cs（一键搭建场景）

**交付**：Unity 中按 Play 可见经营循环运行——顾客排队、点单、出餐、加钱

### M2 暗杀循环 ✅ 已完成

**目标**：接单 → 诱饵餐吸引目标 → 击杀 → 尸体生成 → 搬运 → 处理

**各角色任务**：
| 角色 | 任务 | 状态 |
|:---|:---|:---:|
| A2 | AssassinationManager 接单、BaitSystem 诱饵、KillExecutor 击杀、Corpse 生成联调 | ✅ |
| A1 | CarryController 搬运、攻击输入（F键近战/E键交互/Q键放下）、暗杀系统接入 SceneBootstrapper | ✅ |
| A3 | 目标配置、击杀方式数值、暗巷动线 | ✅ |
| A4 | 目标角色、尸体视觉（暗红方块）、处理站颜色区分 | ✅ |
| A5 | 击杀音效、屏幕震动、爆炸事件 | ✅ |
| A6 | 暗杀流程测试 | ⬜ |

**M2 交付物**：
- `SceneBootstrapper.cs` 全面升级，新增 M2 子系统创建（AssassinationManager、BaitSystem、CorpseManager、AlertSystem、EvidenceTracker、WantedSystem、环境击杀对象 x2、处理站 x3）
- `PlayerInteractor.cs` 升级：E 键交互、F 键近战击杀、Q 键放下尸体、处理站联动
- `Corpse.cs` 实现 ICarryable 接口，支持 CarryController 拾取/放下
- `DisposalStation.cs` 新增 TryDispose() 方法，支持 CarryController 联动
- `KillExecutor.cs` 增强 SpawnCorpse：生成尸体视觉 + CorpseDetectionTag
- `EnvironmentKill.cs` 增强：运行时动态查找 KillExecutor + 触发爆炸事件

**操作说明**：
| 按键 | 功能 |
|:---|:---|
| WASD/方向键 | 移动 |
| E | 交互（烹饪台/拾取尸体/触发环境击杀/处理站） |
| F | 近战击杀（需在目标附近） |
| Q | 放下当前搬运的尸体 |

### M3 尸体处理 + 检测危机 📋 计划

**目标**：尸体 4 种处理方式 + 警察视野/警戒/通缉 + 卫生检查员

### M4 打包结算 📋 计划

**目标**：关卡结算、胜负判定、构建配置、完整 MVP demo

---

## 五、协作规范

### 代码规范

1. **命名空间**：`FoodTruckKiller.{模块名}`（如 `FoodTruckKiller.Cooking`）
2. **跨系统通信**：必须走 `GameEvents.OnXxx.Raise()` / `.Register()`，禁止跨系统 `GetComponent`
3. **单例**：继承 `SingletonMono<T>`，用 `override OnSingletonAwake()` 初始化
4. **AI**：用 `StateMachine<T>`，状态实现 `IState`（`OnEnter/OnUpdate/OnExit`）
5. **交互物**：实现 `IInteractable` 接口（`OnInteract(PlayerController)` + `GetPromptName()`）
6. **数据**：JSON 放 `Data/`，复制到 `Resources/Data/`，`JsonDataLoader` 运行时加载
7. **音效**：WAV 放 `Art/Audio/`，复制到 `Resources/Audio/`，`AudioManager.PlaySFX(name)` 播放
8. **美术**：PNG 放 `Art/Sprites/`，PPU=32，Point filter，无压缩
9. **测试**：EditMode 用 NUnit `[Test]`，PlayMode 用 `[UnityTest]`
10. **沙箱兼容**：不依赖 Inspector 注入，运行时用 `Resources.Load` 或代码构造

### Git 规范

- `.meta` 文件必须提交（Unity 依赖 GUID）
- 二进制文件（.png/.wav/.psd）用 Git LFS
- 场景修改用 Prefab 隔离，避免多人改同一 .unity 文件
- 分支：`main`(稳定) / `dev`(集成) / `feature/A2-cooking`

### Agent 工作流程

1. **接任务**：`TaskUpdate` 标记 in_progress + 设 owner
2. **先读后写**：修改现有文件前必须先 `Read`
3. **接口对齐**：修改前先读 `GameEvents.cs` / `JsonDataLoader.cs` / `SceneBootstrapper.cs` 确认接口
4. **完成后**：`TaskUpdate` 标记 completed + 汇报产出文件清单

---

## 六、关键文件速查

| 文件 | 作用 |
|:---|:---|
| `Scripts/Core/Events/GameEvents.cs` | 16 个静态事件，跨系统通信入口 |
| `Scripts/Core/DataLoader/JsonDataLoader.cs` | JSON 运行时加载到 SO |
| `Scripts/Core/Singleton/SingletonMono.cs` | 泛型单例基类 |
| `Scripts/GameManager/SceneBootstrapper.cs` | 场景引导，一键初始化全部系统 |
| `Scripts/GameManager/GameManager.cs` | 游戏总控，状态机管理 |
| `Scripts/Player/PlayerController.cs` | 玩家移动+交互（含旧版 Input 兜底） |
| `Scripts/Customer/CustomerSpawner.cs` | 顾客生成（含 sprite fallback） |
| `Scripts/Cooking/CookingController.cs` | 烹饪协调，触发 OnOrderServed |
| `Scripts/Audio/AudioManager.cs` | 音效管理，Resources.LoadAll 预加载 |
| `Editor/SceneSetupWizard.cs` | Unity 菜单一键搭建场景 |
| `Data/scene_layout.json` | 场景布局坐标 |
| `Design/M1_Report.md` | M1 阶段成果汇总 |

---

## 七、本地运行

1. Unity 6 LTS / 团结引擎 2.x，创建 Universal 2D 项目
2. 复制 `Assets/_Project` 和 `Assets/Resources` 到新项目
3. Package Manager 安装 Input System
4. 菜单 `FoodTruckKiller > 搭建白天关卡场景`
5. 按 ▶ Play
6. WASD 移动，E 交互

> 浏览器试玩：打开 `WebDemo/index.html`

---

## 八、已知限制（M3 完善）

- ~~暗杀/尸体/检测系统代码已写但未在 SceneBootstrapper 挂载~~ M2 已挂载
- 玩家/顾客视觉为程序生成方块（后续替换精绘 sprite）
- 食材组装自动填充（后续加手动拾取 UI）
- 单订单串行处理（后续支持多订单并行）
- 无场景文件 .unity（用 SceneSetupWizard 生成）
- 无 .meta 文件（Unity 首次打开自动生成）
- 警察AI巡逻/视野检测代码已有但未与场景布点联动（M3）
- HealthInspector 卫生检查员未接入日程系统（M3）

---
## 九、M2 操作速查

| 按键 | 功能 | 场景位置 |
|:---|:---|:---|
| WASD/方向键 | 8方向移动 | — |
| E | 交互（烹饪台/拾放尸体/环境击杀/处理站） | 前方 0.5 单位 |
| F | 近战厨刀击杀目标 | 前方 0.8 单位内 Target 顾客 |
| Q | 放下当前搬运的尸体 | — |

| 场景对象 | 颜色 | 位置 | 说明 |
|:---|:---|:---|:---|
| 玩家 | 红色方块 | (0,0) | 带 KillExecutor + CarryController |
| 煤气罐 | 橙色方块 | (1.66, 0.28) | E 键引爆，8格噪声 |
| 广告牌 | 灰色方块 | (1.75, 0.38) | E 键推倒，3格噪声 |
| 绞肉机 | 深红方块 | (0.19, 1.0) | 处理尸体产人肉 |
| 冰柜 | 蓝色方块 | (1.03, 0.41) | 冷冻尸体 |
| 垃圾桶 | 暗棕方块 | (0.19, 1.09) | 抛尸30%被发现 |
| 尸体 | 暗红方块 | 击杀位置 | 可搬运/可被发现 |

---

*最后更新：M2 暗杀循环完成*
