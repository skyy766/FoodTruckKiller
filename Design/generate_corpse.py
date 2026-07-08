"""
FoodTruckKiller - Corpse sprite generator
Agent: A4 (pixel-artist)
Task : 尸体精灵（16x16 暗红 + 黑色描边 + 血渍）

生成 Assets/Resources/Sprites/Corpse/corpse.png 与 corpse.meta。

设计：俯视角度下一具蜷缩在地的尸体。
- 主体：暗红人体剪影
- 描边：黑色加深轮廓
- 血渍：脚下零星溅血点
- 头部圆，躯干卵形，肢体呈蜷缩状
"""

import os
from PIL import Image

PROJECT_ROOT = "/workspace/FoodTruckKiller"
OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "Resources", "Sprites", "Corpse")
OUT_FILE = os.path.join(OUT_DIR, "corpse.png")

# 调色板
TRANSPARENT = (0, 0, 0, 0)
OUTLINE     = (16, 12, 18, 255)
BLOOD_D     = (132, 14, 14, 255)
BLOOD       = (180, 30, 26, 255)
BLOOD_B     = (216, 60, 50, 255)
SKIN_SHADOW = (170, 96, 84, 255)   # 露出皮肤偏紫红（已死）
SHIRT       = (54, 36, 50, 255)    # 衣物深色

# 16x16 像素手绘数据 (None=透明)
#   y=0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15
DATA = [
    "................",  # 0
    "................",  # 1
    "................",  # 2
    "...ooooooo......",  # 3  头
    "..obBBBBBBbo....",  # 4
    "..oBBBBBBBBbo...",  # 5
    "..obBBBBBBBbo...",  # 6
    "..oBoBoBoBBbo...",  # 7  头侧有眼/耳纹
    "...ooSSSSoo.....",  # 8  颈
    "...oShhhhoo.o...",  # 9  肩+臂
    "..oShhhhhhhhoo..",  # 10 躯干
    "..oShhhhhhhhso..",  # 11
    "..oShhhhhhhhso..",  # 12
    "...ohhhhhhho....",  # 13
    "....oooooooo....",  # 14 腿
    "...bb.b..bb.....",  # 15 血渍
]

# 字符到颜色映射
CHAR_MAP = {
    "o": OUTLINE,
    "B": BLOOD_B,
    "b": BLOOD,
    "S": SHIRT,
    "h": BLOOD_D,
    "s": SKIN_SHADOW,
    ".": TRANSPARENT,
}


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    w, h = 16, 16
    img = Image.new("RGBA", (w, h), TRANSPARENT)
    for y, row in enumerate(DATA):
        if y >= h: break
        for x, ch in enumerate(row):
            if x >= w: break
            color = CHAR_MAP.get(ch)
            if color is not None and color != TRANSPARENT:
                img.putpixel((x, y), color)
    img.save(OUT_FILE, "PNG", optimize=True)
    print(f"Generated: {OUT_FILE}")


if __name__ == "__main__":
    main()
