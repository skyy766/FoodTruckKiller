# Pixel Perfect Camera 配置说明（URP 2D）

> 引擎：Unity 6 LTS + Universal 2D Renderer + Pixel Perfect Camera
> 项目：FoodTruckKiller（俯视角像素动作+经营）

## 1. 目标效果

- 像素美术在任意分辨率下保持整数倍缩放，无撕裂、无抖动。
- 摄像机以固定 PPU 对齐像素网格，角色移动不产生亚像素抖动。

## 2. 关键参数

| 参数 | 取值 | 说明 |
|------|------|------|
| Pixels Per Unit (PPU) | **32** | 1 世界单位 = 32 像素，全项目统一 |
| Reference Resolution | **480 x 270** | 16:9 基准分辨率（32 的整数倍） |
| Pixel Snapping | **开启** | 强制像素对齐，消除亚像素 |
| Crop Frame | X/Y 均勾选 | 裁剪溢出边缘，避免黑边 |
| Stretch Fill | 关闭 | 保持像素比，不拉伸 |
| Filter Mode | **Point (no filter)** | 像素硬边，无模糊 |
| Compression | **None** | 精灵纹理不压缩 |
| Filter (Texture Importer) | Point | 导入器同样设为 Point |

## 3. 正交摄像机 Size 推导

```
正交 Size = 参考高度 / PPU / 2
         = 270 / 32 / 2
         ≈ 4.21875 ≈ 4.22
```

即 Camera.orthographicSize ≈ **4.22**。
Pixel Perfect Camera 组件会按 Reference Resolution 自动管理，无需手动设 Size，但若不使用 PPC，请手动设为 4.22。

## 4. 配置步骤

### 4.1 全局 2D 资产设置
1. `Edit > Project Settings > Editor`，Default Behavior Mode = **2D**。
2. `Edit > Project Settings > Quality`，对 URP 资产关闭 MSAA（Anti Aliasing = Disabled），像素游戏不需要抗锯齿。

### 4.2 URP 2D 渲染器
1. 创建 `URP 2D Renderer Data`（Assets/_Project/Settings/）。
2. `Renderer > Filtering > Opaque Layer Mask`：包含 Default、Player、Enemy、Interactable、Prop。
3. 附加到 `URP Asset`（2D）的 Renderer List。

### 4.3 URP Asset (2D)
1. 创建 `Universal Renderer Pipeline Asset (2D)`，放 Assets/_Project/Settings/。
2. `Render Scale = 1`，`HDR = 关闭`，`MSAA = Disabled`。
3. `Default Surface Material` 使用 `Sprite-Lit-Default`。

### 4.4 Pixel Perfect Camera
1. 主摄像机挂 `Pixel Perfect Camera` 组件（package: com.unity.2d.pixel-perfect）。
2. 设置：
   - Assets Pixels Per Unit = **32**
   - Reference Resolution = **480 x 270**
   - Pixel Snapping = **✓**
   - Crop Frame = **X ✓ / Y ✓**
   - Stretch Fill = ✗

### 4.5 精灵导入设置（批量）
对 Assets/_Project/Art/Sprites/** 下所有精灵：
- Texture Type = Sprite (2D and UI)
- Sprite Mode = Multiple（如需切片）/ Single
- Pixels Per Unit = **32**
- Filter Mode = **Point (no filter)**
- Compression = **None**
- Advanced > Generate Physics Shape = 按需

## 5. 注意事项

- **角色移动**：使用 Rigidbody2D + velocity 时，PPC 会自动对齐渲染像素；若仍有抖动，可在 Rigidbody2D 设 Interpolate = None，并确保移动用整数像素步进。
- **Tilemap**：Tilemap Cell Size = 1/PPU = 1（因 PPU=32，1 单位=1 格）。Tile 美术分辨率建议 32x32。
- **动画**：Sprite Atlas 开启，避免批次断裂；像素动画逐帧切换 Sprite，不使用缩放/旋转。
- **UI**：UI Canvas 设为 Screen Space - Overlay，Scaler Reference Resolution = 480x270，Match=0.5；UI 像素图同样 Point + None。
