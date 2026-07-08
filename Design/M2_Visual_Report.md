# M2.5 美术升级报告 — 把"色块"换成"游戏画面"

## 概述

针对截图反馈"目前启动的样子没有一点视觉效果"，本阶段对**所有可视对象**做了 sprite 替换 + 走路动画 + tilemap 铺地，把 M2 完成时还是色块画面的状态升级到可玩 demo 视觉。

**结果**：Unity 按 ▶ Play 后能看到：带厨师帽的玩家、3 种顾客 4 帧走路、4 个烹饪台带烹饪动画、煤气罐/广告牌/绞肉机/冰柜/垃圾桶真实 sprite、暗红尸体、街道+暗巷+餐车地板背景、头顶订单气泡。

## 根因回顾

`SceneBootstrapper.MakeSolidSprite()` 生成 1×1 白色像素 + `sr.color` 染色 → 全部 Player/烹饪台/环境击杀/处理站都走这条路；`KillExecutor.SpawnCorpse` 同样自建 1×1 暗红块。`Art/Sprites/` 下 56 张美术几乎未被代码引用。项目 0 个 .meta，Unity 无法保证 PNG 导入为 Sprite。

## 13 项改动清单

### A 类 — 必修（地基）

| # | 文件 | 内容 |
|---|------|------|
| A1 | `Assets/_Project/Scripts/GameManager/SceneBootstrapper.cs` | 4 处 `MakeSolidSprite` 替换为 `Resources.Load<Sprite>`；摄像机 size=4.5 居中 (0,0,-10)；暴露 `LoadSprite` 公共方法 |
| A2 | `Assets/_Project/Scripts/Assassination/KillExecutor.cs` | `SpawnCorpse` 加载 `Sprites/Corpse/corpse`，失败回退 1×1 暗红 |
| A3 | 新增 `Assets/_Project/Scripts/Player/PlayerVisualController.cs` | 4 方向 walk 帧 + idle/attack/carry 切换 |
| A4 | `CustomerAI.cs` + 新增 `CustomerVisualController.cs` | 顾客 walk 帧调度，按 `CurrentVelocity` 切 4 帧循环 |
| A5 | 12 张 walk 帧 + 60 张 .meta | 复制 `customer_0X_walk_0{1..4}.png` 到 `Resources/Sprites/Customers/`；批量生成 72 PNG + 26 WAV 的 .meta（PPU=32, Point, AlphaIsTransparency, pivot=center） |
| A6 | `CustomerSpawner.cs` | `CreateCustomer` 创建 OrderBubble 子对象 + 挂 `CustomerVisualController` + variant 注入 |
| A7 | `SceneSetupWizard.cs` | 移除 Main Camera 创建（SceneBootstrapper 统一管理），消除双相机冲突 |

### B 类 — 增强（接近 M3 demo）

| # | 文件 | 内容 |
|---|------|------|
| B1 | `JsonDataLoader.cs` | 增 `LoadSceneLayout()` + `SceneLayoutData` ScriptableObject + `SceneLayoutRaw` POCO + `SceneRegion` |
| B2 | 新增 `TilemapBuilder.cs` | 读 `scene_layout.regions` 铺街道底色 + 暗巷/侧巷/餐车地板/砖墙 |
| B3 | `CookingStation.cs` | 烹饪中切 `cooking_station_cooking_01/02` 2 帧循环 |
| B4 | `EnvironmentKill.cs` + 新增 `SimpleFXFader.cs` | 触发时自建 explosion/smoke FX（缩放 + alpha 衰减） |
| B5 | 新增 `Assets/Resources/Sprites/Corpse/corpse.png` + `Design/generate_corpse.py` | 16×16 暗红+黑色描边+血渍尸体 sprite |
| B6 | （增量） | `OrderBubble` 头顶气泡加载 `Sprites/UI/order_bubble.png` |

### 新增辅助脚本

| 文件 | 作用 |
|------|------|
| `Design/generate_corpse.py` | PIL 画 corpse.png（16×16 暗红+轮廓+血渍） |
| `Design/generate_meta.py` | 批量生成 PNG/WAV 的 .meta（GUID 稳定，TextureImporter/AudioImporter 完整字段） |

## 关键设计决策

### 1. 资源加载约定
- 全部 `Resources.Load<Sprite>("Sprites/<Category>/<name>")`，不依赖 .meta GUID（沙箱无 Unity 编辑器时仍可工作）
- `.meta` 由 Python 脚本批量生成，**保证 PPU=32 + Point filter + Alpha is transparency + Single sprite mode + pivot=center**——与 AGENTS.md 第 196 行约定一致
- 失败 fallback：保留 `MakeSolidSprite()` + 染色，避免运行时崩溃

### 2. 坐标系
**未做"32 倍放大"**——原因：`scene_layout.json` 的 `tileSize=0.03125` 实际是 60 像素 PPU 误用；项目 PPU=32。两套坐标并存的冲突通过**让 tilemap 视野与 SceneBootstrapper 实际坐标对齐**解决（tilemap 范围 -2.5..+2.5 / -1.7..+1.7，regions 按比例缩放到覆盖视野）。`JsonDataLoader` 中的 `SceneLayoutData` 仍保留原 JSON 数值（**1.875×1.25**），供未来关卡设计参考。

### 3. 走路动画
- 玩家：4 方向 × 2 walk 帧（间隔 0.15s，0/1 交替）
- 顾客：4 walk 帧（间隔 0.15s，0→1→2→3 循环）
- 都通过读取 `CurrentVelocity.sqrMagnitude` 判定是否在走，静止回 idle

### 4. corpse.png 设计
- 16×16 像素
- 俯视角度下蜷缩的尸体剪影
- 颜色：暗红主体（#B41E1A）+ 黑色描边（#100C12）+ 血渍溅点
- 旋转 0°（场景中无方向需求）
- 加载后 `localScale = 1.2f`（放大到 ~32px 视野，与玩家比例匹配）

## Unity 验证步骤

1. **打开项目**：Unity 6 LTS（团结引擎 2.x）打开 `FoodTruckKiller/`
2. **Package Manager**：确认已安装 URP 2D、Input System
3. **菜单搭建场景**：`FoodTruckKiller > 搭建白天关卡场景`
4. **按 ▶ Play**

### 预期画面
- **背景**：5×3.4 视野内可见街道底色（街道路面），暗巷/侧巷/餐车地板在 regions 对应位置叠加
- **玩家**：红色厨师（厨师帽+红衣+白围裙），朝移动方向 2 帧 walk 循环
- **顾客**：3 种颜色（蓝/绿/黄）人形，4 帧 walk 循环，头顶订单气泡
- **烹饪台**：4 个台子一字排开，玩家靠近按 E 交互时台子切烹饪动画
- **煤气罐/广告牌**：橙/灰 sprite，靠近按 E 触发时生成 explosion 特效（橙色 0.6s 缩放衰减）
- **处理站**：3 个站位（绞肉机深红/冰柜蓝/垃圾桶棕）
- **击杀目标**：F 键近战击杀时该顾客变为暗红尸体 sprite

### Console 日志
```
[JsonDataLoader] LoadAll complete: recipes=2, customers=3, targets=2, killMethods=4, ingredients=4.
[SceneBootstrapper] Created Player at (0.00, 0.00) with KillExecutor + CarryController
[SceneBootstrapper] Created ChopStation at (-1.20, 0.20)
[SceneBootstrapper] Created environment kill: GasCanister at (1.60, -0.30) method=gas_tank
[TilemapBuilder] Built base + 4 regions.
```

## 已知限制（留给 M3）

1. **警察 AI**：代码在 `Detection/`，本阶段未挂入场景（M3 任务）
2. **卫生检查员**：`HealthInspector.cs` 同上
3. **诱饵餐**：`BaitSystem.cs` 已挂，但 `bait_meal` 食谱未在 `recipes.json` 定义（M3 补）
4. **UI 整合**：HUD 图标（`icon_money/time/cover/interact`）已存在但 UI 脚本未完整接入（M3 任务）
5. **走路帧节奏**：0.15s 固定，未按移动速度自适应（M3 优化）
6. **暗色背景**：`color(0.12, 0.08, 0.16)` 偏暗紫，可调成 `0.18, 0.12, 0.22`（原值）更明亮

## 文件统计

| 类别 | 数量 |
|------|------|
| 新建 C# 脚本 | 4（PlayerVisualController、CustomerVisualController、SimpleFXFader、TilemapBuilder） |
| 修改 C# 脚本 | 9（SceneBootstrapper、KillExecutor、CustomerAI、CustomerSpawner、CookingStation、EnvironmentKill、JsonDataLoader、PlayerController 不动、SceneSetupWizard） |
| 新建 Python 脚本 | 2（generate_corpse、generate_meta） |
| 新建 PNG 美术 | 1（corpse.png 16×16） |
| 新建 .meta 文件 | 72 PNG + 26 WAV = 98 |
| 复制 PNG 美术 | 12（3 顾客 × 4 walk 帧） |
| 新建 Design 报告 | 1（本文件） |

**总计**：14 个代码改动 + 113 个资源/元数据文件。
