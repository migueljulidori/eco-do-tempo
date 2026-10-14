#!/usr/bin/env python3
"""Desenha cada fase por cima do fundo, como o jogo posiciona, sem abrir a Unity.

Uso: python3 tools/render-levels.py [pasta_de_saida] [--esquema]
Gera fase1_passado.png, fase1_futuro.png... (padrão: Previas/). Precisa do Pillow.
Por padrão desenha com a arte do jogo; --esquema desenha caixas coloridas e rótulos.
Serve para conferir se buracos, chão e paredes do mapa batem com o desenho do fundo.

O que só existe na outra era aparece tracejado; o que existe na era da imagem, cheio.
"""
import pathlib
import re
import sys

try:
    from PIL import Image, ImageDraw
except ImportError:
    sys.exit("Instale o Pillow primeiro: pip install pillow")

ROOT = pathlib.Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Assets/Scripts/Level/LevelData.cs"
BACKGROUNDS = ROOT / "Assets/Resources/Backgrounds"
PX = 32            # pixels per map cell in the preview
Y_ALIGN = 0.12     # LevelBuilder.SceneYAlign
HALF_H, ASPECT = 6.0, 16 / 9   # camera used by LevelBackground
WORKSHOP = {"oficina", "coracao"}

PAST, FUTURE = "past", "future"
STYLE = {
    "#": ("both", (200, 200, 200)),
    "p": (PAST, (240, 160, 70)),
    "f": (FUTURE, (80, 220, 255)),
    "G": ("both", (220, 70, 70)), "a": ("both", (70, 230, 210)), "b": ("both", (200, 130, 255)),
    "S": ("both", (120, 80, 40)),
}
MARKS = {
    "@": ("both", (255, 255, 255), "Lia"), "D": ("both", (255, 220, 90), "porta"),
    "*": ("both", (255, 235, 110), "*"), "1": (PAST, (255, 200, 60), "1"), "2": (FUTURE, (110, 240, 255), "2"),
    "^": ("both", (255, 60, 60), "^"), "~": (PAST, (255, 120, 20), "~"),
    "c": ("both", (170, 190, 220), "caixa"), "h": ("both", (250, 240, 200), "relógio"),
    "i": ("both", (250, 250, 210), "i"),
    "L": ("both", (255, 190, 80), "L"), "A": ("both", (70, 230, 210), "A"), "B": ("both", (200, 130, 255), "B"),
    "x": ("both", (255, 190, 80), "_"), "y": ("both", (70, 230, 210), "_"), "z": ("both", (200, 130, 255), "_"),
    "m": ("both", (230, 180, 90), "m"), "M": ("both", (230, 180, 90), "M"), "v": ("both", (255, 80, 80), "lâmina"),
    "=": ("both", (255, 255, 255), "·"), "|": ("both", (255, 255, 255), "·"),
    "!": ("both", (255, 255, 255), "!"),
}


RES = ROOT / "Assets/Resources"
_cache = {}


def art(path):
    if path not in _cache:
        f = RES / f"{path}.png"
        try:
            img = Image.open(f).convert("RGBA") if f.exists() else None
        except OSError:
            img = None
        _cache[path] = img
    return _cache[path]


def first(*paths):
    for p in paths:
        if art(p) is not None:
            return p
    return paths[-1]


def first_frame(path, frame=32):
    img = art(path)
    return img.crop((0, 0, frame, img.height)) if img is not None and img.width > frame else img


def levels():
    text = SOURCE.read_text(encoding="utf-8")
    chunks = text[text.index("Levels ="):].split("new LevelDefinition")[1:]
    for chunk in chunks:
        name = re.search(r'Name = "([^"]*)"', chunk).group(1)
        bg = re.search(r'Background = "([^"]*)"', chunk)
        grid = re.findall(r'"([^"]*)"', re.search(r"Map = new\[\]\s*\{(.*?)\}", chunk, re.S).group(1))
        tiles = re.search(r'Tileset = "([^"]*)"', chunk)
        yield name, bg.group(1) if bg else None, grid, tiles.group(1) if tiles else ""


def load_background(key, era):
    for name in (f"{key}_{era}_1472x576", f"{key}_{era}", key):
        path = BACKGROUNDS / f"{name}.png"
        if path.exists():
            try:
                img = Image.open(path)
                img.load()
                return img.convert("RGB")
            except OSError:
                print(f"  aviso: {path.name} está corrompido ou incompleto")
    return None


def render(name, key, grid, era, tileset="", schematic=False):
    h, w = len(grid), len(grid[0])
    spawn_row = next(r for r, line in enumerate(grid) if "@" in line)
    floor_top = (h - 1 - spawn_row + Y_ALIGN) - 0.5

    # World window: the map plus some room under the floor.
    x0, x1 = -0.5, w - 0.5
    y0, y1 = min(-1.5, floor_top - 10), h - 0.5 + Y_ALIGN
    width, height = int((x1 - x0) * PX), int((y1 - y0) * PX)
    canvas = Image.new("RGB", (width, height), (12, 10, 9))

    def to_px(x, y):
        return (x - x0) * PX, (y1 - y) * PX

    bg = load_background(key, era) if key else None
    if bg is None and key:
        bg = load_background(key, FUTURE if era == PAST else PAST)
    if bg is not None:
        if key in WORKSHOP:
            cover_w, ledge = w, 490 / 1154
        else:
            cover_w, ledge = max(w, HALF_H * ASPECT * 2) + 2, 0.26
        scaled_h = cover_w * bg.height / bg.width
        cx, cy = (w - 1) * 0.5, floor_top - (ledge - 0.5) * scaled_h
        left, top = to_px(cx - cover_w / 2, cy + scaled_h / 2)
        img = bg.resize((max(1, int(cover_w * PX)), max(1, int(scaled_h * PX))), Image.NEAREST)
        canvas.paste(img, (int(round(left)), int(round(top))))

    if not schematic:
        return draw_art(canvas, grid, era, tileset, to_px, floor_top)

    over = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(over)
    for r, line in enumerate(grid):
        for c, ch in enumerate(line):
            cx, cy = c, h - 1 - r + Y_ALIGN
            l, t = to_px(cx - 0.5, cy + 0.5)
            box = (l, t, l + PX - 1, t + PX - 1)
            if ch in STYLE:
                when, color = STYLE[ch]
                here = when in ("both", era)
                if here:
                    d.rectangle(box, fill=color + (110,), outline=color + (255,), width=2)
                else:
                    for k in range(0, PX, 6):
                        d.line((l + k, t, l + k, t + 2), fill=color + (200,))
                        d.line((l + k, t + PX - 3, l + k, t + PX - 1), fill=color + (200,))
                    d.rectangle(box, outline=color + (120,), width=1)
            elif ch in MARKS:
                when, color, label = MARKS[ch]
                alpha = 255 if when in ("both", era) else 90
                d.text((l + 3, t + 9), label, fill=color + (alpha,))
    # Floor line the background was aligned to.
    _, fy = to_px(0, floor_top)
    d.line((0, fy, width, fy), fill=(255, 0, 0, 160), width=1)
    canvas = Image.alpha_composite(canvas.convert("RGBA"), over).convert("RGB")
    return canvas


SOLID_CHARS = set("#pfGS")


def draw_art(canvas, grid, era, tileset, to_px, floor_top):
    """Approximates LevelBuilder: same tile choices, sizes and anchors as the game."""
    h, w = len(grid), len(grid[0])
    canvas = canvas.convert("RGBA")
    garden = tileset == "garden"
    cloister = tileset == "cloister"
    plant_cols = {c for line in grid for c, ch in enumerate(line) if ch == "S"}

    def ch_at(r, c):
        if r < 0:
            return "#"
        if r >= h or c < 0 or c >= w:
            return "."
        return grid[r][c]

    def paste(path_or_img, cx, bottom=None, center_y=None, width=1.0, height=None, present=True, era_tint=None):
        img = art(path_or_img) if isinstance(path_or_img, str) else path_or_img
        if img is None:
            return
        if height is None:
            height = width * img.height / img.width
        pw, ph = max(1, round(width * PX)), max(1, round(height * PX))
        sprite = img.resize((pw, ph), Image.NEAREST)
        if not present:
            alpha = sprite.getchannel("A").point(lambda a: int(a * 0.18))
            sprite.putalpha(alpha)
        if bottom is not None:
            left, top = to_px(cx - width / 2, bottom + height)
        else:
            left, top = to_px(cx - width / 2, center_y + height / 2)
        canvas.alpha_composite(sprite, (int(round(left)), int(round(top))))

    def here(when):
        return when == "both" or when == era

    for r, line in enumerate(grid):
        for c, ch in enumerate(line):
            x, y = c, h - 1 - r + Y_ALIGN
            floor = y - 0.5
            air_above = ch_at(r - 1, c) not in SOLID_CHARS or ch_at(r - 1, c) in "Gab"
            if ch == "#":
                if garden:
                    floating = air_above and ch_at(r + 1, c) not in SOLID_CHARS
                    if floating:
                        pair = ("Tiles/floor_slab_past", "Tiles/floor_slab_future")
                    elif air_above and (c in plant_cols or c - 1 in plant_cols or c + 1 in plant_cols):
                        pair = ("Tiles/floor_soil_past", "Tiles/floor_soil_future")
                    elif air_above:
                        kind = "pebbles" if c % 5 == 0 else "grass" if c % 2 == 0 else "cobble"
                        pair = (f"Tiles/floor_{kind}_past", f"Tiles/floor_{kind}_future")
                    else:
                        pair = ("Tiles/wall_garden_past", "Tiles/wall_garden_future")
                elif cloister:
                    pair = (("Tiles/floor_slab_past", "Tiles/floor_slab_future") if air_above
                            else ("Tiles/wall_garden_past", "Tiles/wall_garden_future"))
                elif tileset == "machine" and not air_above:
                    pair = ("Tiles/block_metal_past", "Tiles/block_metal_future")
                elif air_above:
                    pair = (("Tiles/floor_a", "Tiles/block_stone_future") if c % 2 == 0
                            else ("Tiles/floor_b", "Tiles/block_metal_future"))
                else:
                    pair = (("Tiles/wall", "Tiles/block_stone_future") if (c + int(y)) % 2 == 0
                            else ("Tiles/wall_b", "Tiles/block_metal_future"))
                paste(pair[0] if era == PAST else pair[1], x, center_y=y)
            elif ch == "p":
                bridge = sum(1 for k in range(-2, 3) if ch_at(r, c + k) == "p") >= 3 and ch_at(r - 1, c) != "p" and ch_at(r + 1, c) != "p"
                paste("Tiles/bridge_plank_tile" if bridge else "Tiles/block_wood_past", x, center_y=y, present=here(PAST))
            elif ch == "f":
                path = "Tiles/wall_garden_future" if garden or cloister else "Tiles/block_stone_future"
                paste(path, x, center_y=y, present=here(FUTURE))
            elif ch == "S":
                paste("Tiles/floor_soil_past" if era == PAST else "Tiles/floor_soil_future", x, center_y=y)
            elif ch in "Gab":
                paste("Props/blockade", x, bottom=floor + 0.02, width=2)
            elif ch == "D":
                paste("Props/door", x, bottom=floor, width=1.5)
            elif ch == "^":
                paste("Props/spikes", x, bottom=floor)
            elif ch in "*12":
                when = {"*": "both", "1": PAST, "2": FUTURE}[ch]
                path = "Props/fragment_future" if ch == "2" else "Props/fragment"
                paste(path, x, bottom=floor + 0.35, width=0.7, present=here(when))
            elif ch == "i":
                path = ("Props/diary" if era == PAST else "Props/plaque") if garden else "Props/inspect"
                paste(path, x, bottom=floor)
            elif ch in "LAB":
                paste("Props/lever", x, bottom=floor, width=0.9)
            elif ch == "c":
                paste("Props/crate" if era == PAST else "Props/crate_future", x, center_y=y, width=0.94)
            elif ch in "xyz":
                paste("Props/plate", x, bottom=floor, width=1)
            elif ch == "h":
                paste("Props/clock_face" if era == PAST else "Props/clock_face_future", x, center_y=y, width=1.25)
            elif ch in "mM":
                paste("Tiles/platform_past" if era == PAST else "Tiles/platform_future", x, center_y=y)
            elif ch == "v":
                paste("Props/blade", x, center_y=y, width=0.9)
            elif ch == "~":
                paste("Props/fire", x, bottom=floor, present=here(PAST))
            elif ch == "@":
                paste(first_frame("Characters/lia_idle"), x, bottom=floor, width=0.95)
    return canvas.convert("RGB")


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    schematic = "--esquema" in sys.argv
    out = pathlib.Path(args[0]) if args else ROOT / "Previas"
    out.mkdir(parents=True, exist_ok=True)
    for i, (name, key, grid, tileset) in enumerate(levels(), start=1):
        for era, label in ((PAST, "passado"), (FUTURE, "futuro")):
            img = render(name, key, grid, era, tileset, schematic)
            path = out / f"fase{i}_{label}.png"
            img.save(path)
        print(f"Fase {i} ({name}): {out}/fase{i}_passado.png e _futuro.png")


if __name__ == "__main__":
    main()
