# Input System 配置说明

> 引擎：Unity 6 LTS + Input System package (com.unity.inputsystem)
> 项目：FoodTruckKiller

## 1. 总览

使用新版 Input System，通过 `InputActionAsset`（资产）配置 Action Map。
代码侧用 `inputAsset.FindAction("ActionName")` 获取并 `Enable/Disable`，详见 `PlayerController.cs` / `PlayerInteractor.cs`。

> 资产存放路径：`Assets/_Project/Settings/InputActions.inputactions`

## 2. Action Map 设计

### 2.1 Player Action Map（玩家操作）

| Action 名 | 类型 | 控制绑定 | 用途 | 消费者 |
|-----------|------|----------|------|--------|
| Move | Value (Vector2) | Keyboard: WASD / Arrow Keys；Gamepad: Left Stick | 8 方向移动 | PlayerController |
| Interact | Button | Keyboard: E；Gamepad: Button South (A) | 交互（烹饪/拾取/暗杀） | PlayerInteractor |
| Cook | Button | Keyboard: F；Gamepad: Button West (X) | 开始/结束烹饪 | CookingSystem (A2) |
| Attack | Button | Keyboard: Space / Mouse Left；Gamepad: Button East (B) | 暗杀攻击 | AssassinationSystem (A2) |
| Carry | Button | Keyboard: Q；Gamepad: Button North (Y) | 拾起物品 | CarryController |
| Drop | Button | Keyboard: R；Gamepad: Left Shoulder | 放下物品 | CarryController |
| Pause | Button | Keyboard: Esc；Gamepad: Start | 暂停 | GameManager |

### 2.2 UI Action Map（菜单操作，预留）

| Action 名 | 类型 | 控制绑定 | 用途 |
|-----------|------|----------|------|
| Navigate | Value (Vector2) | Arrow Keys / D-Pad | 菜单导航 |
| Submit | Button | Enter / Button South | 确认 |
| Cancel | Button | Esc / Button East | 取消 |

## 3. 资产创建步骤

1. `Assets > Create > Input Actions`，命名为 `InputActions`，存放 `Assets/_Project/Settings/`。
2. 新建 Action Map：`Player`、`UI`。
3. 按 2.1/2.2 表格添加 Action 并绑定 Bindings。
4. **Move** Action：
   - Action Type = Value
   - Control Type = Vector2
   - Add Binding(2D Vector Composite)：Up=W/UpArrow，Down=S/DownArrow，Left=A/LeftArrow，Right=D/RightArrow
   - Add Binding：Left Stick [Gamepad]
5. **Interact/Cook/Attack/Carry/Drop/Pause**：
   - Action Type = Button
   - 绑定对应键位
6. 保存资产。

## 4. 代码消费约定

```csharp
// PlayerController.cs（已实现）
[SerializeField] private InputActionAsset inputAsset;
private InputAction moveAction;

void Awake() {
    moveAction = inputAsset.FindAction("Move", throwIfNotFound: false);
}
void OnEnable()  => moveAction?.Enable();
void OnDisable() => moveAction?.Disable();
void Update()    => Vector2 input = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
```

Button 类 Action 监听 performed：
```csharp
interactAction.performed += ctx => { /* 触发交互 */ };
```

## 5. 注意事项

- 启用 Input System：`Project Settings > Player > Active Input Handling = Input System Package (New)` 或 Both。
- Action 在 `OnEnable` 启用、`OnDisable` 禁用，避免场景切换后泄漏。
- Gamepad 支持为预留，MVP 优先键盘。
