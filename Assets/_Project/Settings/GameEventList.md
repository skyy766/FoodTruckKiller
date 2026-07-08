# GameEvent 资产清单

> 项目：FoodTruckKiller
> 架构约定：跨系统通信必须通过 GameEvent（ScriptableObject），禁止直接跨系统 GetComponent。
> 所有事件资产创建于 `Assets/_Project/ScriptableObjects/Events/`，使用菜单
> `Assets > Create > FoodTruckKiller > Events > GameEvent`。

## 事件清单

### 1. OnOrderServed
- **资产名**：`OnOrderServed`
- **触发时机**：玩家将做好的餐品成功交付给顾客时。
- **触发者**：CookingSystem / OrderSystem（A2 玩法）
- **监听者**：EconomySystem（加分/结算金币）、CustomerAI（顾客状态变更：用餐）、DayTimeController（无关）
- **载荷**：无（MVP）；后续可扩展为带顾客 ID 的泛型事件

### 2. OnCustomerDied
- **资产名**：`OnCustomerDied`
- **触发时机**：任一顾客死亡（含非目标顾客的误杀/食物中毒）。
- **触发者**：AssassinationSystem / PoisonSystem（A2 玩法）
- **监听者**：DetectionSystem（产生尸体可被发现）、CorpseSystem（生成尸体实体）、WantedSystem（误杀平民增加通缉）

### 3. OnTargetKilled
- **资产名**：`OnTargetKilled`
- **触发时机**：玩家成功暗杀一个**目标**（非平民）。
- **触发者**：AssassinationSystem（A2 玩法）
- **监听者**：ObjectiveTracker（累计击杀进度）、UIManager（更新目标 HUD）

### 4. OnWanted
- **资产名**：`OnWanted`
- **触发时机**：通缉等级变化（如被目击暗杀/尸体被发现）。
- **触发者**：DetectionSystem / WantedSystem（A2 玩法）
- **监听者**：UIManager（通缉条 HUD）、PoliceAI（派遣警察）、GameManager（满级触发 GameOver）
- **说明**：通缉满级时由 GameManager 切换 GameState.GameOver。

### 5. OnCorpseFound
- **资产名**：`OnCorpseFound`
- **触发时机**：警察/顾客发现尸体时。
- **触发者**：DetectionSystem（A2 玩法）
- **监听者**：WantedSystem（通缉上升）、PoliceAI（进入警戒/搜索状态）、AudioManager（播放警报音效）

### 6. OnAlertChanged
- **资产名**：`OnAlertChanged`
- **触发时机**：区域警戒值变化（0~100），由 AI 感知系统周期性更新。
- **触发者**：DetectionSystem（A2 玩法）
- **监听者**：UIManager（警戒条 HUD）、PoliceAI（根据警戒值切换 StateMachine 状态：Patrol/Suspicious/Search/Combat）

### 7. OnDayEnd
- **资产名**：`OnDayEnd`
- **触发时机**：白天倒计时归零（默认 480 秒）。
- **触发者**：DayTimeController
- **监听者**：GameManager（执行结算：判定 Victory/GameOver）、EconomySystem（结算当日收入）、SaveManager（自动存档）

## 预留事件（后续扩展）

| 事件名 | 触发时机 | 监听者 |
|--------|----------|--------|
| OnIngredientPicked | 拾取食材 | InventorySystem |
| OnCookingStart / OnCookingDone | 烹饪开始/完成 | CookingSystem、UIManager |
| OnPlayerSpotted | 玩家被警察发现 | WantedSystem、GameManager |

## 创建与使用约定

1. **创建**：`Assets/_Project/ScriptableObjects/Events/` 下右键 `Create > FoodTruckKiller > Events > GameEvent`，命名严格匹配上表。
2. **触发**：`onTargetKilled.Raise();`
3. **代码监听**：
   ```csharp
   private void OnEnable()  => onTargetKilled.Register(HandleKill);
   private void OnDisable() => onTargetKilled.Unregister(HandleKill);
   private void HandleKill() { /* ... */ }
   ```
4. **Inspector 监听**：挂 `GameEventListener` 组件，绑定 GameEvent 资产与 UnityEvent。
5. **命名规范**：事件名用 PascalCase，动词过去式（已发生），资产文件名与事件名一致。
