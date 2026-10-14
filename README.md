# Eco do Tempo

Jogo de plataforma e puzzle 2D na Unity. Lia viaja entre o **passado** e o **futuro** para reunir os 18 fragmentos da *Canção do Eco*.

São seis fases, cada uma construída em torno de uma ideia de tempo (inspiração: *That Level Again* e *Cube Escape*). O diapasão de Lia aprende três poderes: trocar de era (Q), deixar um eco de si mesma (F) e segurar o tempo com a fermata (C).

**Passo a passo na Unity, roteiro de teste e como editar as fases: [GUIA_UNITY.md](GUIA_UNITY.md).**

## Como rodar

1. No Unity Hub: **Add > Add project from disk** e selecione esta pasta (Unity 2022.3 LTS ou Unity 6).
2. Abra qualquer cena (a `SampleScene` serve) e aperte **Play**. O jogo monta tudo por código.
3. Na tela de título, as teclas **1–6** abrem uma fase direto (só no editor).

## Controles

| Tecla | Ação |
| --- | --- |
| A/D ou setas | mover |
| Espaço / W | pular |
| Q | alternar passado/futuro |
| F | eco: gravar; F de novo volta no tempo e deixa um eco repetindo seus passos |
| C | fermata: segurar o tempo por 3 s |
| E | interagir / examinar / girar relógio |
| R | reiniciar a fase |
| Enter | avançar textos |

## Arte (LibreSprite → Unity)

Todos os PNGs vão em `Assets/Resources/...`. Sem o arquivo, o jogo mantém a forma simples.

### Personagem
| Arquivo | Tamanho sugerido |
| --- | --- |
| `Characters/lia_walk.png` (sprite sheet horizontal) | frames 32×32 |

Inspector: Texture Type `Sprite (2D and UI)`, Mode `Multiple`, Filter `Point`, PPU `32` → Sprite Editor → Slice.

### Objetos (`Resources/Props/`) — um PNG cada, Mode `Single`
| Arquivo | O quê |
| --- | --- |
| `spikes.png` | espinhos `^` |
| `fragment.png` | fragmentos da canção `*` / `1` / `2` |
| `fragment_future.png` | (opcional) fragmento só no futuro |
| `inspect.png` | objetos para examinar `i` |
| `lever.png` | alavanca `L` / `A` / `B` |
| `lever_pulled.png` | (opcional) alavanca puxada |
| `door.png` | porta fechada `D` |
| `door_open.png` | (opcional) porta aberta |
| `sapling.png` | muda plantada `S` |
| `blockade.png` | comporta `G` / `a` / `b` |

### Chão / blocos (`Resources/Tiles/`)
| Arquivo | O quê |
| --- | --- |
| `wall.png` / `wall_b.png` / `wall_c.png` | parede `#` |
| `floor_a.png` / `floor_b.png` / `floor_c.png` | superfície caminhável `#` |
| `block_stone_past.png` / `block_stone_future.png` | pedra (chão / parede) |
| `block_wood_past.png` / `block_wood_future.png` | madeira (`p` / `f` curtos) |
| `block_metal_past.png` / `block_metal_future.png` | metal (variação) |
| `bridge_past.png` (128×32) | ponte no vão longo de `p` |
| `blockade.png` em `Props/` (64×64) | comporta `G` |
| `gate.png` | fallback da comporta |
| `soil.png` | terra da semente `S` |

Inspector dos tiles/props: Texture Type `Sprite (2D and UI)`, Mode `Single` (tileset = `Multiple` + Slice 32×32), Filter `Point`, Compression `None`, **PPU 32**. Ponte e bloqueio: pivot **Bottom Center**.

### Fundos (`Resources/Backgrounds/`)
| Arquivo | Fase |
| --- | --- |
| `salao_past.png` / `salao_future.png` | Salão do Relógio |
| `jardim_past.png` / `jardim_future.png` | Jardim Esquecido |
| `oficina_past.png` / `oficina_future.png` | A Oficina (ainda sem arte) |

Inspector do fundo: Sprite (2D and UI), Mode `Single`, Filter `Point` (ou Bilinear se a arte for grande), Compression `None`.

Sugestão tiles/props: canvas **32×32** (porta pode ser **32×64**). Filter Mode `Point`, Compression `None`.

## Criando fases

As fases ficam em `Assets/Scripts/Level/LevelData.cs`, desenhadas em texto (ASCII). A legenda está no comentário de `LevelDefinition`. O jogo e a prévia na Hierarchy saem sempre desse texto.

Alavancas, placas, relógios e comportas usam **canais** (a cor do losango mostra o canal): `L`/`x`/`G` no canal 0, `A`/`y`/`a` no 1, `B`/`z`/`b` no 2. Alavanca e relógio abrem para sempre; placas seguram só enquanto alguém pisa (todas as placas do canal ao mesmo tempo).

Outros símbolos: `c` caixa de ferro, `h` relógio-cadeado, `m`/`M` plataformas móveis, `v` lâmina, `=`/`|` trilhos, `~` brasa.

Para conferir os mapas sem abrir a Unity: `python3 tools/check-levels.py`.
