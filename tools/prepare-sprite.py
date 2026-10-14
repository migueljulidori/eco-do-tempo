#!/usr/bin/env python3
"""Deixa uma imagem gerada (ChatGPT etc.) pronta para o jogo.

Reduz para o tamanho exato do sprite, remove o fundo magenta (#FF00FF) e limita a paleta.

Exemplos:
  python3 tools/prepare-sprite.py ~/Downloads/caixa.png Assets/Resources/Props/crate.png --size 32x32
  python3 tools/prepare-sprite.py ~/Downloads/lia.png Assets/Resources/Characters/lia_walk.png --size 32x32 --frames 6
  python3 tools/prepare-sprite.py ~/Downloads/claustro.png Assets/Resources/Backgrounds/claustro_past.png --size 1472x576 --opaque

Precisa do Pillow: pip install pillow
"""
import argparse
import sys

try:
    from PIL import Image
except ImportError:
    sys.exit("Instale o Pillow primeiro: pip install pillow")

KEY = (255, 0, 255)


def parse_size(text):
    w, h = text.lower().split("x")
    return int(w), int(h)


def is_key(pixel, tolerance):
    r, g, b = pixel[:3]
    return abs(r - KEY[0]) + abs(g - KEY[1]) + abs(b - KEY[2]) <= tolerance


def crop_to_content(img, tolerance):
    """Cut away the empty magenta margin so the drawing fills the frame."""
    rgb = img.convert("RGB")
    w, h = rgb.size
    px = rgb.load()
    xs, ys = [], []
    for y in range(h):
        for x in range(w):
            if not is_key(px[x, y], tolerance):
                xs.append(x)
                ys.append(y)
    if not xs:
        return img
    return img.crop((min(xs), min(ys), max(xs) + 1, max(ys) + 1))


def fit(img, size, scale=None):
    """Scale (no stretching), centered horizontally, standing on the bottom edge."""
    w, h = size
    if scale is None:
        scale = min(w / img.width, h / img.height)
    new = (max(1, round(img.width * scale)), max(1, round(img.height * scale)))
    small = img.resize(new, Image.BOX)
    canvas = Image.new("RGBA", size, KEY + (255,))
    canvas.paste(small, ((w - new[0]) // 2, h - new[1]))
    return canvas


def key_out(img, tolerance):
    img = img.convert("RGBA")
    px = img.load()
    for y in range(img.height):
        for x in range(img.width):
            p = px[x, y]
            # Pixels blended with the magenta background become see-through too.
            if is_key(p, tolerance) or (p[0] > 180 and p[2] > 180 and p[1] < 90):
                px[x, y] = (0, 0, 0, 0)
    return img


def limit_palette(img, colors):
    alpha = img.getchannel("A")
    rgb = img.convert("RGB").quantize(colors=colors, method=Image.Quantize.MEDIANCUT).convert("RGB")
    out = rgb.convert("RGBA")
    out.putalpha(alpha.point(lambda a: 255 if a >= 128 else 0))
    return out


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("input")
    ap.add_argument("output")
    ap.add_argument("--size", required=True, help="tamanho de UM quadro, ex.: 32x32")
    ap.add_argument("--frames", type=int, default=1, help="quadros lado a lado na imagem (folha de animação)")
    ap.add_argument("--opaque", action="store_true", help="fundo de cenário: sem transparência, preenche tudo")
    ap.add_argument("--colors", type=int, default=24, help="cores máximas por sprite (0 = sem limite)")
    ap.add_argument("--tolerance", type=int, default=90, help="o quanto a cor pode fugir do magenta puro")
    args = ap.parse_args()

    size = parse_size(args.size)
    src = Image.open(args.input).convert("RGBA")

    if args.opaque:
        out = src.convert("RGB")
        scale = max(size[0] / out.width, size[1] / out.height)
        out = out.resize((round(out.width * scale), round(out.height * scale)), Image.BOX)
        left = (out.width - size[0]) // 2
        top = (out.height - size[1]) // 2
        out = out.crop((left, top, left + size[0], top + size[1]))
        out.save(args.output)
        print(f"OK {args.output} {out.size[0]}x{out.size[1]} (opaco)")
        return

    frame_w = src.width / args.frames
    frames = [crop_to_content(src.crop((round(i * frame_w), 0, round((i + 1) * frame_w), src.height)),
                              args.tolerance) for i in range(args.frames)]
    # One scale for every frame, so the character keeps the same size while animating.
    tallest = max(f.height for f in frames)
    widest = max(f.width for f in frames)
    scale = min(size[0] / widest, size[1] / tallest) * (0.94 if args.frames > 1 else 1.0)
    sheet = Image.new("RGBA", (size[0] * args.frames, size[1]), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        frame = key_out(fit(frame, size, scale), args.tolerance)
        if args.colors > 0:
            frame = limit_palette(frame, args.colors)
        sheet.paste(frame, (i * size[0], 0))
    sheet.save(args.output)
    print(f"OK {args.output} {sheet.size[0]}x{sheet.size[1]} ({args.frames} quadro(s) de {size[0]}x{size[1]})")


if __name__ == "__main__":
    main()
