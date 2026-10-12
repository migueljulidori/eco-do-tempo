#!/usr/bin/env bash
# Verifica se os PNGs esperados pelo jogo estão em Assets/Resources.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
RES="$ROOT/Assets/Resources"
ok=0
missing=0

check() {
  local rel="$1"
  if [[ -f "$RES/$rel" ]]; then
    echo "  OK   $rel  ($(stat -c%s "$RES/$rel") bytes)"
    ok=$((ok+1))
  else
    echo "  FALTA $rel"
    missing=$((missing+1))
  fi
}

echo "Verificando arte em: $RES"
echo
echo "== Characters =="
check "Characters/lia_walk.png"
echo "== Props =="
for f in spikes fragment fragment_future inspect lever lever_pulled door door_open sapling; do
  check "Props/$f.png"
done
echo "== Tiles =="
for f in wall block_past block_future gate soil; do
  check "Tiles/$f.png"
done
echo "== Backgrounds =="
check "Backgrounds/salao_past.png"
check "Backgrounds/salao_future.png"

echo
echo "Resumo: $ok presentes, $missing faltando"
if [[ "$missing" -gt 0 ]]; then
  echo "Obs: salao_future é opcional; o resto é usado na Fase 1."
fi
