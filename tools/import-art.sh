#!/usr/bin/env bash
# Copia arte para Assets/Resources.
# Aceita pasta plana OU a estrutura do zip (eco_do_tempo_lote*).
# Uso: ./tools/import-art.sh ~/Downloads/arquivos-eco-novo
set -euo pipefail

SRC="${1:-$HOME/Downloads/arquivos-eco-novo}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

if [[ ! -d "$SRC" ]]; then
  echo "Pasta não encontrada: $SRC"
  exit 1
fi

mkdir -p "$ROOT/Assets/Resources"/{Characters,Props,Tiles,UI,Backgrounds}

map_copy() {
  local dest="$1"; shift
  local target="$ROOT/Assets/Resources/$dest"
  local found=""
  for name in "$@"; do
    if [[ -f "$SRC/$name" ]]; then found="$SRC/$name"; break; fi
    local hit
    hit="$(find "$SRC" -type f -iname "$name" 2>/dev/null | head -1 || true)"
    if [[ -n "${hit:-}" ]]; then found="$hit"; break; fi
  done
  if [[ -z "$found" ]]; then echo "  (faltando) $dest"; return 0; fi
  cp -f "$found" "$target"
  echo "  OK  $dest  <-  $(basename "$found")"
}

echo "Origem: $SRC"
echo "== Characters =="
map_copy Characters/lia_walk.png lia_walk.png lia.png
map_copy Characters/lia_jump.png lia_jump.png
echo "== Props =="
for pair in \
  "Props/spikes.png:spikes.png" \
  "Props/fragment.png:fragment.png" \
  "Props/fragment_future.png:fragment_future.png" \
  "Props/inspect.png:inspect.png" \
  "Props/lever.png:lever.png" \
  "Props/lever_pulled.png:lever_pulled.png" \
  "Props/door.png:door.png" \
  "Props/door_open.png:door_open.png" \
  "Props/sapling.png:sapling.png" \
  "Props/blockade.png:blockade.png" \
  "Props/diary.png:diary.png" \
  "Props/plaque.png:plaque.png"
do
  dest="${pair%%:*}"; name="${pair##*:}"
  map_copy "$dest" "$name"
done
echo "== Tiles =="
for pair in \
  "Tiles/wall_tileset.png:wall_tileset.png" \
  "Tiles/wall.png:wall.png" \
  "Tiles/wall_b.png:wall_b.png" \
  "Tiles/wall_c.png:wall_c.png" \
  "Tiles/floor_a.png:floor_a.png" \
  "Tiles/floor_b.png:floor_b.png" \
  "Tiles/floor_c.png:floor_c.png" \
  "Tiles/block_wood_past.png:block_wood_past.png" \
  "Tiles/block_wood_future.png:block_wood_future.png" \
  "Tiles/block_stone_past.png:block_stone_past.png" \
  "Tiles/block_stone_future.png:block_stone_future.png" \
  "Tiles/block_metal_past.png:block_metal_past.png" \
  "Tiles/block_metal_future.png:block_metal_future.png" \
  "Tiles/blocks_tileset.png:blocks_tileset.png" \
  "Tiles/bridge_past.png:bridge_past.png" \
  "Tiles/gate.png:gate.png" \
  "Tiles/soil.png:soil.png" \
  "Tiles/soil_future.png:soil_future.png" \
  "Tiles/floor_garden_past.png:floor_garden_past.png" \
  "Tiles/floor_garden_future.png:floor_garden_future.png" \
  "Tiles/wall_garden_past.png:wall_garden_past.png" \
  "Tiles/wall_garden_future.png:wall_garden_future.png" \
  "Tiles/tree_trunk.png:tree_trunk.png" \
  "Tiles/tree_trunk_future.png:tree_trunk_future.png"
do
  dest="${pair%%:*}"; name="${pair##*:}"
  map_copy "$dest" "$name"
done
# Aliases: generic block_* → wood variants when dedicated files are absent.
if [[ ! -f "$ROOT/Assets/Resources/Tiles/block_past.png" && -f "$ROOT/Assets/Resources/Tiles/block_wood_past.png" ]]; then
  cp -f "$ROOT/Assets/Resources/Tiles/block_wood_past.png" "$ROOT/Assets/Resources/Tiles/block_past.png"
  echo "  OK  Tiles/block_past.png  <-  block_wood_past.png"
fi
if [[ ! -f "$ROOT/Assets/Resources/Tiles/block_future.png" && -f "$ROOT/Assets/Resources/Tiles/block_wood_future.png" ]]; then
  cp -f "$ROOT/Assets/Resources/Tiles/block_wood_future.png" "$ROOT/Assets/Resources/Tiles/block_future.png"
  echo "  OK  Tiles/block_future.png  <-  block_wood_future.png"
fi
map_copy Tiles/block_past.png block_past.png
map_copy Tiles/block_future.png block_future.png
echo "== UI =="
map_copy UI/fragment_ui.png fragment_ui.png
echo "== Backgrounds =="
map_copy Backgrounds/salao_past.png salao_past.png
map_copy Backgrounds/salao_future.png salao_future.png
map_copy Backgrounds/salao_past_1472x576.png salao_past_1472x576.png
map_copy Backgrounds/salao_future_1472x576.png salao_future_1472x576.png
map_copy Backgrounds/jardim_past_1472x576.png jardim_past_1472x576.png
map_copy Backgrounds/jardim_future_1472x576.png jardim_future_1472x576.png

echo
find "$ROOT/Assets/Resources" -name '*.png' | sort
