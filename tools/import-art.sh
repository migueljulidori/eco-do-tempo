#!/usr/bin/env bash
# Copia arte de uma pasta (ex.: ~/Downloads/arquivos-eco) para Assets/Resources.
# Uso:
#   ./tools/import-art.sh ~/Downloads/arquivos-eco
set -euo pipefail

SRC="${1:-$HOME/Downloads/arquivos-eco}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

if [[ ! -d "$SRC" ]]; then
  echo "Pasta não encontrada: $SRC"
  echo "Uso: $0 /caminho/para/arquivos-eco"
  exit 1
fi

mkdir -p \
  "$ROOT/Assets/Resources/Characters" \
  "$ROOT/Assets/Resources/Props" \
  "$ROOT/Assets/Resources/Tiles" \
  "$ROOT/Assets/Resources/Backgrounds"

# dest_relative|nome possível 1|nome possível 2|...
map_copy() {
  local dest="$1"; shift
  local target="$ROOT/Assets/Resources/$dest"
  local found=""
  for name in "$@"; do
    # aceita exatamente o nome, ou com espaços/variações comuns
    if [[ -f "$SRC/$name" ]]; then
      found="$SRC/$name"
      break
    fi
    # busca case-insensitive pelo basename
    local hit
    hit="$(find "$SRC" -maxdepth 2 -type f -iname "$name" 2>/dev/null | head -1 || true)"
    if [[ -n "${hit:-}" ]]; then
      found="$hit"
      break
    fi
  done

  if [[ -z "$found" ]]; then
    echo "  (faltando) $dest"
    return 0
  fi
  cp -f "$found" "$target"
  echo "  OK  $dest  <-  $(basename "$found")"
}

echo "Origem:  $SRC"
echo "Destino: $ROOT/Assets/Resources"
echo

echo "== Characters =="
map_copy "Characters/lia_walk.png" \
  "lia_walk.png" "lia-walk.png" "Lia_walk.png" "lia.png" "lia_walk_sheet.png"

echo "== Props =="
map_copy "Props/spikes.png" "spikes.png" "espinhos.png" "spike.png"
map_copy "Props/fragment.png" "fragment.png" "fragmento.png" "melody_fragment.png"
map_copy "Props/fragment_future.png" "fragment_future.png" "fragmento_futuro.png" "fragment-future.png"
map_copy "Props/inspect.png" "inspect.png" "examinar.png" "cartaz.png" "placa.png"
map_copy "Props/lever.png" "lever.png" "alavanca.png"
map_copy "Props/lever_pulled.png" "lever_pulled.png" "alavanca_puxada.png" "lever-pulled.png"
map_copy "Props/door.png" "door.png" "porta.png"
map_copy "Props/door_open.png" "door_open.png" "porta_aberta.png" "door-open.png"
map_copy "Props/sapling.png" "sapling.png" "muda.png" "broto.png"

echo "== Tiles =="
map_copy "Tiles/wall.png" "wall.png" "parede.png"
map_copy "Tiles/block_past.png" "block_past.png" "bloco_passado.png" "block-past.png" "ponte.png"
map_copy "Tiles/block_future.png" "block_future.png" "bloco_futuro.png" "block-future.png"
map_copy "Tiles/gate.png" "gate.png" "comporta.png" "portao.png"
map_copy "Tiles/soil.png" "soil.png" "terra.png"

echo "== Backgrounds =="
map_copy "Backgrounds/salao_past.png" "salao_past.png" "salao-past.png" "bg_salao_past.png" "background_past.png"
map_copy "Backgrounds/salao_future.png" "salao_future.png" "salao-future.png" "bg_salao_future.png" "background_future.png"

echo
echo "Arquivos agora em Resources:"
find "$ROOT/Assets/Resources" -type f \( -name '*.png' -o -name '*.jpg' -o -name '*.webp' \) | sort
echo
echo "Pronto. Abra a Unity, selecione os PNGs novos e configure:"
echo "  Texture Type = Sprite (2D and UI)"
echo "  Filter Mode  = Point (no filter)"
echo "  Compression  = None"
echo "  lia_walk     = Sprite Mode Multiple + Slice"
echo "  demais       = Sprite Mode Single"
