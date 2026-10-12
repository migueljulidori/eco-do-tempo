# Eco do Tempo

Jogo de plataforma e puzzle 2D na Unity. Lia viaja entre o **passado** e o **futuro** para reunir os 12 fragmentos da *Canção do Eco*.

## Como rodar

1. No Unity Hub: **Add > Add project from disk** e selecione esta pasta (Unity 2022.3 LTS ou Unity 6).
2. Abra qualquer cena (a `SampleScene` serve) e aperte **Play**. O jogo monta tudo por código.

## Controles

| Tecla | Ação |
| --- | --- |
| A/D ou setas | mover |
| Espaço / W | pular |
| Q | alternar passado/futuro |
| E | interagir / examinar |
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
| `lever.png` | alavanca `L` |
| `lever_pulled.png` | (opcional) alavanca puxada |
| `door.png` | porta fechada `D` |
| `door_open.png` | (opcional) porta aberta |
| `sapling.png` | muda plantada `S` |

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
| `salao_past.png` | Salão do Relógio (passado) |
| `salao_future.png` | Salão do Relógio (futuro, opcional) |

Inspector do fundo: Sprite (2D and UI), Mode `Single`, Filter `Point` (ou Bilinear se a arte for grande), Compression `None`.

Sugestão tiles/props: canvas **32×32** (porta pode ser **32×64**). Filter Mode `Point`, Compression `None`.

## Criando fases

As fases ficam em `Assets/Scripts/Level/LevelData.cs`, desenhadas em texto (ASCII). A legenda está no comentário de `LevelDefinition`.
