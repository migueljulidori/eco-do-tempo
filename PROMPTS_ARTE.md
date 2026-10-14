# Prompts de pixel art para o ChatGPT

Sprites que o jogo já procura e ainda não existem, mais algumas peças que deixam as fases novas mais bonitas. Sem eles, o jogo usa formas simples, então nada quebra.

## Como usar (leia antes)

1. **Abra uma conversa nova no ChatGPT para cada sprite.** Misturar pedidos faz ele copiar coisas de um no outro.
2. **Anexe as referências indicadas** (arraste os PNGs da pasta do projeto). É isso que mantém o estilo igual ao da Lia e dos fundos.
3. Cole o **bloco de estilo** e depois o **prompt do sprite**.
4. O ChatGPT quase nunca entrega o tamanho exato nem a transparência certa. Por isso todos os prompts pedem **fundo magenta liso (#FF00FF)**. Baixe a imagem e rode o script, que reduz, recorta e tira o magenta:

   ```bash
   pip install pillow          # só na primeira vez
   python3 tools/prepare-sprite.py ~/Downloads/ARQUIVO.png Assets/Resources/Props/crate.png --size 32x32
   ```

5. Na Unity, selecione o PNG e use as mesmas configurações dos outros sprites: **Texture Type** = Sprite (2D and UI), **Filter Mode** = Point, **Compression** = None, **Pixels Per Unit** = 32. Clique em **Apply**.
6. Se o resultado vier borrado ou com "pixels" de tamanhos diferentes, responda no ChatGPT: *"Redo it as true pixel art on a strict grid: every pixel the same size, no anti-aliasing, no blur, no gradients."*

---

## Bloco de estilo (cole no começo de todo pedido)

```
Style: 2D side-view pixel art for a puzzle-platformer about a clockmaker's apprentice who travels between the PAST and the FUTURE of the same town.
- True pixel art on a strict grid, every pixel the same size, hard edges, no anti-aliasing, no blur, no soft gradients, 1-pixel dark outline (#1A1008).
- Light comes from the upper left.
- PAST version: warm, cared for, brass and polished wood, palette #2A1810 #5A3A28 #8B5A2B #C4A574 #E6A64D, amber highlights.
- FUTURE version: the same object decades later, abandoned: rust, cracks, dust, palette #0A101F #1A2438 #38506B #5A6F86 #A0B4C8, rust #6E3A2A, at most a tiny accent of cold cyan #4DE6FF.
- Match the attached reference images exactly in pixel density and outline style.
- Background: one flat solid magenta #FF00FF, filling everything that is not the object. No shadows on the background, no floor, no text, no letters, no watermark, no border.
```

---

## 1. Lia andando (o mais importante)

Hoje a Lia só tem as animações parada e pulando: ao andar, ela desliza.

**Anexe:** `Assets/Resources/Characters/lia_idle.png` e `lia_jump.png`
**Salvar como:** `Assets/Resources/Characters/lia_walk.png` (192×32, 6 quadros de 32×32)

```
Create a WALK CYCLE sprite sheet of the same girl in the attached reference sprites (Lia: messy brown hair, red scarf trailing behind, cream shirt, brown vest and trousers, brown boots).
- 6 frames in ONE horizontal row, evenly spaced, each frame the same size, character facing RIGHT.
- Same proportions, colors and outline as the reference: she is about 28 pixels tall inside a 32x32 frame, feet on the bottom of the frame.
- Classic 6-frame walk: contact, down, passing, up, contact (other leg), passing. The scarf trails and bounces slightly.
- Keep the head the same size and position in every frame (no zooming), only legs, arms and scarf move.
```

Depois:
```bash
python3 tools/prepare-sprite.py ~/Downloads/ARQUIVO.png Assets/Resources/Characters/lia_walk.png --size 32x32 --frames 6
```
Na Unity: **Sprite Mode** = Multiple → **Sprite Editor** → Slice → Grid By Cell Size 32×32 → Apply.

## 2. Caixa de ferro (fases 2, 4 e 6)

A caixa precisa parecer **móvel**, diferente dos blocos de parede.

**Anexe:** `Assets/Resources/Tiles/block_metal_past.png` e `block_metal_future.png` (para ela NÃO ficar igual)
**Salvar como:** `Props/crate.png` e `Props/crate_future.png` (32×32 cada)

```
Create TWO separate images of the same object, side by side, clearly separated by magenta space:
LEFT = PAST, RIGHT = FUTURE.
Object: a small heavy iron crate that can be pushed, square, front view, filling the frame. Riveted iron frame, brass corner caps, two brass handles on the sides, a small engraved clock gear on the front. It must look like a movable box, NOT like a wall tile: rounded corners, visible handles, a thin shadow line at the bottom.
PAST: polished dark iron, shiny brass.
FUTURE: same crate decades later, covered in orange rust, one handle broken, moss at the bottom edge, rusted to the floor.
```
O script processa uma imagem por vez: recorte a esquerda e a direita antes (qualquer editor serve), ou peça ao ChatGPT uma imagem para cada era.

## 3. Lâmina do pêndulo (fases 5 e 6)

Ela **gira** no jogo, então precisa ser redonda e simétrica.

**Salvar como:** `Props/blade.png` (32×32)

```
Create one object: a round spinning sawblade from a giant clock mechanism, seen flat from the front, perfectly circular and symmetric so it can rotate. Steel disc with 8 sharp teeth, a brass hub in the center with a small gear pattern, a thin red warning stripe. It fills the frame. Dangerous but elegant, clockwork style.
```

## 4. Brasa da forja (fase 3)

Só existe no passado. O jogo a faz "respirar" sozinho.

**Salvar como:** `Props/fire.png` (32×32, o fogo encostado na borda de baixo)

```
Create one object: a low pile of glowing forge coals with small flames on top, side view, sitting on the bottom edge of the image, about half as tall as it is wide. Deep red coals, orange and yellow flame tips, a few bright sparks. No floor, no forge, only the coals and flames.
```

## 5. Placa de pressão (fases 4 e 6)

**Salvar como:** `Props/plate.png` e `Props/plate_pressed.png` (32×32 cada; a placa ocupa só a faixa de baixo)

```
Create TWO separate images side by side, separated by magenta space:
LEFT = plate UP, RIGHT = plate PRESSED DOWN.
Object: a floor pressure plate seen from the side: a flat brass plate on a stone base, only about 6 pixels tall out of 32, sitting on the bottom edge of the image, wide as the whole frame. A small engraved clock symbol on the plate. When pressed, the brass plate is lower and flush with the base, and a thin glowing line appears along its edge.
Everything above the plate is magenta.
```
A cor do canal é aplicada pelo jogo por cima, então desenhe em latão neutro.

## 6. Mostrador do relógio-cadeado (fase 2)

O jogo desenha o **ponteiro** por cima e o gira. Portanto: **sem ponteiros**.

**Anexe:** `Assets/Resources/Backgrounds/jardim_past_1472x576.png` (tem o relógio pintado no muro)
**Salvar como:** `Props/clock_face.png` e `Props/clock_face_future.png` (32×32 cada)

```
Create TWO separate images side by side, separated by magenta space: LEFT = PAST, RIGHT = FUTURE.
Object: a small round wall clock face, front view, perfectly centered and circular, filling the frame. Brass rim, cream dial with 12 short tick marks (longer at 12, 3, 6, 9). IMPORTANT: NO clock hands at all, the center is empty except a tiny brass pin.
PAST: shiny brass, clean cream dial.
FUTURE: same clock, rusted rim, cracked dirty dial, moss on one side.
Match the clock painted on the wall in the attached garden background.
```

## 7. Plataforma de engrenagem (elevadores e plataformas móveis, fases 3 e 6)

**Anexe:** `Assets/Resources/Tiles/block_metal_past.png`
**Salvar como:** `Tiles/platform_past.png` e `Tiles/platform_future.png` (32×32 cada, opacos, borda a borda)

```
Create TWO separate square tiles side by side, separated by magenta space: LEFT = PAST, RIGHT = FUTURE.
Object: one tile of a moving machine platform, seen from the side, filling the whole tile edge to edge (no magenta inside the tile). A brass and iron beam with visible gear teeth along the bottom edge and two rivets, a walkable flat top. It must tile seamlessly left-to-right with itself.
PAST: polished brass with warm highlights. FUTURE: same tile rusted, teeth chipped, cyan crack accent.
```
Este é um **tile opaco**: depois do script, confira se não sobrou magenta nas bordas.

---

## 8. Fundos novos (opcional, deixam as fases 4 e 6 com cara própria)

Hoje a fase 4 reaproveita o jardim e a fase 6 reaproveita a oficina. Fundos próprios são o que mais muda a sensação de "fase nova". Depois de salvar, me peça para trocar o `Background` dessas fases (é uma linha).

### 8a. Claustro dos Ecos (fase 4) — 1472×576, opaco

**Anexe:** `jardim_past_1472x576.png` e `jardim_future_1472x576.png` (mesma composição e escala)
**Salvar como:** `Backgrounds/claustro_past.png` e `claustro_future.png`

```
Pixel art background, exact canvas 1472x576, opaque, no characters, no text, no UI.
Side view of a quiet stone CLOISTER behind the clockmaker's garden: a row of stone arches with columns along the back wall, ivy, small echo-symbol carvings (concentric circles) on the arches, hanging lanterns.
The walkable floor is ONE continuous stone ledge at exactly y=426 from the top, with 150 px of foundation masonry below it down to the bottom edge. No pits, no platforms, no stairs, no doors drawn on the floor: gameplay objects are added later as sprites.
Keep the center of the image calm and low in contrast so sprites stay readable.
Same pixel density and camera as the attached garden background.
```
Para o futuro, mesma conversa:
```
Now the SAME image, same composition pixel for pixel, same ledge at y=426: decades later, abandoned. Broken arches, dead ivy, cold blue-gray fog, lanterns dark and shattered, puddles. Palette #0A101F #1A2438 #38506B #5A6F86 #A0B4C8, rust #6E3A2A, a few cyan #4DE6FF glints in cracks.
```
```bash
python3 tools/prepare-sprite.py ~/Downloads/ARQUIVO.png Assets/Resources/Backgrounds/claustro_past.png --size 1472x576 --opaque
```

### 8b. O Coração do Relógio (fase 6) — 2900×1154, opaco

Os vãos do chão precisam ficar **exatamente** nos mesmos lugares da oficina, porque o mapa da fase 6 usa os mesmos buracos.

**Anexe:** `oficina_past.png` e `oficina_future.png`
**Salvar como:** `Backgrounds/coracao_past.png` e `coracao_future.png`

```
Pixel art background, exact canvas 2900x1154, opaque, no characters, no text.
Use the attached workshop image as the EXACT layout template: same stone floor lip at y=664 from the top, and the same three holes in the floor at the same x positions (650-900, 1900-2250, 2700-2850). Keep the masonry below the lip the same.
Change only what is above the floor: this is the HEART OF THE GREAT CLOCK, deeper inside the tower. Huge brass gears turning on the back wall, a giant pendulum shaft in the middle (x 1250-1600) rising out of frame, chains, glowing amber clock glass windows, steam pipes.
Do not draw platforms, elevators, blades, levers, gates or doors: those are sprites added by the game.
```
Futuro, mesma conversa:
```
Now the SAME image, identical layout and holes: decades later. Gears frozen and rusted, the pendulum broken and hanging still, glass shattered, cold fog, palette #0A101F #1A2438 #38506B #5A6F86 #A0B4C8, rust #6E3A2A, small cyan #4DE6FF cracks.
```
```bash
python3 tools/prepare-sprite.py ~/Downloads/ARQUIVO.png Assets/Resources/Backgrounds/coracao_past.png --size 2900x1154 --opaque
```
Fundos usam **PPU 128** na Unity (como os outros fundos), não 32.

---

## Ordem sugerida

1. `lia_walk` (aparece o tempo todo)
2. `crate` / `crate_future`
3. `blade`, `fire`
4. `plate`, `clock_face`, `platform`
5. Fundos 8a e 8b

Quando adicionar qualquer um, aperte Play: o jogo pega o arquivo sozinho, sem mexer em código.
