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

## Arte da Lia (sprite sheet)

1. Exporte do LibreSprite como **Horizontal Strip** (`lia_walk.png`).
2. Coloque em `Assets/Resources/Characters/lia_walk.png` (nome exato).
3. Na Unity, Inspector do PNG:
   - Texture Type: `Sprite (2D and UI)`
   - Sprite Mode: `Multiple`
   - Filter Mode: `Point (no filter)`
   - Compression: `None`
   - Pixels Per Unit: tamanho de um frame (ex.: `32`)
4. Sprite Editor → Slice → Grid By Cell Size (ex.: 32×32) → Apply.
5. Play: a Lia usa a animação ao andar; sem o arquivo, continua o boneco simples.

## Criando fases

As fases ficam em `Assets/Scripts/Level/LevelData.cs`, desenhadas em texto (ASCII). A legenda está no comentário de `LevelDefinition`.
