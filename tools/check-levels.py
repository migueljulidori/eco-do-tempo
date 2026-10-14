#!/usr/bin/env python3
"""Confere os mapas de Assets/Scripts/Level/LevelData.cs sem abrir o Unity.

Uso: python3 tools/check-levels.py
Aponta linhas de tamanhos diferentes, símbolos desconhecidos, fragmentos faltando,
comportas sem alavanca/placa/relógio, peças móveis sem trilho e textos faltando.
"""
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Assets/Scripts/Level/LevelData.cs"

KNOWN = set(".#!pf^~@D*12SicxyzhLGAaBbmMv=|")
SOLID = set("#!pfSG")
CHANNELS = {0: ("L", "x", "G"), 1: ("A", "y", "a"), 2: ("B", "z", "b")}


def blocks(text):
    """Split the Levels array into one chunk per LevelDefinition."""
    start = text.index("Levels =")
    parts = text[start:].split("new LevelDefinition")[1:]
    return parts


def track_length(grid, row, col, d_row, d_col, rail):
    n = 0
    while 0 <= row < len(grid) and 0 <= col < len(grid[row]) and grid[row][col] == rail:
        n += 1
        row += d_row
        col += d_col
    return n


def check_level(index, chunk, per_level):
    errors, notes = [], []
    name = re.search(r'Name = "([^"]*)"', chunk).group(1)
    map_body = re.search(r"Map = new\[\]\s*\{(.*?)\}", chunk, re.S).group(1)
    grid = re.findall(r'"([^"]*)"', map_body)
    label = f"Fase {index + 1} ({name})"

    widths = {len(r) for r in grid}
    if len(widths) != 1:
        errors.append(f"linhas com larguras diferentes: {sorted(widths)}")

    for r, line in enumerate(grid):
        for c, ch in enumerate(line):
            if ch not in KNOWN:
                errors.append(f"símbolo desconhecido '{ch}' na linha {r}, coluna {c}")

    flat = "".join(grid)
    if flat.count("@") != 1:
        errors.append(f"precisa de exatamente um '@' (tem {flat.count('@')})")
    if "D" not in flat:
        errors.append("sem porta 'D'")

    fragments = sum(flat.count(ch) for ch in "*12")
    if fragments != per_level:
        errors.append(f"{fragments} fragmentos; o jogo espera {per_level} por fase")

    inspect_texts = chunk.count("new InspectNote(")
    if flat.count("i") != inspect_texts:
        notes.append(f"{flat.count('i')} objetos 'i' e {inspect_texts} textos em Notes")

    clocks = chunk.count("new ClockPuzzle(")
    if flat.count("h") != clocks:
        errors.append(f"{flat.count('h')} relógios 'h' e {clocks} configurações em Clocks")
    clock_channels = [int(m) for m in re.findall(r"new ClockPuzzle\(\s*\d+\s*,\s*(\d+)", chunk)]

    for ch, (lever, plate, gate) in CHANNELS.items():
        has_gate = gate in flat
        has_opener = lever in flat or plate in flat or ch in clock_channels
        if has_gate and not has_opener:
            errors.append(f"comporta '{gate}' (canal {ch}) sem alavanca '{lever}', placa '{plate}' nem relógio")
        if has_opener and not has_gate:
            notes.append(f"canal {ch} tem alavanca/placa/relógio mas nenhuma comporta '{gate}'")

    for r, line in enumerate(grid):
        c = 0
        while c < len(line):
            ch = line[c]
            if ch not in "mMv":
                c += 1
                continue
            end = c
            while ch != "v" and end + 1 < len(line) and line[end + 1] == ch:
                end += 1
            found = (track_length(grid, r, end + 1, 0, 1, "=")
                     or track_length(grid, r, c - 1, 0, -1, "=")
                     or track_length(grid, r - 1, c, -1, 0, "|")
                     or track_length(grid, r + 1, c, 1, 0, "|"))
            if not found:
                errors.append(f"peça móvel '{ch}' na linha {r}, coluna {c} sem trilho '=' ou '|' encostado")
            c = end + 1

    for r, line in enumerate(grid):
        c = line.find("@")
        if c >= 0 and (r + 1 >= len(grid) or grid[r + 1][c] not in SOLID):
            errors.append("Lia ('@') não começa em cima de um bloco")

    status = "OK" if not errors else "ERRO"
    size = f"{len(grid[0]) if grid else 0}×{len(grid)}"
    print(f"[{status}] {label}: {size}, {fragments} fragmentos")
    for e in errors:
        print(f"    erro: {e}")
    for n in notes:
        print(f"    aviso: {n}")
    return not errors


def main():
    text = SOURCE.read_text(encoding="utf-8")
    per_level = int(re.search(r"FragmentsPerLevel = (\d+)", text).group(1))
    melody = re.search(r"Melody =\s*\{(.*?)\}", text, re.S).group(1)
    melody_len = len(re.findall(r"\d+", melody))
    levels = blocks(text)
    ok = all([check_level(i, chunk, per_level) for i, chunk in enumerate(levels)])
    if melody_len != len(levels) * per_level:
        print(f"[ERRO] A melodia tem {melody_len} notas; {len(levels)} fases × {per_level} pedem {len(levels) * per_level}.")
        ok = False
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
