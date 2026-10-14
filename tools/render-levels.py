#!/usr/bin/env python3
"""Desenha cada fase por cima do fundo, como o jogo posiciona, sem abrir a Unity.

Uso: python3 tools/render-levels.py [pasta_de_saida]
Gera fase1_passado.png, fase1_futuro.png... (padrão: Previas/). Precisa do Pillow.
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


def levels():
    text = SOURCE.read_text(encoding="utf-8")
    chunks = text[text.index("Levels ="):].split("new LevelDefinition")[1:]
    for chunk in chunks:
        name = re.search(r'Name = "([^"]*)"', chunk).group(1)
        bg = re.search(r'Background = "([^"]*)"', chunk)
        grid = re.findall(r'"([^"]*)"', re.search(r"Map = new\[\]\s*\{(.*?)\}", chunk, re.S).group(1))
        yield name, bg.group(1) if bg else None, grid


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


def render(name, key, grid, era):
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


def main():
    out = pathlib.Path(sys.argv[1]) if len(sys.argv) > 1 else ROOT / "Previas"
    out.mkdir(parents=True, exist_ok=True)
    for i, (name, key, grid) in enumerate(levels(), start=1):
        for era, label in ((PAST, "passado"), (FUTURE, "futuro")):
            img = render(name, key, grid, era)
            path = out / f"fase{i}_{label}.png"
            img.save(path)
        print(f"Fase {i} ({name}): {out}/fase{i}_passado.png e _futuro.png")


if __name__ == "__main__":
    main()
