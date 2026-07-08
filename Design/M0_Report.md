# M0 准备期成果汇报 ——《餐车杀手 Food Truck Killer》

> 6 角色多 Agent 团队并行开发，M0 阶段全部交付完成。
> 引擎：Unity 6 LTS + URP 2D + Pixel Perfect (PPU=32)

---

## 一、团队阵容与各角色"能干的活"

| 代号 | 角色 | Agent 名 | M0 交付 |
|:---:|:---|:---|:---|
| A1 | 主程/架构师 | architect | 项目骨架 + Core 框架 + 玩家系统 |
| A2 | 玩法程序 | gameplay-dev | 7 大玩法系统 34 个脚本 |
| A3 | 关卡/数值设计 | level-designer | JSON 数据配置 + 关卡布局 + 数值平衡 |
| A4 | 像素美术 | pixel-artist | 43 个 PNG 占位资源 |
| A5 | 音效/手感反馈 | audio-eng | 13 个 WAV + 音频/手感脚本 |
| A6 | QA/测试 | qa-tester | 7 个测试骨架 + 15 条用例 + Bug 模板 |

---

## 二、产出统计

| 类别 | 数量 | 位置 |
|:---|---:|:---|
| C# 脚本 | 64 | `Assets/_Project/Scripts/` |
| 像素美术 (PNG) | 43 | `Assets/_Project/Art/Sprites/` |
| 音效 (WAV) | 13 | `Assets/_Project/Art/Audio/` |
| JSON 数据配置 | 6 | `Data/` |
| 设计/配置文档 (MD) | 8 | `Design/` + `Settings/` |
| 测试文件 | 7 | `Assets/_Project/Tests/` |

---

## 三、各角色详细汇报

### A1 主程/架构师 —— 项目地基

**项目配置**
- `.gitignore` / `.gitattributes`（Unity 专用 + Git LFS 追踪二进制）
- `Packages/manifest.json`（Unity 6000.0：2D sprite/tilemap/animation、URP、Input System、Test Framework）
- `ProjectSettings/ProjectVersion.txt`

**Core 框架层**（7 脚本，`Scripts/Core/`）
- `GameEvent.cs` / `GameEventListener.cs` —— ScriptableObject 事件系统，双通道监听（GameEventListener + Action）
- `IState.cs` / `StateMachine.cs` —— 通用 FSM
- `SingletonMono.cs` —— 泛型单例（DontDestroyOnLoad + 重复销毁）
- `ISaveable.cs` / `SaveManager.cs` —— JSON 存档

**GameManager**（5 脚本）：`GameManager`(生命周期) / `LevelManager`(关卡加载) / `DayTimeController`(480s 倒计时) / `GameState` 枚举 / `ObjectiveTracker`(目标进度)

**玩家系统**（5 脚本）：`PlayerController`(8方向输入) / `PlayerMotor`(Rigidbody2D) / `PlayerInteractor`(交互检测) / `PlayerState` / `CarryController`(携带，含 ICarryable 接口)

**交互框架**（2 脚本）：`IInteractable` 接口 / `InteractionPromptUI`

**配置文档**：`PixelPerfectSetup.md` / `InputSystemSetup.md` / `GameEventList.md`（7 个关键事件清单）

### A2 玩法程序 —— 全部玩法逻辑

7 大模块 **34 个脚本**，已核对 A1 的 Core 接口并修正（StateMachine 用非泛型 IState）：

| 模块 | 脚本数 | 关键内容 |
|:---|---:|:---|
| Cooking 烹饪 | 6 | RecipeData(SO) / Order / OrderValidator / CookingStation(IInteractable) / CookingController / CookingState |
| Customer 顾客 | 6 | CustomerProfile(SO) / CustomerAI(FSM) / 7 状态 / Spawner / OrderBubble / QueueManager |
| Economy 经济 | 3 | EconomyManager(单例) / CoverSystem(伪装度) / TransactionEvent |
| Assassination 暗杀 | 6 | TargetProfile(SO) / KillMethodData(SO) / AssassinationManager / BaitSystem / KillExecutor / EnvironmentKill |
| Corpse 尸体 | 5 | DisposalMethod 枚举 / Corpse(IInteractable) / CorpseManager / DisposalStation / CorpseDetectionTag |
| Detection 检测 | 7 | VisionSensor(扇形视野) / AlertSystem / WantedSystem / PoliceAI(FSM) / 5 状态 / HealthInspector / EvidenceTracker |
| Inventory 物品栏 | 1 | InventorySystem |

### A3 关卡/数值设计 —— 数据与关卡

**JSON 数据配置**（6 份，全部通过 json.load 校验）
- `recipes.json`：4 食谱（汉堡15/热狗10/诱饵餐/人肉餐）
- `ingredients.json`：6 食材（标注 isIllegal）
- `customers.json`：3 类型（普通85%/线人10%/目标5%）
- `targets.json`：2 暗杀目标（Tony/Lena）
- `killmethods.json`：4 击杀方式（厨刀/煤气罐/广告牌/投毒）
- `gameconfig.json`：全局配置（480s/100元/视野5×60°/检查员120s）

**设计文档**
- `LevelDesign_DayLevel01.md`：60×40 tile 布局，ASCII 俯视图（40 行均 60 字符），4 分区，22 个关键坐标脚本校验
- `BalanceDesign.md`：经济模型（~50元/min）、通关条件、8 阶段难度曲线
- `Data/README.md`：JSON 结构说明 + ScriptableObject 转换流程

### A4 像素美术 —— 43 个占位资源

`generate_art.py`（Pillow）生成，全部 PNG / Point filter / PPU=32：

| 分类 | 尺寸 | 数量 |
|:---|:---:|---:|
| 玩家厨师（4方向×2帧+idle/cook/attack/carry） | 32×32 | 12 |
| 顾客变体（蓝/绿/黄） | 32×32 | 9 |
| 敌人（警察/检查员） | 32×32 | 2 |
| Tileset（街道/人行道/墙体/暗巷/餐车地面） | 16×16 | 5 |
| Props（烹饪台/绞肉机/冷库/煤气罐/广告牌/垃圾桶） | 32×32 | 6 |
| UI 图标（金钱/伪装度/时间/气泡/交互） | 16×16 | 5 |
| FX（血液/爆炸/烟雾/中毒） | 32×32 | 4 |

调色板：高饱和暖色（红/橙/金/木色）+ 赛博朋克霓虹点缀（品红/青）。

### A5 音效/手感反馈 —— 13 个音效 + 手感脚本

`generate_audio.py`（numpy + wave）合成，22050Hz mono 16bit：

**SFX（12）**：chop / sizzle / serve / cash / footstep / kill / grind / explosion / scream / alarm / order_in / wanted_warning

**BGM（1）**：`bgm_day.wav`（16s 循环，C-G-Am-F 进行，含贝斯/和弦/旋律/鼓组）

**C# 脚本（4）**：`AudioManager`(单例+对象池) / `ScreenShake`(Perlin 抖动) / `HitStop`(timeScale 停顿) / `AudioFeedbackBinder`(事件→音效绑定)

### A6 QA/测试 —— 测试骨架与用例

**EditMode 测试（4 文件，37 个测试方法）**：OrderValidator / AlertSystem / RecipeData / Economy
**PlayMode 测试（3 文件，15 个 [UnityTest]）**：CustomerFlow / AssassinationFlow / DetectionFlow
**文档**：`TestCases.md`（15 条用例 TC-001~015）+ `BugTrackerTemplate.md`

---

## 四、项目目录结构

```
FoodTruckKiller/
├── .gitignore / .gitattributes
├── Packages/manifest.json
├── ProjectSettings/ProjectVersion.txt
├── Data/                        # JSON 数据配置 (6)
├── Design/                      # 设计文档 + 生成脚本
└── Assets/_Project/
    ├── Art/
    │   ├── Sprites/             # 43 个 PNG (Characters/Tilesets/Props/UI/FX)
    │   └── Audio/               # 13 个 WAV (SFX + Music)
    ├── Scripts/                 # 64 个 C# 脚本
    │   ├── Core/                # 框架 (Events/StateMachine/Singleton/Save)
    │   ├── GameManager/         # 总控
    │   ├── Player/              # 玩家
    │   ├── Interaction/         # 交互
    │   ├── Cooking/             # 烹饪
    │   ├── Customer/            # 顾客 AI
    │   ├── Economy/             # 经济
    │   ├── Assassination/       # 暗杀
    │   ├── Corpse/              # 尸体
    │   ├── Detection/           # 检测/警戒/警察
    │   ├── Inventory/           # 物品栏
    │   └── Audio/               # 音频/手感
    ├── ScriptableObjects/       # 数据资产目录 (待编辑器创建)
    ├── Settings/                # 配置文档 (3)
    └── Tests/                   # EditMode + PlayMode (7)
```

---

## 五、M1 集成注意事项

M0 各角色并行开发，以下集成点需在 M1 阶段统一：

1. **GameEvent 资产创建**：A1 定义了 GameEvent 系统（ScriptableObject），需在 Unity 编辑器 `ScriptableObjects/Events/` 下创建 7 个事件资产（OnOrderServed/OnCustomerDied/OnTargetKilled/OnWanted/OnCorpseFound/OnAlertChanged/OnDayEnd）
2. **GameEvents 静态聚合类**：A5 的 AudioFeedbackBinder 引用了静态 `GameEvents` 类，需 A1 补充或在编辑器中用 SO 引用替代
3. **SingletonMono.Awake 可见性**：A5 用 override 接入，需确认 A1 的 Awake 声明为 protected virtual
4. **ScriptableObject 数据导入**：A3 的 JSON 需在编辑器中转为 RecipeData/CustomerProfile/TargetProfile/KillMethodData 等 .asset
5. **场景搭建**：需在 Unity 编辑器中按 `LevelDesign_DayLevel01.md` 搭建 Tilemap 关卡、放置 Prefab

---

## 六、下一步：M1 经营循环

| 角色 | M1 任务 |
|:---|:---|
| A2 | 烹饪+顾客+经济系统联调，接通 GameEvent |
| A1 | 玩家交互完善、创建 GameEvent 资产、Editor 工具 |
| A3 | 食谱/顾客数据转 SO、Tilemap 铺设、排队点 |
| A4 | 烹饪台/食材/订单气泡精绘、顾客动画 |
| A5 | 烹饪音效接入、收款音效 |
| A6 | 经营流程测试、数值初调 |

**M1 交付目标**：玩家能出餐 → 顾客付款 → 赚钱，经营循环可玩。
