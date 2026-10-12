#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
RES="$ROOT/Assets/Resources"
ok=0
missing=0

check() {
  local rel="$1"
  if [[ -f "$RES/$rel" ]]; then
    echo "  OK   $rel"
    ok=$((ok+1))
  else
    echo "  FALTA $rel"
    missing=$((missing+1))
  fi
}

echo "Verificando: $RES"
echo "== Characters =="; check "Characters/lia_walk.png"
echo "== Props =="
for f in spikes fragment fragment_future inspect lever lever_pulled door door_open sapling; do check "Props/$f.png"; done
echo "== Tiles =="
for f in wall wall_b wall_c floor_a floor_b floor_c block_past block_future gate soil wall_tileset; do check "Tiles/$f.png"; done
echo "== UI =="; check "UI/fragment_ui.png"
echo "== Backgrounds =="
check "Backgrounds/salao_past.png"; check "Backgrounds/salao_future.png"
echo
echo "Resumo: $ok ok, $missing faltando"
