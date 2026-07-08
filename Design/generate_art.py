"""
Food Truck Killer - Placeholder Pixel Art Generator
Agent: A4 (pixel-artist)
Task : #3 (M0) + M1 refinement

Generates Hi-Bit placeholder pixel art (PNG, nearest-neighbor / point filter,
lossless) for the M0 milestone. All sprites are color-block placeholders that
are easy to distinguish; they will be replaced with hand-drawn art later.

M1 refinement (this version):
  - cooking_station refined (metal counter / grill / cutting board /
    serving window) + 2-frame flame animation (cooking_station_cooking_01/02)
  - ingredient_bread / patty / lettuce / burger_assembled (16x16 detail icons)
  - order_bubble upgraded to 32x24 with empty interior + bottom-left tail
  - customers now have 4 walk frames each (M0 had 2)
  - HUD icons (money / cover / time) refined with bevels, ticks, highlights
  - tileset_day.png: 80x16 horizontal sheet (5 tiles) for Unity Tilemap

Run:
    python3 generate_art.py

Output root:
    /workspace/FoodTruckKiller/Assets/_Project/Art/Sprites/
"""

import os
from PIL import Image

# ---------------------------------------------------------------------------
# Paths
# ---------------------------------------------------------------------------
PROJECT_ROOT = "/workspace/FoodTruckKiller"
ART_ROOT     = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Art", "Sprites")

# ---------------------------------------------------------------------------
# Palette  (high-saturation urban warm + neon cyberpunk accents, RGBA)
# ---------------------------------------------------------------------------
def C(r, g, b, a=255):
    return (r, g, b, a)

PAL = {
    # neutral / outline
    "transparent": (0, 0, 0, 0),
    "outline":     (20, 18, 26, 255),
    "black":       (28, 26, 34, 255),

    # player chef (red family + white hat)
    "chef_red":      (224, 64, 64, 255),
    "chef_red_d":    (160, 32, 36, 255),
    "chef_pants":    (70, 44, 48, 255),
    "white":         (244, 244, 246, 255),
    "white_shadow":  (200, 198, 205, 255),
    "skin":          (255, 206, 168, 255),
    "skin_shadow":   (214, 164, 130, 255),
    "hair_brown":    (88, 52, 32, 255),
    "hair_black":    (40, 34, 40, 255),

    # customers
    "cust_blue":     (72, 132, 232, 255),
    "cust_blue_d":   (40, 86, 168, 255),
    "cust_green":    (72, 200, 96, 255),
    "cust_green_d":  (38, 148, 60, 255),
    "cust_yellow":   (242, 220, 60, 255),
    "cust_yellow_d": (188, 168, 28, 255),

    # enemies
    "police_blue":   (44, 58, 124, 255),
    "police_blue_d": (26, 36, 80, 255),
    "badge_gold":    (244, 212, 60, 255),
    "inspector_w":   (232, 232, 238, 255),
    "inspector_g":   (168, 168, 178, 255),
    "inspector_cap": (40, 38, 46, 255),

    # tiles
    "street":        (72, 72, 80, 255),
    "street_d":      (54, 54, 62, 255),
    "street_line":   (96, 92, 100, 255),
    "sidewalk":      (174, 174, 180, 255),
    "sidewalk_d":    (140, 140, 146, 255),
    "wall":          (62, 58, 66, 255),
    "wall_brick":    (96, 72, 68, 255),
    "wall_brick_d":  (74, 52, 50, 255),
    "alley":         (52, 36, 30, 255),
    "alley_d":       (36, 24, 20, 255),
    "truck_floor":   (148, 98, 56, 255),
    "truck_floor_d": (110, 70, 40, 255),
    "truck_plank":   (180, 128, 78, 255),

    # props
    "metal":         (152, 152, 162, 255),
    "metal_d":       (102, 102, 112, 255),
    "metal_l":       (190, 190, 200, 255),
    "fire_orange":   (252, 132, 30, 255),
    "fire_yellow":   (252, 222, 60, 255),
    "fire_red":      (210, 60, 20, 255),
    "grinder_red":   (134, 32, 32, 255),
    "grinder_red_d": (92, 20, 20, 255),
    "silver":        (206, 206, 216, 255),
    "silver_d":      (150, 150, 162, 255),
    "freezer_blue":  (176, 224, 244, 255),
    "freezer_blue_d":(124, 184, 216, 255),
    "freezer_handle":(220, 220, 228, 255),
    "gas_red":       (204, 52, 40, 255),
    "gas_red_d":     (150, 30, 24, 255),
    "gas_cap":       (60, 60, 70, 255),
    "bill_post":     (120, 90, 60, 255),
    "bill_cyan":     (60, 222, 230, 255),
    "bill_magenta":  (224, 60, 182, 255),
    "bill_yellow":   (244, 220, 60, 255),
    "bill_bg":       (30, 28, 40, 255),
    "trash_green":   (52, 92, 56, 255),
    "trash_green_d": (34, 66, 40, 255),
    "trash_lid":     (72, 118, 76, 255),

    # ui
    "gold":          (252, 200, 40, 255),
    "gold_d":        (200, 150, 20, 255),
    "shield_green":  (64, 204, 104, 255),
    "shield_green_d":(36, 150, 70, 255),
    "clock_face":    (244, 244, 244, 255),
    "clock_rim":     (40, 40, 48, 255),
    "bubble":        (250, 250, 250, 255),
    "bubble_tail":   (220, 220, 220, 255),
    "e_yellow":      (252, 222, 52, 255),
    "e_yellow_d":    (200, 168, 24, 255),

    # fx
    "blood":         (192, 30, 30, 255),
    "blood_d":       (132, 14, 14, 255),
    "blood_bright":  (232, 70, 60, 255),
    "expl_core":     (255, 244, 210, 255),
    "expl_mid":      (252, 168, 36, 255),
    "expl_outer":    (204, 64, 22, 255),
    "expl_smoke":    (120, 90, 80, 255),
    "smoke_gray":    (124, 124, 130, 255),
    "smoke_dark":    (84, 84, 90, 255),
    "smoke_light":   (170, 170, 176, 255),
    "poison":        (124, 232, 80, 255),
    "poison_d":      (72, 164, 52, 255),
    "poison_light":  (180, 250, 140, 255),

    # ingredients (M1)
    "bread_crust":   (180, 110, 50, 255),
    "bread_crust_d": (132, 76, 30, 255),
    "bread_inner":   (252, 218, 156, 255),
    "bread_inner_d": (220, 178, 110, 255),
    "sesame":        (252, 232, 168, 255),
    "patty":         (102, 56, 30, 255),
    "patty_d":       (66, 34, 16, 255),
    "patty_l":       (140, 80, 44, 255),
    "lettuce":       (124, 196, 70, 255),
    "lettuce_d":     (78, 148, 42, 255),
    "lettuce_l":     (180, 232, 110, 255),
    "cheese":        (252, 200, 40, 255),
    "cheese_d":      (200, 150, 20, 255),
}

# ---------------------------------------------------------------------------
# Canvas helpers  (canvas = 2D list[y][x] of color tuples or None=transparent)
# ---------------------------------------------------------------------------
def new_canvas(w, h):
    return [[None] * w for _ in range(h)]

def cw(c):
    return len(c[0])

def ch(c):
    return len(c)

def fill_rect(c, x1, y1, x2, y2, color):
    if color is None:
        return
    h, w = ch(c), cw(c)
    for y in range(max(0, y1), min(h, y2 + 1)):
        for x in range(max(0, x1), min(w, x2 + 1)):
            c[y][x] = color

def set_px(c, x, y, color):
    if color is None:
        return
    h, w = ch(c), cw(c)
    if 0 <= x < w and 0 <= y < h:
        c[y][x] = color

def to_image(c):
    h, w = ch(c), cw(c)
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = img.load()
    for y in range(h):
        for x in range(w):
            col = c[y][x]
            if col is not None:
                px[x, y] = col
    return img

def hflip_image(img):
    return img.transpose(Image.FLIP_LEFT_RIGHT)

# ---------------------------------------------------------------------------
# Character drawing (32x32)
# ---------------------------------------------------------------------------
def draw_head(c, skin, hair, direction, hat=None, hat_brim=None):
    """Draw head + optional hat. hat is the cap color, hat_brim the brim color."""
    if direction == "down":
        fill_rect(c, 11, 7, 20, 12, skin)            # face
        set_px(c, 12, 6, PAL["skin_shadow"])
        set_px(c, 13, 9, PAL["black"])               # eyes
        set_px(c, 18, 9, PAL["black"])
        # mouth
        set_px(c, 15, 11, (180, 110, 100, 255))
        set_px(c, 16, 11, (180, 110, 100, 255))
        if hat:
            fill_rect(c, 10, 2, 21, 5, hat)          # hat top
            fill_rect(c, 9, 6, 22, 6, hat_brim or hat)  # brim
    elif direction == "up":
        fill_rect(c, 11, 6, 20, 12, hair)            # back of head
        if hat:
            fill_rect(c, 10, 2, 21, 5, hat)
            fill_rect(c, 9, 6, 22, 6, hat_brim or hat)
    elif direction == "left":
        fill_rect(c, 10, 7, 17, 12, skin)            # profile face
        set_px(c, 11, 9, PAL["black"])               # one eye
        set_px(c, 12, 11, (180, 110, 100, 255))      # mouth
        fill_rect(c, 17, 6, 19, 12, hair)            # back hair
        if hat:
            fill_rect(c, 9, 2, 18, 5, hat)
            fill_rect(c, 8, 6, 19, 6, hat_brim or hat)
    elif direction == "right":
        fill_rect(c, 14, 7, 21, 12, skin)
        set_px(c, 20, 9, PAL["black"])
        set_px(c, 19, 11, (180, 110, 100, 255))
        fill_rect(c, 12, 6, 14, 12, hair)
        if hat:
            fill_rect(c, 13, 2, 22, 5, hat)
            fill_rect(c, 12, 6, 23, 6, hat_brim or hat)


def draw_torso(c, body, body_d, direction):
    """Torso + arms (rest pose)."""
    # main torso
    fill_rect(c, 10, 13, 21, 23, body)
    # shading on one side
    if direction in ("down", "up"):
        fill_rect(c, 18, 13, 21, 23, body_d)
        # arms down at sides
        fill_rect(c, 8, 14, 9, 21, body)
        fill_rect(c, 22, 14, 23, 21, body)
    elif direction == "left":
        fill_rect(c, 17, 13, 21, 23, body_d)
        fill_rect(c, 9, 14, 10, 21, body)            # near arm
    elif direction == "right":
        fill_rect(c, 10, 13, 14, 23, body_d)
        fill_rect(c, 21, 14, 22, 21, body)


def draw_legs(c, pants, direction, frame):
    """Up to four walk frames. frame=1,2 (M0) + 3,4 (M1 extra).

    Cycle: 1=contact, 2=stride-L, 3=contact-mirror, 4=stride-R.
    Frames 1 and 2 are kept identical to M0 so existing player sprites
    remain byte-stable.
    """
    if direction in ("down", "up"):
        if frame == 1:
            fill_rect(c, 12, 24, 15, 30, pants)
            fill_rect(c, 16, 24, 19, 30, pants)
        elif frame == 2:
            fill_rect(c, 11, 24, 14, 29, pants)      # left leg forward
            fill_rect(c, 17, 24, 20, 30, pants)
        elif frame == 3:
            # contact-mirror: left planted, right heel slightly lifted
            fill_rect(c, 12, 24, 15, 30, pants)
            fill_rect(c, 16, 24, 19, 29, pants)
        elif frame == 4:
            fill_rect(c, 11, 24, 14, 30, pants)      # right leg forward
            fill_rect(c, 17, 24, 20, 29, pants)
    elif direction == "left":
        if frame == 1:
            fill_rect(c, 12, 24, 16, 30, pants)
            fill_rect(c, 15, 24, 18, 28, pants)
        elif frame == 2:
            fill_rect(c, 10, 24, 14, 29, pants)
            fill_rect(c, 16, 24, 19, 30, pants)
        elif frame == 3:
            # mirror: rear foot lifts slightly
            fill_rect(c, 12, 24, 16, 29, pants)
            fill_rect(c, 15, 24, 18, 28, pants)
        elif frame == 4:
            fill_rect(c, 11, 24, 15, 30, pants)
            fill_rect(c, 16, 24, 19, 29, pants)
    elif direction == "right":
        if frame == 1:
            fill_rect(c, 15, 24, 19, 30, pants)
            fill_rect(c, 13, 24, 16, 28, pants)
        elif frame == 2:
            fill_rect(c, 17, 24, 21, 29, pants)
            fill_rect(c, 12, 24, 15, 30, pants)
        elif frame == 3:
            # mirror: rear foot lifts slightly
            fill_rect(c, 15, 24, 19, 29, pants)
            fill_rect(c, 13, 24, 16, 28, pants)
        elif frame == 4:
            fill_rect(c, 16, 24, 20, 30, pants)
            fill_rect(c, 12, 24, 15, 29, pants)


def draw_humanoid(direction, frame, body, body_d, pants, skin, hair,
                  hat=None, hat_brim=None, bob=False, shadow_first=False):
    c = new_canvas(32, 32)
    # shadow_first: draw ground shadow before legs so per-frame leg differences
    # at y=30 are not overwritten (used by customer 4-frame walk).
    if shadow_first:
        fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    draw_head(c, skin, hair, direction, hat, hat_brim)
    draw_torso(c, body, body_d, direction)
    draw_legs(c, pants, direction, frame)
    if not shadow_first:
        # M0 behavior: shadow on top of legs (keeps player sprites byte-stable)
        fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    # optional 1px vertical bob on stride frames (2 & 4) for livelier walk
    if bob and frame in (2, 4):
        shifted = new_canvas(32, 32)
        for y in range(31):
            for x in range(32):
                shifted[y + 1][x] = c[y][x]
        c = shifted
    return to_image(c)


# ----- Player (chef, red + white hat) -------------------------------------
def player_walk(direction, frame):
    return draw_humanoid(
        direction, frame,
        body=PAL["chef_red"], body_d=PAL["chef_red_d"],
        pants=PAL["chef_pants"], skin=PAL["skin"], hair=PAL["hair_brown"],
        hat=PAL["white"], hat_brim=PAL["white_shadow"],
    )

def player_idle():
    c = new_canvas(32, 32)
    draw_head(c, PAL["skin"], PAL["hair_brown"], "down",
              hat=PAL["white"], hat_brim=PAL["white_shadow"])
    draw_torso(c, PAL["chef_red"], PAL["chef_red_d"], "down")
    # legs together (idle)
    fill_rect(c, 12, 24, 15, 30, PAL["chef_pants"])
    fill_rect(c, 16, 24, 19, 30, PAL["chef_pants"])
    fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    return to_image(c)

def player_cook():
    c = new_canvas(32, 32)
    draw_head(c, PAL["skin"], PAL["hair_brown"], "down",
              hat=PAL["white"], hat_brim=PAL["white_shadow"])
    draw_torso(c, PAL["chef_red"], PAL["chef_red_d"], "down")
    fill_rect(c, 12, 24, 15, 30, PAL["chef_pants"])
    fill_rect(c, 16, 24, 19, 30, PAL["chef_pants"])
    # pan in front (left hand)
    fill_rect(c, 4, 17, 9, 19, PAL["metal_d"])       # pan body
    fill_rect(c, 3, 18, 4, 18, PAL["black"])         # handle
    # flame above pan
    set_px(c, 6, 14, PAL["fire_yellow"])
    set_px(c, 7, 14, PAL["fire_orange"])
    set_px(c, 6, 15, PAL["fire_orange"])
    set_px(c, 5, 16, PAL["fire_red"])
    set_px(c, 7, 16, PAL["fire_red"])
    fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    return to_image(c)

def player_attack():
    c = new_canvas(32, 32)
    draw_head(c, PAL["skin"], PAL["hair_brown"], "right",
              hat=PAL["white"], hat_brim=PAL["white_shadow"])
    draw_torso(c, PAL["chef_red"], PAL["chef_red_d"], "right")
    fill_rect(c, 17, 24, 21, 29, PAL["chef_pants"])
    fill_rect(c, 12, 24, 15, 30, PAL["chef_pants"])
    # extended arm + cleaver (right side)
    fill_rect(c, 22, 14, 26, 15, PAL["skin"])        # arm
    fill_rect(c, 26, 12, 29, 17, PAL["silver"])      # cleaver blade
    fill_rect(c, 26, 11, 27, 18, PAL["silver_d"])
    fill_rect(c, 25, 14, 26, 15, PAL["black"])       # handle
    fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    return to_image(c)

def player_carry():
    c = new_canvas(32, 32)
    draw_head(c, PAL["skin"], PAL["hair_brown"], "down",
              hat=PAL["white"], hat_brim=PAL["white_shadow"])
    draw_torso(c, PAL["chef_red"], PAL["chef_red_d"], "down")
    fill_rect(c, 12, 24, 15, 30, PAL["chef_pants"])
    fill_rect(c, 16, 24, 19, 30, PAL["chef_pants"])
    # crate carried in front (both arms forward)
    fill_rect(c, 9, 15, 22, 21, PAL["truck_floor"])  # wooden crate
    fill_rect(c, 9, 15, 22, 15, PAL["truck_floor_d"])
    fill_rect(c, 9, 21, 22, 21, PAL["truck_floor_d"])
    set_px(c, 13, 18, PAL["truck_floor_d"])
    set_px(c, 18, 18, PAL["truck_floor_d"])
    fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    return to_image(c)


# ----- Customers (blue / green / yellow) ----------------------------------
def customer_base(variant):
    """Return (body, body_d, hair) for a customer variant 1..3."""
    if variant == 1:
        return PAL["cust_blue"], PAL["cust_blue_d"], PAL["hair_black"]
    if variant == 2:
        return PAL["cust_green"], PAL["cust_green_d"], PAL["hair_brown"]
    return PAL["cust_yellow"], PAL["cust_yellow_d"], PAL["hair_black"]

def customer_idle(variant):
    body, body_d, hair = customer_base(variant)
    c = new_canvas(32, 32)
    draw_head(c, PAL["skin"], hair, "down")
    draw_torso(c, body, body_d, "down")
    fill_rect(c, 12, 24, 15, 30, body_d)
    fill_rect(c, 16, 24, 19, 30, body_d)
    fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    return to_image(c)

def customer_walk(variant, frame):
    body, body_d, hair = customer_base(variant)
    return draw_humanoid("down", frame, body, body_d, body_d,
                         PAL["skin"], hair, bob=True, shadow_first=True)


# ----- Enemies ------------------------------------------------------------
def enemy_police():
    c = new_canvas(32, 32)
    # cap
    fill_rect(c, 10, 2, 21, 5, PAL["police_blue_d"])
    fill_rect(c, 9, 6, 22, 6, PAL["police_blue_d"])
    # face
    fill_rect(c, 11, 7, 20, 12, PAL["skin"])
    set_px(c, 13, 9, PAL["black"])
    set_px(c, 18, 9, PAL["black"])
    # torso (dark blue uniform)
    fill_rect(c, 10, 13, 21, 23, PAL["police_blue"])
    fill_rect(c, 18, 13, 21, 23, PAL["police_blue_d"])
    # arms
    fill_rect(c, 8, 14, 9, 21, PAL["police_blue"])
    fill_rect(c, 22, 14, 23, 21, PAL["police_blue"])
    # badge (gold dots)
    set_px(c, 12, 15, PAL["badge_gold"])
    set_px(c, 13, 15, PAL["badge_gold"])
    set_px(c, 12, 16, PAL["badge_gold"])
    set_px(c, 13, 16, PAL["badge_gold"])
    # belt
    fill_rect(c, 10, 22, 21, 22, PAL["black"])
    # legs
    fill_rect(c, 12, 24, 15, 30, PAL["police_blue_d"])
    fill_rect(c, 16, 24, 19, 30, PAL["police_blue_d"])
    fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    return to_image(c)

def enemy_inspector():
    c = new_canvas(32, 32)
    # cap (black)
    fill_rect(c, 10, 2, 21, 5, PAL["inspector_cap"])
    fill_rect(c, 9, 6, 22, 6, PAL["inspector_cap"])
    # face
    fill_rect(c, 11, 7, 20, 12, PAL["skin"])
    set_px(c, 13, 9, PAL["black"])
    set_px(c, 18, 9, PAL["black"])
    # white uniform coat
    fill_rect(c, 10, 13, 21, 23, PAL["inspector_w"])
    fill_rect(c, 18, 13, 21, 23, PAL["inspector_g"])
    # arms
    fill_rect(c, 8, 14, 9, 21, PAL["inspector_w"])
    fill_rect(c, 22, 14, 23, 21, PAL["inspector_w"])
    # coat center line + buttons
    fill_rect(c, 15, 13, 16, 23, PAL["inspector_g"])
    set_px(c, 16, 15, PAL["black"])
    set_px(c, 16, 18, PAL["black"])
    set_px(c, 16, 21, PAL["black"])
    # clip board (right hand)
    fill_rect(c, 23, 15, 27, 21, PAL["white"])
    fill_rect(c, 23, 15, 27, 15, PAL["metal_d"])
    # legs
    fill_rect(c, 12, 24, 15, 30, PAL["inspector_g"])
    fill_rect(c, 16, 24, 19, 30, PAL["inspector_g"])
    fill_rect(c, 10, 30, 21, 30, (20, 16, 24, 90))
    return to_image(c)


# ---------------------------------------------------------------------------
# Tilesets (16x16)
# ---------------------------------------------------------------------------
def tile_street():
    c = new_canvas(16, 16)
    fill_rect(c, 0, 0, 15, 15, PAL["street"])
    # asphalt speckle
    for (x, y) in [(2, 3), (5, 1), (11, 2), (13, 5), (3, 8), (8, 6),
                   (10, 10), (14, 12), (1, 11), (6, 13)]:
        set_px(c, x, y, PAL["street_d"])
    for (x, y) in [(4, 5), (9, 3), (12, 9), (7, 12)]:
        set_px(c, x, y, PAL["street_line"])
    # a crack
    set_px(c, 7, 7, PAL["street_d"])
    set_px(c, 8, 8, PAL["street_d"])
    set_px(c, 9, 9, PAL["street_d"])
    return to_image(c)

def tile_sidewalk():
    c = new_canvas(16, 16)
    fill_rect(c, 0, 0, 15, 15, PAL["sidewalk"])
    # tile grid lines
    for y in range(0, 16, 8):
        fill_rect(c, 0, y, 15, y, PAL["sidewalk_d"])
    for x in range(0, 16, 8):
        fill_rect(c, x, 0, x, 15, PAL["sidewalk_d"])
    # speckle
    for (x, y) in [(3, 3), (10, 4), (6, 11), (13, 12)]:
        set_px(c, x, y, PAL["sidewalk_d"])
    return to_image(c)

def tile_wall():
    c = new_canvas(16, 16)
    fill_rect(c, 0, 0, 15, 15, PAL["wall"])
    # brick pattern
    for y in range(0, 16, 4):
        fill_rect(c, 0, y, 15, y, PAL["wall_brick_d"])
    # offset bricks
    for row in range(4):
        offset = 0 if row % 2 == 0 else 4
        for x in range(-4 + offset, 16, 8):
            fill_rect(c, x, row * 4, x, row * 4 + 3, PAL["wall_brick_d"])
        for x in range(0, 16, 8):
            xx = (x + offset) % 16
            fill_rect(c, xx, row * 4 + 1, min(15, xx + 6), row * 4 + 3,
                      PAL["wall_brick"])
    return to_image(c)

def tile_alley():
    c = new_canvas(16, 16)
    fill_rect(c, 0, 0, 15, 15, PAL["alley"])
    # grime
    for (x, y) in [(2, 2), (7, 1), (12, 3), (4, 6), (10, 7),
                   (1, 9), (8, 11), (13, 10), (5, 13), (11, 14)]:
        set_px(c, x, y, PAL["alley_d"])
    # puddle
    fill_rect(c, 5, 9, 9, 10, (40, 50, 70, 255))
    set_px(c, 6, 9, (70, 90, 120, 255))
    # crack
    set_px(c, 12, 6, PAL["alley_d"])
    set_px(c, 13, 7, PAL["alley_d"])
    set_px(c, 14, 8, PAL["alley_d"])
    return to_image(c)

def tile_truck_floor():
    c = new_canvas(16, 16)
    fill_rect(c, 0, 0, 15, 15, PAL["truck_floor"])
    # planks (horizontal)
    for y in range(0, 16, 5):
        fill_rect(c, 0, y, 15, y, PAL["truck_floor_d"])
    # plank highlights
    for y in [1, 6, 11]:
        fill_rect(c, 0, y, 15, y, PAL["truck_plank"])
    # wood grain dots
    for (x, y) in [(3, 2), (10, 3), (6, 7), (12, 8), (2, 12), (9, 13)]:
        set_px(c, x, y, PAL["truck_floor_d"])
    return to_image(c)


def tileset_day():
    """Horizontal 80x16 tile sheet: 5 tiles side-by-side.

    Order: street | sidewalk | wall | alley | truck_floor
    Kept individually too (M0 files preserved) for backwards compatibility.
    """
    tiles = [tile_street(), tile_sidewalk(), tile_wall(),
             tile_alley(), tile_truck_floor()]
    sheet = Image.new("RGBA", (16 * len(tiles), 16), (0, 0, 0, 0))
    for i, t in enumerate(tiles):
        sheet.paste(t, (i * 16, 0), t)
    return sheet


# ---------------------------------------------------------------------------
# Props (32x32)
# ---------------------------------------------------------------------------
def _draw_cooking_station_base(c, with_food=False):
    """Draw the refined 32x32 cooking station body.

    Layout (left -> right):
      - wooden cutting board (left)
      - orange grill with bars (center)
      - metal counter with serving window (right)
    """
    # --- metal counter base (full width) ---
    fill_rect(c, 2, 14, 29, 28, PAL["metal_d"])        # body
    fill_rect(c, 2, 14, 29, 15, PAL["metal_l"])        # top highlight
    fill_rect(c, 2, 27, 29, 28, PAL["black"])          # bottom shadow
    # side panels shading
    fill_rect(c, 2, 16, 3, 26, PAL["metal"])           # left bevel
    fill_rect(c, 28, 16, 29, 26, PAL["metal"])         # right bevel
    # cabinet doors
    fill_rect(c, 5, 18, 13, 25, PAL["metal"])
    fill_rect(c, 5, 18, 13, 18, PAL["metal_d"])
    fill_rect(c, 5, 25, 13, 25, PAL["metal_d"])
    fill_rect(c, 5, 18, 5, 25, PAL["metal_d"])
    fill_rect(c, 13, 18, 13, 25, PAL["metal_d"])
    set_px(c, 9, 22, PAL["silver_d"])                  # handle dot
    # knobs (gold)
    set_px(c, 6, 26, PAL["badge_gold"])
    set_px(c, 9, 26, PAL["badge_gold"])
    set_px(c, 12, 26, PAL["badge_gold"])

    # --- wooden cutting board (left top) ---
    fill_rect(c, 4, 10, 11, 14, PAL["truck_plank"])    # board surface
    fill_rect(c, 4, 10, 11, 10, PAL["truck_floor"])    # top edge darker
    fill_rect(c, 4, 14, 11, 14, PAL["truck_floor_d"])  # bottom edge
    fill_rect(c, 4, 11, 4, 13, PAL["truck_floor_d"])   # left edge
    fill_rect(c, 11, 11, 11, 13, PAL["truck_floor_d"]) # right edge
    # wood grain
    set_px(c, 6, 12, PAL["truck_floor_d"])
    set_px(c, 8, 11, PAL["truck_floor_d"])
    set_px(c, 9, 13, PAL["truck_floor_d"])
    # a tomato/onion on the board (small red + white dots)
    set_px(c, 6, 11, PAL["blood"])
    set_px(c, 7, 11, PAL["blood_bright"])
    set_px(c, 9, 12, PAL["white"])

    # --- orange grill (center top) ---
    # grill frame
    fill_rect(c, 13, 9, 22, 14, PAL["black"])          # grill pit
    fill_rect(c, 12, 9, 23, 9, PAL["metal_d"])         # top rim
    fill_rect(c, 12, 14, 23, 14, PAL["metal_d"])       # bottom rim
    fill_rect(c, 12, 9, 12, 14, PAL["metal_d"])        # left rim
    fill_rect(c, 23, 9, 23, 14, PAL["metal_d"])        # right rim
    # orange glow under bars
    fill_rect(c, 14, 11, 22, 13, PAL["fire_orange"])
    # grill bars (metal, vertical)
    for x in range(14, 23, 2):
        fill_rect(c, x, 10, x, 13, PAL["metal_l"])
        set_px(c, x, 10, PAL["silver"])
        set_px(c, x, 13, PAL["metal_d"])
    # hot embers between bars
    set_px(c, 15, 12, PAL["fire_yellow"])
    set_px(c, 17, 12, PAL["fire_red"])
    set_px(c, 19, 11, PAL["fire_yellow"])
    set_px(c, 21, 13, PAL["fire_red"])

    # food on grill (optional)
    if with_food:
        # patty (brown/red)
        fill_rect(c, 15, 10, 18, 10, PAL["blood_d"])
        fill_rect(c, 15, 10, 18, 10, PAL["grinder_red_d"])
        set_px(c, 16, 9, PAL["blood_bright"])
        # sausage
        fill_rect(c, 20, 10, 22, 10, PAL["grinder_red"])
        set_px(c, 21, 9, PAL["fire_yellow"])

    # --- serving window (right top) ---
    # window opening (dark interior)
    fill_rect(c, 24, 9, 29, 14, PAL["black"])
    # window frame
    fill_rect(c, 24, 9, 29, 9, PAL["metal_l"])
    fill_rect(c, 24, 14, 29, 14, PAL["metal_d"])
    fill_rect(c, 24, 9, 24, 14, PAL["metal_d"])
    fill_rect(c, 29, 9, 29, 14, PAL["metal_d"])
    # shelf (where finished dishes sit)
    fill_rect(c, 24, 12, 29, 12, PAL["truck_plank"])
    set_px(c, 25, 12, PAL["truck_floor_d"])
    set_px(c, 28, 12, PAL["truck_floor_d"])
    # a finished plate in the window
    fill_rect(c, 25, 11, 28, 11, PAL["white"])
    set_px(c, 26, 10, PAL["cust_yellow"])
    set_px(c, 27, 10, PAL["cust_yellow"])

    # --- legs ---
    fill_rect(c, 3, 28, 5, 31, PAL["metal_d"])
    fill_rect(c, 26, 28, 28, 31, PAL["metal_d"])
    set_px(c, 3, 31, PAL["black"])
    set_px(c, 28, 31, PAL["black"])


def prop_cooking_station():
    """Refined cooking station (idle, no flames above)."""
    c = new_canvas(32, 32)
    _draw_cooking_station_base(c, with_food=False)
    # soft ground shadow
    fill_rect(c, 2, 31, 29, 31, (20, 16, 24, 90))
    return to_image(c)


def prop_cooking_station_cooking(frame):
    """Cooking station with animated flames. frame=1 or 2."""
    c = new_canvas(32, 32)
    _draw_cooking_station_base(c, with_food=True)
    # flames rising above the grill (x: 13..22, y: 1..8)
    if frame == 1:
        # tall flame
        fill_rect(c, 14, 6, 21, 8, PAL["fire_orange"])
        fill_rect(c, 15, 3, 20, 5, PAL["fire_yellow"])
        fill_rect(c, 16, 1, 19, 2, PAL["fire_yellow"])
        fill_rect(c, 16, 7, 21, 8, PAL["fire_red"])
        # highlights
        set_px(c, 16, 5, (255, 244, 210, 255))
        set_px(c, 19, 4, (255, 244, 210, 255))
        set_px(c, 17, 2, (255, 255, 255, 255))
        # side flickers
        set_px(c, 13, 7, PAL["fire_orange"])
        set_px(c, 22, 6, PAL["fire_orange"])
        set_px(c, 12, 8, PAL["fire_red"])
        set_px(c, 23, 8, PAL["fire_red"])
        # smoke wisp
        set_px(c, 18, 0, PAL["smoke_light"])
    else:
        # frame 2: shorter, wider flame
        fill_rect(c, 13, 7, 22, 8, PAL["fire_orange"])
        fill_rect(c, 14, 4, 21, 6, PAL["fire_yellow"])
        fill_rect(c, 15, 2, 20, 3, PAL["fire_yellow"])
        fill_rect(c, 15, 7, 22, 8, PAL["fire_red"])
        # highlights
        set_px(c, 16, 6, (255, 244, 210, 255))
        set_px(c, 19, 5, (255, 244, 210, 255))
        set_px(c, 17, 3, (255, 255, 255, 255))
        set_px(c, 18, 1, PAL["fire_yellow"])
        # side flickers (different pattern)
        set_px(c, 12, 7, PAL["fire_orange"])
        set_px(c, 23, 7, PAL["fire_orange"])
        set_px(c, 11, 8, PAL["fire_red"])
        set_px(c, 24, 8, PAL["fire_red"])
        # smoke wisp (offset)
        set_px(c, 19, 0, PAL["smoke_light"])
        set_px(c, 17, 0, PAL["smoke_gray"])
    # soft ground shadow
    fill_rect(c, 2, 31, 29, 31, (20, 16, 24, 90))
    return to_image(c)


def prop_grinder():
    c = new_canvas(32, 32)
    # base
    fill_rect(c, 6, 22, 25, 30, PAL["grinder_red_d"])
    fill_rect(c, 6, 22, 25, 24, PAL["grinder_red"])
    # body (main cylinder)
    fill_rect(c, 9, 10, 22, 22, PAL["grinder_red"])
    fill_rect(c, 20, 10, 22, 22, PAL["grinder_red_d"])
    fill_rect(c, 9, 10, 11, 22, PAL["grinder_red_d"])
    # top funnel
    fill_rect(c, 11, 4, 20, 10, PAL["silver"])
    fill_rect(c, 11, 4, 20, 5, PAL["silver_d"])
    fill_rect(c, 11, 4, 12, 10, PAL["silver_d"])
    # chute (front)
    fill_rect(c, 13, 16, 18, 19, PAL["silver_d"])
    # ground meat coming out
    fill_rect(c, 13, 19, 18, 21, PAL["blood"])
    set_px(c, 14, 21, PAL["blood_d"])
    set_px(c, 17, 21, PAL["blood_d"])
    # crank handle
    fill_rect(c, 23, 13, 27, 14, PAL["silver_d"])
    fill_rect(c, 26, 12, 27, 16, PAL["silver_d"])
    # bolts
    set_px(c, 11, 22, PAL["silver_d"])
    set_px(c, 20, 22, PAL["silver_d"])
    return to_image(c)

def prop_freezer():
    c = new_canvas(32, 32)
    # body
    fill_rect(c, 4, 4, 27, 30, PAL["freezer_blue_d"])
    fill_rect(c, 4, 4, 27, 6, PAL["freezer_blue"])
    fill_rect(c, 4, 4, 6, 30, PAL["freezer_blue"])
    # door panel
    fill_rect(c, 7, 7, 24, 28, PAL["freezer_blue"])
    fill_rect(c, 7, 7, 9, 28, PAL["freezer_blue_d"])
    # handle
    fill_rect(c, 21, 10, 23, 22, PAL["freezer_handle"])
    fill_rect(c, 21, 10, 23, 11, PAL["metal_d"])
    # frost / ice crystals
    set_px(c, 11, 9, PAL["white"])
    set_px(c, 12, 10, PAL["white"])
    set_px(c, 15, 13, PAL["white"])
    set_px(c, 17, 9, PAL["white"])
    set_px(c, 13, 18, PAL["white"])
    set_px(c, 16, 22, PAL["white"])
    # top vent
    fill_rect(c, 8, 5, 23, 5, PAL["freezer_blue_d"])
    # feet
    fill_rect(c, 5, 30, 7, 31, PAL["metal_d"])
    fill_rect(c, 24, 30, 26, 31, PAL["metal_d"])
    return to_image(c)

def prop_gas_canister():
    c = new_canvas(32, 32)
    # body (cylinder)
    fill_rect(c, 10, 8, 21, 28, PAL["gas_red"])
    fill_rect(c, 10, 8, 12, 28, PAL["gas_red_d"])      # left shade
    fill_rect(c, 19, 8, 21, 28, PAL["gas_red_d"])      # right shade
    # top cap
    fill_rect(c, 12, 5, 19, 8, PAL["gas_cap"])
    fill_rect(c, 13, 4, 18, 5, PAL["gas_cap"])
    # valve / nozzle
    fill_rect(c, 14, 3, 16, 5, PAL["silver_d"])
    fill_rect(c, 19, 9, 23, 11, PAL["silver_d"])       # side nozzle
    # label band
    fill_rect(c, 10, 14, 21, 18, PAL["white"])
    set_px(c, 13, 16, PAL["black"])
    set_px(c, 14, 16, PAL["black"])
    set_px(c, 17, 16, PAL["black"])
    set_px(c, 18, 16, PAL["black"])
    # base
    fill_rect(c, 9, 28, 22, 30, PAL["gas_red_d"])
    fill_rect(c, 9, 30, 22, 31, PAL["black"])
    return to_image(c)

def prop_billboard():
    c = new_canvas(32, 32)
    # posts
    fill_rect(c, 7, 18, 9, 31, PAL["bill_post"])
    fill_rect(c, 22, 18, 24, 31, PAL["bill_post"])
    # panel frame
    fill_rect(c, 3, 2, 28, 18, PAL["bill_bg"])
    fill_rect(c, 3, 2, 28, 3, PAL["metal_d"])
    fill_rect(c, 3, 17, 28, 18, PAL["metal_d"])
    fill_rect(c, 3, 2, 4, 18, PAL["metal_d"])
    fill_rect(c, 27, 2, 28, 18, PAL["metal_d"])
    # neon art: magenta + cyan + yellow shapes
    fill_rect(c, 6, 5, 13, 9, PAL["bill_magenta"])
    fill_rect(c, 16, 5, 25, 9, PAL["bill_cyan"])
    fill_rect(c, 6, 11, 25, 15, PAL["bill_yellow"])
    # text-like dots
    set_px(c, 8, 12, PAL["black"])
    set_px(c, 10, 13, PAL["black"])
    set_px(c, 12, 12, PAL["black"])
    set_px(c, 14, 13, PAL["black"])
    set_px(c, 18, 12, PAL["black"])
    set_px(c, 20, 13, PAL["black"])
    set_px(c, 22, 12, PAL["black"])
    return to_image(c)

def prop_trash_bin():
    c = new_canvas(32, 32)
    # lid
    fill_rect(c, 6, 8, 25, 10, PAL["trash_lid"])
    fill_rect(c, 6, 8, 25, 9, PAL["trash_green_d"])
    # handle
    fill_rect(c, 14, 7, 17, 8, PAL["trash_green_d"])
    # body
    fill_rect(c, 7, 10, 24, 30, PAL["trash_green"])
    fill_rect(c, 7, 10, 9, 30, PAL["trash_green_d"])
    fill_rect(c, 22, 10, 24, 30, PAL["trash_green_d"])
    # ridges
    for y in range(12, 30, 3):
        fill_rect(c, 7, y, 24, y, PAL["trash_green_d"])
    # dents / stains
    set_px(c, 12, 16, PAL["trash_green_d"])
    set_px(c, 13, 17, PAL["trash_green_d"])
    set_px(c, 18, 22, PAL["trash_green_d"])
    # a bit of trash poking out
    set_px(c, 10, 8, PAL["cust_yellow"])
    set_px(c, 20, 8, PAL["bill_cyan"])
    # base shadow
    fill_rect(c, 7, 30, 24, 31, PAL["black"])
    return to_image(c)


# ---------------------------------------------------------------------------
# UI (16x16)
# ---------------------------------------------------------------------------
def ui_money():
    """Gold coin with $ symbol, beveled rim and shine highlight."""
    c = new_canvas(16, 16)
    gold   = PAL["gold"]
    gold_d = PAL["gold_d"]
    gold_l = (255, 224, 120, 255)  # bright highlight
    dark   = PAL["black"]
    # outer rim (dark)
    disc = [
        # y: x-range (inclusive)
        (3, [7, 8]),
        (2, [5, 6, 7, 8, 9, 10]),
        (1, [4, 5, 10, 11]),
        (0, [6, 7, 8, 9]),
    ]
    # build disc by scanlines (symmetric top/bottom)
    def disc_rows():
        rows = {
            2:  [4, 5, 6, 7, 8, 9, 10, 11],
            3:  [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
            4:  [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
            5:  [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13],
            6:  [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13],
            7:  [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13],
            8:  [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13],
            9:  [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13],
            10: [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
            11: [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
            12: [4, 5, 6, 7, 8, 9, 10, 11],
            13: [6, 7, 8, 9],
        }
        return rows
    rows = disc_rows()
    # base fill
    for y, xs in rows.items():
        for x in xs:
            c[y][x] = gold
    # outer rim (darker) on the edge pixels
    for y, xs in rows.items():
        if not xs:
            continue
        c[y][xs[0]] = gold_d
        c[y][xs[-1]] = gold_d
    # top row + bottom row full rim
    for x in rows[2]:
        c[2][x] = gold_d if x in (rows[2][0], rows[2][-1]) else gold
    for x in rows[12]:
        c[12][x] = gold_d if x in (rows[12][0], rows[12][-1]) else gold
    c[3][rows[3][0]] = gold_d; c[3][rows[3][-1]] = gold_d
    c[11][rows[11][0]] = gold_d; c[11][rows[11][-1]] = gold_d
    # inner rim ring (slightly darker)
    for y, xs in rows.items():
        if len(xs) >= 4:
            c[y][xs[1]] = gold_d
            c[y][xs[-2]] = gold_d
    # shine highlight (top-left)
    set_px(c, 4, 4, gold_l)
    set_px(c, 5, 3, gold_l)
    set_px(c, 6, 3, gold_l)
    set_px(c, 4, 5, gold_l)
    # $ symbol in the center (darker)
    sd = gold_d
    # S curve
    set_px(c, 6, 5, sd);  set_px(c, 7, 5, sd);  set_px(c, 8, 5, sd)
    set_px(c, 5, 6, sd)
    set_px(c, 6, 7, sd);  set_px(c, 7, 7, sd)
    set_px(c, 8, 8, sd)
    set_px(c, 6, 9, sd);  set_px(c, 7, 9, sd);  set_px(c, 8, 9, sd)
    # vertical stroke through S
    set_px(c, 7, 4, sd)
    set_px(c, 7, 10, sd)
    return to_image(c)


def ui_cover():
    """Green shield with metallic rim, cross emblem and highlight."""
    c = new_canvas(16, 16)
    g   = PAL["shield_green"]
    gd  = PAL["shield_green_d"]
    gl  = (140, 232, 160, 255)   # light green highlight
    wht = PAL["white"]
    blk = PAL["black"]
    # shield outline shape (per-row x-ranges, inclusive)
    shape = {
        1:  [7, 8, 9],
        2:  [5, 6, 7, 8, 9, 10],
        3:  [4, 5, 6, 7, 8, 9, 10, 11],
        4:  [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
        5:  [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
        6:  [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
        7:  [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
        8:  [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
        9:  [4, 5, 6, 7, 8, 9, 10, 11],
        10: [4, 5, 6, 7, 8, 9, 10, 11],
        11: [5, 6, 7, 8, 9, 10],
        12: [5, 6, 7, 8, 9, 10],
        13: [6, 7, 8, 9],
        14: [7, 8],
    }
    # base fill
    for y, xs in shape.items():
        for x in xs:
            c[y][x] = g
    # outer outline (black)
    for y, xs in shape.items():
        if not xs:
            continue
        c[y][xs[0]] = blk
        c[y][xs[-1]] = blk
    # top + bottom rim
    for x in shape[1]:
        c[1][x] = blk
    for x in shape[14]:
        c[14][x] = blk
    # right-side shading (darker green)
    for y, xs in shape.items():
        if len(xs) >= 3:
            c[y][xs[-2]] = gd
            c[y][xs[-3]] = gd
    # left-side highlight (light green)
    for y, xs in shape.items():
        if len(xs) >= 4:
            c[y][xs[1]] = gl
    # top highlight band
    for x in shape[3]:
        if x not in (shape[3][0], shape[3][-1]):
            c[3][x] = gl
    # metallic rim studs along edge (gold dots)
    set_px(c, 7, 1, PAL["gold"])
    set_px(c, 8, 1, PAL["gold"])
    set_px(c, 4, 4, PAL["gold"])
    set_px(c, 11, 4, PAL["gold"])
    set_px(c, 3, 7, PAL["gold"])
    set_px(c, 12, 7, PAL["gold"])
    # cross emblem (white) in center
    # vertical bar
    for y in range(4, 11):
        set_px(c, 7, y, wht)
        set_px(c, 8, y, wht)
    # horizontal bar
    for x in range(5, 10):
        set_px(c, x, 6, wht)
        set_px(c, x, 7, wht)
    # cross shadow (right edge)
    for y in range(4, 11):
        set_px(c, 8, y, PAL["white_shadow"])
    for x in range(5, 10):
        set_px(c, x, 7, PAL["white_shadow"])
    # shadow under shield
    fill_rect(c, 5, 15, 10, 15, (20, 16, 24, 90))
    return to_image(c)


def ui_time():
    """Clock with rim, tick marks and visible hour/minute hands."""
    c = new_canvas(16, 16)
    rim   = PAL["clock_rim"]
    face  = PAL["clock_face"]
    face_d= (200, 198, 205, 255)   # face shadow
    blk   = PAL["black"]
    red   = (210, 60, 20, 255)     # second hand
    # circle outline (per-row x-ranges)
    circ = {
        1:  [6, 7, 8, 9],
        2:  [4, 5, 10, 11],
        3:  [3, 12],
        4:  [2, 13],
        5:  [2, 13],
        6:  [2, 13],
        7:  [2, 13],
        8:  [2, 13],
        9:  [2, 13],
        10: [2, 13],
        11: [2, 13],
        12: [3, 12],
        13: [4, 5, 10, 11],
        14: [6, 7, 8, 9],
    }
    # rim pixels
    for y, xs in circ.items():
        if not xs:
            continue
        c[y][xs[0]] = rim
        c[y][xs[-1]] = rim
    # top + bottom rim
    for x in circ[1]:
        c[1][x] = rim
    for x in circ[14]:
        c[14][x] = rim
    # inner face fill
    for y in range(2, 14):
        for x in range(2, 14):
            if c[y][x] is None:
                c[y][x] = face
    # face shadow (right + bottom)
    for y in range(2, 14):
        c[y][12] = face_d if c[y][12] != rim else rim
        c[y][11] = face_d if c[y][11] != rim else rim
    for x in range(2, 14):
        c[12][x] = face_d if c[12][x] != rim else rim
    # tick marks (12, 3, 6, 9 o'clock) - bold
    for (x, y) in [(7, 2), (8, 2), (7, 13), (8, 13),
                   (2, 7), (2, 8), (13, 7), (13, 8)]:
        c[y][x] = blk
    # minor ticks (1,2,4,5,7,8,10,11)
    for (x, y) in [(5, 3), (10, 3), (3, 5), (12, 5),
                   (3, 10), (12, 10), (5, 12), (10, 12)]:
        c[y][x] = blk
    # hour hand (pointing to ~10 o'clock - short, dark)
    set_px(c, 7, 7, blk)
    set_px(c, 6, 6, blk)
    set_px(c, 5, 5, blk)
    set_px(c, 4, 4, blk)
    # minute hand (pointing to ~2 o'clock - long)
    set_px(c, 8, 8, blk)
    set_px(c, 9, 7, blk)
    set_px(c, 10, 6, blk)
    set_px(c, 11, 5, blk)
    # second hand (red, thin, pointing to 12)
    set_px(c, 8, 7, red)
    set_px(c, 8, 6, red)
    set_px(c, 8, 5, red)
    set_px(c, 8, 4, red)
    # center pivot (black dot with silver ring)
    set_px(c, 7, 8, blk)
    set_px(c, 8, 8, blk)
    set_px(c, 7, 7, blk)
    set_px(c, 8, 7, blk)
    set_px(c, 7, 8, PAL["silver_d"])
    set_px(c, 8, 8, PAL["silver_d"])
    # shine highlight on glass (top-left)
    set_px(c, 4, 3, PAL["white"])
    set_px(c, 5, 4, (220, 220, 226, 255))
    # shadow under clock
    fill_rect(c, 4, 15, 11, 15, (20, 16, 24, 90))
    return to_image(c)

def ui_order_bubble():
    """Order bubble: 32x24 white frame with a tail at bottom-left.

    The interior is left empty so game code can composite food/recipe icons
    on top at runtime.
    """
    c = new_canvas(32, 24)
    rim = PAL["clock_rim"]
    body = PAL["bubble"]
    body_d = PAL["bubble_tail"]
    # ---- rounded bubble body (x:1..30, y:1..17) ----
    # main fill
    fill_rect(c, 3, 1, 28, 17, body)
    fill_rect(c, 1, 3, 30, 15, body)
    # corner rounding (transparent notches)
    set_px(c, 1, 1, None); set_px(c, 2, 1, None)
    set_px(c, 1, 2, None)
    set_px(c, 29, 1, None); set_px(c, 30, 1, None)
    set_px(c, 30, 2, None)
    set_px(c, 1, 17, None); set_px(c, 1, 16, None)
    set_px(c, 2, 17, None)
    set_px(c, 29, 17, None); set_px(c, 30, 17, None)
    set_px(c, 30, 16, None)

    # ---- outline (dark rim) ----
    # top edge
    for x in range(3, 29):
        set_px(c, x, 0, rim)
    # bottom edge (above tail)
    for x in range(3, 29):
        set_px(c, x, 18, rim)
    # left edge
    for y in range(3, 16):
        set_px(c, 0, y, rim)
    # right edge
    for y in range(3, 16):
        set_px(c, 31, y, rim)
    # rounded corners
    set_px(c, 2, 1, rim); set_px(c, 1, 2, rim)
    set_px(c, 29, 1, rim); set_px(c, 30, 2, rim)
    set_px(c, 2, 17, rim); set_px(c, 1, 16, rim)
    set_px(c, 29, 17, rim); set_px(c, 30, 16, rim)

    # ---- soft inner shadow (bottom + right) ----
    for x in range(3, 29):
        set_px(c, x, 17, body_d)
    for y in range(3, 16):
        set_px(c, 30, y, body_d)

    # ---- tail at bottom-left (pointing down-left) ----
    # triangular tail: rows from y=18..22, narrowing toward the tip
    # row 18 (base): x 4..8 (already partly rim)
    set_px(c, 4, 18, rim); set_px(c, 5, 18, body)
    set_px(c, 6, 18, body); set_px(c, 7, 18, body); set_px(c, 8, 18, rim)
    # row 19
    set_px(c, 4, 19, rim); set_px(c, 5, 19, body)
    set_px(c, 6, 19, body); set_px(c, 7, 19, rim)
    # row 20
    set_px(c, 3, 20, rim); set_px(c, 4, 20, body)
    set_px(c, 5, 20, body); set_px(c, 6, 20, rim)
    # row 21 (near tip)
    set_px(c, 3, 21, rim); set_px(c, 4, 21, body); set_px(c, 5, 21, rim)
    # row 22 (tip)
    set_px(c, 3, 22, rim); set_px(c, 4, 22, rim)

    # ---- "order" hint: 3 small dots along the top, inside ----
    # (kept subtle so food icons remain the focus)
    set_px(c, 14, 3, body_d)
    set_px(c, 16, 3, body_d)
    set_px(c, 18, 3, body_d)

    return to_image(c)

def ui_interact():
    # yellow E
    c = new_canvas(16, 16)
    # round background (dark) so E pops
    fill_rect(c, 3, 3, 12, 12, (30, 26, 36, 255))
    fill_rect(c, 4, 2, 11, 13, (30, 26, 36, 255))
    fill_rect(c, 2, 4, 13, 11, (30, 26, 36, 255))
    # E shape (yellow), 7 wide x 9 tall, centered at x=4..10, y=3..11
    ex, ey = 4, 3
    # vertical bar
    fill_rect(c, ex, ey, ex, ey + 8, PAL["e_yellow"])
    # top bar
    fill_rect(c, ex + 1, ey, ex + 6, ey, PAL["e_yellow"])
    # middle bar
    fill_rect(c, ex + 1, ey + 4, ex + 5, ey + 4, PAL["e_yellow"])
    # bottom bar
    fill_rect(c, ex + 1, ey + 8, ex + 6, ey + 8, PAL["e_yellow"])
    # shadow
    fill_rect(c, ex, ey + 8, ex + 6, ey + 8, PAL["e_yellow_d"])
    return to_image(c)


# ---------------------------------------------------------------------------
# Ingredients (16x16)  -- M1 cooking-loop food items
# ---------------------------------------------------------------------------
def ingredient_bread():
    """Top bun, sesame seeds, brown crust."""
    c = new_canvas(16, 16)
    # bottom flat face of the bun (slice view)
    fill_rect(c, 2, 9, 13, 13, PAL["bread_inner"])
    fill_rect(c, 2, 13, 13, 13, PAL["bread_inner_d"])
    fill_rect(c, 2, 9, 13, 9, PAL["bread_crust_d"])
    fill_rect(c, 2, 9, 2, 13, PAL["bread_inner_d"])
    fill_rect(c, 13, 9, 13, 13, PAL["bread_inner_d"])
    # dome top (crust)
    fill_rect(c, 4, 5, 11, 8, PAL["bread_crust"])
    fill_rect(c, 3, 6, 12, 8, PAL["bread_crust"])
    fill_rect(c, 2, 7, 13, 8, PAL["bread_crust"])
    # crust shadow line
    fill_rect(c, 2, 8, 13, 8, PAL["bread_crust_d"])
    # highlight on dome
    set_px(c, 5, 6, PAL["sesame"])
    set_px(c, 8, 5, PAL["sesame"])
    set_px(c, 10, 6, PAL["sesame"])
    # sesame seeds on top
    set_px(c, 5, 7, PAL["sesame"])
    set_px(c, 7, 6, PAL["sesame"])
    set_px(c, 9, 7, PAL["sesame"])
    set_px(c, 11, 7, PAL["sesame"])
    set_px(c, 4, 7, PAL["sesame"])
    # outline pixels
    set_px(c, 4, 4, PAL["bread_crust_d"])
    set_px(c, 11, 4, PAL["bread_crust_d"])
    set_px(c, 3, 5, PAL["bread_crust_d"])
    set_px(c, 12, 5, PAL["bread_crust_d"])
    # shadow under bun
    fill_rect(c, 2, 14, 13, 14, (20, 16, 24, 90))
    return to_image(c)


def ingredient_patty():
    """Raw/cooked meat patty, slightly irregular circle."""
    c = new_canvas(16, 16)
    # patty body (rounded)
    fill_rect(c, 3, 6, 12, 11, PAL["patty"])
    fill_rect(c, 4, 5, 11, 12, PAL["patty"])
    fill_rect(c, 2, 7, 13, 10, PAL["patty"])
    # darker bottom (cooked side)
    fill_rect(c, 3, 11, 12, 12, PAL["patty_d"])
    fill_rect(c, 4, 12, 11, 12, PAL["patty_d"])
    fill_rect(c, 2, 10, 13, 10, PAL["patty_d"])
    # top highlight (juicy)
    set_px(c, 5, 6, PAL["patty_l"])
    set_px(c, 6, 5, PAL["patty_l"])
    set_px(c, 9, 5, PAL["patty_l"])
    set_px(c, 10, 6, PAL["patty_l"])
    set_px(c, 7, 6, PAL["patty_l"])
    # texture specks (grill marks / fat)
    set_px(c, 5, 8, PAL["patty_d"])
    set_px(c, 8, 9, PAL["patty_d"])
    set_px(c, 10, 8, PAL["patty_d"])
    set_px(c, 6, 10, PAL["patty_l"])
    set_px(c, 9, 11, PAL["patty_l"])
    # crust edge
    set_px(c, 3, 7, PAL["patty_d"])
    set_px(c, 12, 7, PAL["patty_d"])
    set_px(c, 3, 9, PAL["patty_d"])
    set_px(c, 12, 9, PAL["patty_d"])
    # outline
    set_px(c, 4, 4, PAL["patty_d"])
    set_px(c, 11, 4, PAL["patty_d"])
    set_px(c, 4, 13, PAL["patty_d"])
    set_px(c, 11, 13, PAL["patty_d"])
    set_px(c, 1, 8, PAL["patty_d"])
    set_px(c, 14, 8, PAL["patty_d"])
    # shadow
    fill_rect(c, 2, 14, 13, 14, (20, 16, 24, 90))
    return to_image(c)


def ingredient_lettuce():
    """Leaf of lettuce with ruffled edge and veins."""
    c = new_canvas(16, 16)
    # main leaf body
    fill_rect(c, 3, 5, 12, 11, PAL["lettuce"])
    fill_rect(c, 2, 6, 13, 10, PAL["lettuce"])
    fill_rect(c, 4, 4, 11, 12, PAL["lettuce"])
    # ruffled edge (darker bumps around perimeter)
    set_px(c, 3, 5, PAL["lettuce_d"])
    set_px(c, 12, 5, PAL["lettuce_d"])
    set_px(c, 2, 7, PAL["lettuce_d"])
    set_px(c, 13, 7, PAL["lettuce_d"])
    set_px(c, 2, 9, PAL["lettuce_d"])
    set_px(c, 13, 9, PAL["lettuce_d"])
    set_px(c, 3, 11, PAL["lettuce_d"])
    set_px(c, 12, 11, PAL["lettuce_d"])
    set_px(c, 4, 12, PAL["lettuce_d"])
    set_px(c, 11, 12, PAL["lettuce_d"])
    set_px(c, 4, 4, PAL["lettuce_d"])
    set_px(c, 11, 4, PAL["lettuce_d"])
    # edge bumps (light highlights on top of ruffles)
    set_px(c, 3, 4, PAL["lettuce_l"])
    set_px(c, 12, 4, PAL["lettuce_l"])
    set_px(c, 1, 8, PAL["lettuce_l"])
    set_px(c, 14, 8, PAL["lettuce_l"])
    set_px(c, 4, 13, PAL["lettuce_l"])
    set_px(c, 11, 13, PAL["lettuce_l"])
    # central vein
    fill_rect(c, 7, 5, 8, 11, PAL["lettuce_l"])
    set_px(c, 7, 4, PAL["lettuce_l"])
    set_px(c, 8, 12, PAL["lettuce_l"])
    # side veins
    set_px(c, 5, 7, PAL["lettuce_l"])
    set_px(c, 5, 9, PAL["lettuce_l"])
    set_px(c, 10, 6, PAL["lettuce_l"])
    set_px(c, 10, 10, PAL["lettuce_l"])
    set_px(c, 6, 6, PAL["lettuce_d"])
    set_px(c, 9, 10, PAL["lettuce_d"])
    # shadow
    fill_rect(c, 2, 14, 13, 14, (20, 16, 24, 90))
    return to_image(c)


def ingredient_burger_assembled():
    """Full burger: bun top / lettuce / patty / cheese / bun bottom."""
    c = new_canvas(16, 16)
    # ---- bottom bun ----
    fill_rect(c, 3, 12, 12, 13, PAL["bread_crust"])
    fill_rect(c, 3, 13, 12, 13, PAL["bread_crust_d"])
    fill_rect(c, 4, 11, 11, 12, PAL["bread_inner"])
    # ---- cheese slice (melting down sides) ----
    fill_rect(c, 3, 10, 12, 11, PAL["cheese"])
    fill_rect(c, 3, 11, 3, 11, PAL["cheese_d"])
    fill_rect(c, 12, 11, 12, 11, PAL["cheese_d"])
    set_px(c, 2, 11, PAL["cheese"])
    set_px(c, 13, 11, PAL["cheese"])
    set_px(c, 4, 10, PAL["cheese_d"])
    set_px(c, 8, 10, PAL["cheese_d"])
    # ---- patty ----
    fill_rect(c, 3, 8, 12, 9, PAL["patty"])
    fill_rect(c, 3, 9, 12, 9, PAL["patty_d"])
    set_px(c, 5, 8, PAL["patty_l"])
    set_px(c, 9, 8, PAL["patty_l"])
    set_px(c, 2, 8, PAL["patty_d"])
    set_px(c, 13, 8, PAL["patty_d"])
    # ---- lettuce (ruffled, sticking out sides) ----
    fill_rect(c, 3, 7, 12, 7, PAL["lettuce"])
    set_px(c, 2, 7, PAL["lettuce"])
    set_px(c, 13, 7, PAL["lettuce"])
    set_px(c, 1, 7, PAL["lettuce_l"])
    set_px(c, 14, 7, PAL["lettuce_l"])
    set_px(c, 4, 7, PAL["lettuce_l"])
    set_px(c, 8, 7, PAL["lettuce_l"])
    set_px(c, 11, 7, PAL["lettuce_l"])
    set_px(c, 6, 7, PAL["lettuce_d"])
    set_px(c, 10, 7, PAL["lettuce_d"])
    # ---- top bun ----
    fill_rect(c, 4, 4, 11, 6, PAL["bread_crust"])
    fill_rect(c, 3, 5, 12, 6, PAL["bread_crust"])
    fill_rect(c, 2, 6, 13, 6, PAL["bread_crust"])
    fill_rect(c, 2, 6, 13, 6, PAL["bread_crust_d"])  # shadow line under bun
    # dome highlight
    set_px(c, 5, 5, PAL["sesame"])
    set_px(c, 8, 4, PAL["sesame"])
    set_px(c, 10, 5, PAL["sesame"])
    # sesame seeds
    set_px(c, 5, 5, PAL["sesame"])
    set_px(c, 7, 4, PAL["sesame"])
    set_px(c, 9, 5, PAL["sesame"])
    set_px(c, 11, 5, PAL["sesame"])
    set_px(c, 4, 6, PAL["sesame"])
    # outline top
    set_px(c, 4, 3, PAL["bread_crust_d"])
    set_px(c, 11, 3, PAL["bread_crust_d"])
    set_px(c, 3, 4, PAL["bread_crust_d"])
    set_px(c, 12, 4, PAL["bread_crust_d"])
    # shadow under burger
    fill_rect(c, 2, 14, 13, 14, (20, 16, 24, 90))
    return to_image(c)


# ---------------------------------------------------------------------------
# FX (32x32)
# ---------------------------------------------------------------------------
def fx_blood_splash():
    c = new_canvas(32, 32)
    # central pool
    fill_rect(c, 13, 14, 18, 18, PAL["blood"])
    fill_rect(c, 12, 15, 19, 17, PAL["blood"])
    set_px(c, 15, 13, PAL["blood"]); set_px(c, 16, 13, PAL["blood"])
    set_px(c, 15, 19, PAL["blood"]); set_px(c, 16, 19, PAL["blood"])
    # bright highlight
    set_px(c, 14, 15, PAL["blood_bright"])
    set_px(c, 15, 14, PAL["blood_bright"])
    # droplets radiating
    droplets = [
        (4, 6), (7, 3), (11, 5), (3, 12), (5, 20), (8, 24),
        (13, 26), (18, 27), (23, 24), (26, 20), (28, 13),
        (25, 7), (21, 4), (16, 2), (24, 11), (10, 22),
        (20, 21), (6, 16),
    ]
    for (x, y) in droplets:
        set_px(c, x, y, PAL["blood"])
    # small speckles (darker)
    speckles = [(6, 9), (9, 7), (12, 8), (4, 15), (7, 23), (14, 28),
                (22, 28), (27, 17), (26, 10), (19, 6)]
    for (x, y) in speckles:
        set_px(c, x, y, PAL["blood_d"])
    # elongated splashes
    fill_rect(c, 6, 13, 9, 13, PAL["blood_d"])
    fill_rect(c, 22, 18, 25, 18, PAL["blood_d"])
    fill_rect(c, 15, 22, 15, 25, PAL["blood_d"])
    return to_image(c)

def fx_explosion():
    c = new_canvas(32, 32)
    # outer ring
    ring = [
        (16, 5), (15, 6), (17, 6), (14, 7), (18, 7),
        (12, 9), (19, 9), (11, 11), (20, 11), (10, 14),
        (21, 14), (10, 17), (21, 17), (11, 20), (20, 20),
        (12, 22), (19, 22), (14, 24), (18, 24), (15, 25),
        (17, 25), (16, 26),
    ]
    for (x, y) in ring:
        set_px(c, x, y, PAL["expl_outer"])
    # spikes
    spikes = [
        (16, 3), (16, 4), (3, 16), (4, 16), (27, 16), (28, 16),
        (16, 27), (16, 28), (8, 8), (24, 8), (8, 24), (24, 24),
        (7, 12), (7, 13), (24, 12), (24, 13),
    ]
    for (x, y) in spikes:
        set_px(c, x, y, PAL["expl_outer"])
    # mid layer
    mid = []
    for y in range(8, 24):
        for x in range(8, 24):
            d = abs(x - 16) + abs(y - 16)
            if d <= 7:
                mid.append((x, y))
    for (x, y) in mid:
        set_px(c, x, y, PAL["expl_mid"])
    # inner core
    core = []
    for y in range(11, 21):
        for x in range(11, 21):
            d = abs(x - 16) + abs(y - 16)
            if d <= 4:
                core.append((x, y))
    for (x, y) in core:
        set_px(c, x, y, PAL["expl_core"])
    # bright center
    fill_rect(c, 14, 14, 18, 18, PAL["expl_core"])
    set_px(c, 15, 15, (255, 255, 255, 255))
    set_px(c, 16, 15, (255, 255, 255, 255))
    # smoke puffs at edges
    for (x, y) in [(5, 5), (26, 6), (5, 26), (27, 25)]:
        set_px(c, x, y, PAL["expl_smoke"])
    return to_image(c)

def fx_smoke():
    c = new_canvas(32, 32)
    # cluster of overlapping puffs
    puffs = [
        (8, 18, 6),   # center x, center y, radius
        (16, 14, 7),
        (22, 19, 5),
        (12, 22, 4),
        (20, 24, 4),
        (16, 9, 4),
    ]
    for (cx, cy, r) in puffs:
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                d = (x - cx) ** 2 + (y - cy) ** 2
                if d <= r * r:
                    if c[y][x] is None:
                        c[y][x] = PAL["smoke_gray"]
    # darker bottoms
    for (cx, cy, r) in puffs:
        for x in range(cx - r + 1, cx + r):
            y = cy + r - 1
            if 0 <= y < 32 and c[y][x] is None:
                c[y][x] = PAL["smoke_dark"]
    # highlights on top
    for (cx, cy, r) in puffs:
        for x in range(cx - 1, cx + 2):
            y = cy - r + 1
            if 0 <= y < 32 and 0 <= x < 32:
                c[y][x] = PAL["smoke_light"]
    # wisps rising
    set_px(c, 16, 5, PAL["smoke_gray"])
    set_px(c, 15, 4, PAL["smoke_gray"])
    set_px(c, 17, 4, PAL["smoke_gray"])
    set_px(c, 16, 3, PAL["smoke_light"])
    return to_image(c)

def fx_poison():
    c = new_canvas(32, 32)
    # bubbles of varying size
    bubbles = [
        (8, 20, 4), (16, 22, 5), (22, 19, 4),
        (12, 15, 3), (19, 14, 3), (15, 10, 4),
        (23, 12, 2), (7, 13, 2),
    ]
    for (cx, cy, r) in bubbles:
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                d = (x - cx) ** 2 + (y - cy) ** 2
                if d <= r * r:
                    if c[y][x] is None:
                        c[y][x] = PAL["poison"]
    # darker rim
    for (cx, cy, r) in bubbles:
        for ang_x in range(-r, r + 1):
            y = cy + r
            x = cx + ang_x
            if 0 <= x < 32 and 0 <= y < 32:
                if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                    c[y][x] = PAL["poison_d"]
        y = cy - r
        if 0 <= y < 32:
            for x in range(cx - r, cx + r + 1):
                if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                    c[y][x] = PAL["poison_d"]
    # highlights
    for (cx, cy, r) in bubbles:
        if r >= 3:
            set_px(c, cx - 1, cy - r + 1, PAL["poison_light"])
            set_px(c, cx, cy - r + 1, PAL["poison_light"])
    # small floating dots
    for (x, y) in [(5, 8), (26, 7), (28, 16), (4, 17), (25, 25), (6, 25)]:
        set_px(c, x, y, PAL["poison"])
    for (x, y) in [(11, 6), (20, 5), (13, 26)]:
        set_px(c, x, y, PAL["poison_light"])
    return to_image(c)


# ---------------------------------------------------------------------------
# Output registry
# ---------------------------------------------------------------------------
def save(img, *subpaths):
    path = os.path.join(ART_ROOT, *subpaths)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    # PNG is lossless; no resample applied -> effectively point/nearest-neighbor.
    img.save(path, format="PNG", optimize=False, compress_level=0)
    return path

def build_all():
    files = []

    # ---- Characters / Player ----
    for direction in ("down", "up", "left", "right"):
        for frame in (1, 2):
            files.append(save(
                player_walk(direction, frame),
                "Characters", "Player",
                f"player_walk_{direction}_{frame:02d}.png"))
    files.append(save(player_idle(),  "Characters", "Player", "player_idle.png"))
    files.append(save(player_cook(),  "Characters", "Player", "player_cook.png"))
    files.append(save(player_attack(),"Characters", "Player", "player_attack.png"))
    files.append(save(player_carry(), "Characters", "Player", "player_carry.png"))

    # ---- Characters / Customers ----
    for v in (1, 2, 3):
        files.append(save(customer_idle(v),
                          "Characters", "Customers", f"customer_{v:02d}.png"))
        for frame in (1, 2, 3, 4):
            files.append(save(customer_walk(v, frame),
                              "Characters", "Customers",
                              f"customer_{v:02d}_walk_{frame:02d}.png"))

    # ---- Characters / Enemies ----
    files.append(save(enemy_police(),   "Characters", "Enemies", "police.png"))
    files.append(save(enemy_inspector(), "Characters", "Enemies", "inspector.png"))

    # ---- Tilesets (16x16) ----
    files.append(save(tile_street(),      "Tilesets", "street.png"))
    files.append(save(tile_sidewalk(),    "Tilesets", "sidewalk.png"))
    files.append(save(tile_wall(),        "Tilesets", "wall.png"))
    files.append(save(tile_alley(),       "Tilesets", "alley.png"))
    files.append(save(tile_truck_floor(), "Tilesets", "truck_floor.png"))
    # M1: combined horizontal sheet for Unity Tilemap palette (80x16)
    files.append(save(tileset_day(),      "Tilesets", "tileset_day.png"))

    # ---- Props (32x32) ----
    files.append(save(prop_cooking_station(),                "Props", "cooking_station.png"))
    files.append(save(prop_cooking_station_cooking(1),       "Props", "cooking_station_cooking_01.png"))
    files.append(save(prop_cooking_station_cooking(2),       "Props", "cooking_station_cooking_02.png"))
    files.append(save(prop_grinder(),         "Props", "grinder.png"))
    files.append(save(prop_freezer(),         "Props", "freezer.png"))
    files.append(save(prop_gas_canister(),    "Props", "gas_canister.png"))
    files.append(save(prop_billboard(),       "Props", "billboard.png"))
    files.append(save(prop_trash_bin(),       "Props", "trash_bin.png"))

    # ---- UI ----
    files.append(save(ui_money(),       "UI", "icon_money.png"))
    files.append(save(ui_cover(),       "UI", "icon_cover.png"))
    files.append(save(ui_time(),        "UI", "icon_time.png"))
    files.append(save(ui_order_bubble(),"UI", "order_bubble.png"))
    files.append(save(ui_interact(),    "UI", "icon_interact.png"))

    # ---- Ingredients (16x16) -- M1 cooking-loop food items ----
    files.append(save(ingredient_bread(),           "UI", "ingredient_bread.png"))
    files.append(save(ingredient_patty(),           "UI", "ingredient_patty.png"))
    files.append(save(ingredient_lettuce(),         "UI", "ingredient_lettuce.png"))
    files.append(save(ingredient_burger_assembled(),"UI", "ingredient_burger_assembled.png"))

    # ---- FX (32x32) ----
    files.append(save(fx_blood_splash(), "FX", "blood_splash.png"))
    files.append(save(fx_explosion(),    "FX", "explosion.png"))
    files.append(save(fx_smoke(),        "FX", "smoke.png"))
    files.append(save(fx_poison(),       "FX", "poison.png"))

    return files


def main():
    os.makedirs(ART_ROOT, exist_ok=True)
    files = build_all()
    print(f"Generated {len(files)} files under {ART_ROOT}")
    for f in files:
        rel = os.path.relpath(f, PROJECT_ROOT)
        print(f"  - {rel}")
    return files


if __name__ == "__main__":
    main()
