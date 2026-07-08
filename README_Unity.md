# 餐车杀手 — 本地 Unity 运行指南

## 环境要求

- **Unity 6 LTS**（版本 6000.0.x），通过 Unity Hub 安装
- 安装时勾选 **Universal 2D Render Pipeline** 模块

## 步骤

### 1. 创建 Unity 项目

打开 Unity Hub → New Project → 选择 **Universal 2D** 模板 → 命名（如 FoodTruckKiller）→ Create

### 2. 复制项目文件

将压缩包里的两个文件夹复制到新项目的 `Assets/` 目录下：

```
你的Unity项目/
└── Assets/
    ├── _Project/          ← 从压缩包复制（脚本/美术/音效/数据/测试）
    ├── Resources/         ← 从压缩包复制（运行时加载的 JSON/音效/sprite）
    ├── Settings/          ← Unity 自动生成的 URP 配置（保留）
    └── ...
```

> 如果提示同名文件夹冲突，选择"合并"或"替换"。

### 3. 安装依赖包

菜单 `Window > Package Manager`，确保以下包已安装：

| 包名 | 用途 |
|:---|:---|
| Universal RP | URP 2D 渲染 + Pixel Perfect Camera |
| Input System | 玩家输入（WASD 移动、E 交互） |
| 2D Tilemap | 关卡 Tilemap |
| 2D Sprite | 精灵渲染 |
| Test Framework | 测试（可选） |

> Universal 2D 模板默认已包含大部分包。**Input System** 可能需手动安装。

### 4. 等待编译

复制文件后 Unity 会自动导入资源并编译脚本。观察 Console 窗口：

- ✅ **无红色错误** → 编译成功，继续下一步
- ❌ **有红色错误** → 截图发给我，我来修

### 5. 一键搭建场景

编译通过后，点击菜单：

```
FoodTruckKiller > 搭建白天关卡场景
```

这会自动：
- 创建 `DayLevel_01.unity` 场景
- 配置正交相机（Pixel Perfect, PPU=32）
- 添加 SceneBootstrapper（运行时自动初始化所有系统）
- 把场景加入 Build Settings

> SceneBootstrapper 会在 Play 时自动创建 GameManager、EconomyManager、CustomerSpawner、CookingStation、Player 等全部 GameObject，无需手动拖拽。

### 6. 运行

点击顶部 **▶ Play** 按钮，游戏开始运行。

## 操作说明

| 按键 | 功能 |
|:---|:---|
| WASD / 方向键 | 移动玩家厨师 |
| E | 交互（走到烹饪台前按 E 开始烹饪/出餐） |
| Esc | 暂停（如已实现） |

## 游戏目标（M1 经营循环）

- **赚够 150 元** + **不被通缉** + **时间到（8分钟）结算**
- 顾客自动来排队 → 头顶显示订单 → 你烹饪出餐 → 赚钱

## 项目结构

```
FoodTruckKiller/
├── Assets/
│   ├── _Project/
│   │   ├── Scripts/          # 70 个 C# 脚本
│   │   │   ├── Core/         # 框架（事件/状态机/单例/数据加载）
│   │   │   ├── GameManager/  # 总控 + 场景引导
│   │   │   ├── Player/       # 玩家移动/交互
│   │   │   ├── Cooking/      # 烹饪系统
│   │   │   ├── Customer/     # 顾客 AI（FSM）
│   │   │   ├── Economy/      # 经济/伪装度
│   │   │   ├── Assassination/# 暗杀系统（M2 完善）
│   │   │   ├── Corpse/       # 尸体处理（M3 完善）
│   │   │   ├── Detection/    # 警戒/通缉/警察（M3 完善）
│   │   │   ├── Audio/        # 音效/手感
│   │   │   └── ...
│   │   ├── Art/              # 56 个像素 PNG + 26 个 WAV
│   │   ├── Tests/            # EditMode + PlayMode 测试
│   │   ├── Editor/           # 一键搭建场景工具
│   │   └── Settings/         # 配置说明文档
│   └── Resources/            # 运行时加载（JSON/音效/sprite）
├── Data/                     # JSON 数据配置（源文件）
├── Design/                   # 设计文档 + 生成脚本
└── WebDemo/                  # 浏览器试玩 Demo
```

## 已知限制（M2/M3 完善）

- 暗杀/尸体/检测系统代码已写但**未在场景中挂载**（M2/M3 阶段接入）
- 玩家/顾客视觉为**占位色块**（后续精化为完整像素动画）
- 食材组装目前**自动填充**（后续加手动拾取 UI）
- 单订单串行处理（后续支持多订单并行）

## 遇到问题？

1. **编译错误**：截图 Console 红色报错发给我
2. **运行黑屏**：检查 Camera 是否有 PixelPerfectCamera 组件，PPU=32
3. **顾客不出现**：检查 Console 是否有 GameEvents/JsonDataLoader 相关报错
4. **想试玩核心玩法**：直接打开 `WebDemo/index.html` 用浏览器玩
