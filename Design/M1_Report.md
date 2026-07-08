# M1 经营循环成果汇报 ——《餐车杀手 Food Truck Killer》

> 6 角色团队 + team lead 协作，M1 经营循环联调完成。
> 核心成果：**出餐 → 顾客付款 → 赚钱** 经营闭环代码层面打通。

---

## 一、M1 目标与达成

| 目标 | 状态 |
|:---|:---:|
| 修复 M0 集成断点（GameEvents 静态类/数据加载/场景引导） | ✅ |
| 烹饪+顾客+经济系统联调 | ✅ |
| 经营循环完整闭环（生成→点单→出餐→加钱→离开→再生） | ✅ |
| 顾客视觉表现（SpriteRenderer + 像素 sprite） | ✅ |
| 音效接入经营循环（Resources 运行时加载） | ✅ |
| 测试对齐实际接口 + 经营流程集成测试 | ✅ |

---

## 二、M1 各角色交付

### A1 主程/架构师 —— Core 框架修复

**新建 5 文件**
- `Core/Events/GameEvents.cs` — 静态事件聚合类，16 个 GameEvent（7 核心 + 9 音效），`Init()` 幂等创建
- `Core/DataLoader/JsonDataLoader.cs` — JSON 运行时加载，POCO 中转 + `CreateInstance<SO>` 填充
- `Core/DataLoader/GameConfig.cs` — 全局配置 POCO
- `Cooking/IngredientData.cs` — 食材 SO
- `GameManager/SceneBootstrapper.cs` — 场景引导，Awake 按 7 步初始化全部子系统

**修改 10 文件** — EconomyManager/GameManager/DayTimeController/ObjectiveTracker/CookingController/PlayerController/PlayerInteractor/CustomerProfile/TargetProfile/KillMethodData，全部迁移为 `GameEvents` 静态访问

### A2 玩法程序 —— 经营循环联调

**迁移 7 文件** Inspector 注入 → GameEvents 静态：CoverSystem / KillExecutor / AssassinationManager / HealthInspector / AlertSystem / WantedSystem / DisposalStation

**联调 3 核心系统**
- CustomerSpawner：从 JsonDataLoader 加载画像/食谱，运行时构造顾客，按概率生成
- CookingController：组装→校验→出餐→触发 OnOrderServed
- 顾客 FSM：Queuing→Ordering→Waiting（推送订单）→Eating→Leaving 完整状态链

### A3 关卡/数值设计 —— 场景数据与数值

- `Data/scene_layout.json` — 完整场景布局（玩家/烹饪台/排队点/生成点/警巡/处理点/环境击杀物坐标 + 区域矩形）
- JSON ↔ SO 字段比对验证（修复 recipes 缺 cookDuration、customers 缺 preferredRecipeId）
- `BalanceDesign.md` v1.1 — M1 数值验算（41元/min，3.7min 达 150 元目标）

### A4 像素美术 —— 精化资源

56 个 PNG（M0 保留 + M1 新增 13）：
- 烹饪台精化（金属台面/煎烤架/切菜板/出餐窗口 + 2 帧火焰动画）
- 食材图标（面包/肉饼/生菜/组合汉堡，16×16 细节像素）
- 订单气泡（32×24 圆角框 + 尖角）
- 顾客 4 帧行走动画 × 3 变体
- HUD 图标精化（金币/盾牌/时钟）
- Tilemap tile sheet（80×16 横排 5 tile）

### A5 音效/手感 —— Resources 运行时加载

- AudioManager 重构：`Resources.LoadAll` 预加载 SFX/Music 到字典，移除 Inspector 注入
- AudioFeedbackBinder 修复：移除反射 hack，直接用 `GameEvents.OnXxx.Unregister`
- 13 个 WAV 复制到 `Resources/Audio/`

### A6 QA/测试 —— 接口对齐与集成测试

- 4 个 EditMode 测试重写（对齐 OrderValidator/AlertSystem/Economy/RecipeData 实际接口）
- 新增 `BusinessLoopTests.cs`（5 个 PlayMode 集成测试：出餐加钱/错误订单/多顾客/时间结算胜负）
- `TestCases.md` 更新 TC-016~020

### Team Lead —— 顾客视觉修复

- CustomerSpawner.CreateCustomer 添加 SpriteRenderer + `Resources.Load<Sprite>` 加载顾客像素 sprite（3 变体循环）
- 顾客 sprite 复制到 `Resources/Sprites/Customers/`

---

## 三、经营循环链路验证

```
CustomerSpawner.TrySpawn()
  ├─ PickProfile() ← JsonDataLoader.CustomerProfiles（按概率权重）
  ├─ CreateCustomer() ← 运行时构造 GO + Rigidbody2D + SpriteRenderer + CustomerAI
  ├─ GetRecipeFor() ← JsonDataLoader.Recipes（随机/偏好）
  ├─ ai.Initialize(profile, order)
  └─ ChangeState(QueuingState)
        ▼
QueuingState → OrderingState → WaitingState
  └─ TryPushOrderToController() → CookingController.SetCurrentOrder()
      └─ GameEvents.OnOrderIn.Raise() → 音效
        ▼
玩家交互 CookingStation(Assemble) → OrderValidator.Validate() → Order.MarkReady()
玩家交互 CookingStation(Serve)    → TryServe()
  ├─ GameEvents.OnServe.Raise()
  └─ GameEvents.OnOrderServed.Raise() ──┐
        ▼                                │
EconomyManager.HandleOrderServed ◄───────┤
  └─ AddMoney(price) → GameEvents.OnCash.Raise() → 音效
        ▼
顾客 WaitingState 检测 Order.State==Served → EatingState → LeavingState
  └─ NotifyCustomerLeft() → Destroy → 新顾客生成
```

---

## 四、项目统计（M0 + M1 累计）

| 类别 | M0 | M1 新增 | 累计 |
|:---|---:|---:|---:|
| C# 脚本 | 64 | +6 | **70** |
| PNG 美术 | 43 | +13 | **56** |
| WAV 音效 | 13 | 0 | **26**（含 Resources 副本） |
| JSON 数据 | 6 | +1 | **7**（+7 Resources 副本） |
| 设计文档 | 8 | +1 | **9** |
| 测试文件 | 7 | +1 | **8** |

---

## 五、已知限制（M2/M3 处理）

1. **食材拾取 UI 未实装**：AssembleStation 自动从食谱填充组装序列，玩家只需按交互
2. **单订单串行处理**：CookingController 一次只处理一个订单
3. **检测/暗杀系统未挂载**：SceneBootstrapper 仅创建经营系统，AlertSystem/WantedSystem/PoliceAI 等待 M2/M3
4. **Player 视觉**：SceneBootstrapper 创建的 Player 尚无 SpriteRenderer（需后续补充玩家 sprite）

---

## 六、下一步：M2 暗杀循环

| 角色 | M2 任务 |
|:---|:---|
| A2 | 暗杀系统 + 诱饵餐 + 击杀执行 + 尸体生成 |
| A1 | CarryController、暗杀相关 GameEvent、Player sprite |
| A3 | 目标配置、击杀方式配置、暗巷动线 |
| A4 | 目标角色、击杀动画、血液 FX |
| A5 | 击杀音效、屏幕震动、命中停顿 |
| A6 | 暗杀流程测试 |

**M2 交付目标**：接单 → 诱杀 → 生成尸体，暗杀循环可玩。
