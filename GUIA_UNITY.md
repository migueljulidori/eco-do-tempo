# Guia: atualizar e testar o Eco do Tempo na Unity

Este guia acompanha a reformulação das fases. Lia agora tem três poderes de tempo, e as fases foram refeitas como puzzles de plataforma no estilo *That Level Again* (cada sala tem uma ideia) com a investigação entre eras de *Cube Escape* (pistas espalhadas entre o passado e o futuro).

---

## 1. Trazer as mudanças para o seu computador

No terminal, dentro da pasta do projeto:

```bash
git fetch origin
git checkout claude/hopeful-goldberg-hrhgt2
git pull origin claude/hopeful-goldberg-hrhgt2
```

> Feche a Unity antes do `git pull`. Se ele reclamar de arquivos modificados em `Assets/Scenes`, salve ou descarte a cena antes (`git stash` guarda as mudanças).

## 2. Abrir na Unity

1. Abra o projeto pelo Unity Hub (Unity 6, a mesma versão de antes).
2. Espere a compilação terminar (barra no canto inferior direito).
3. Abra a **Console** (`Window > General > Console`). Ela deve estar sem erros vermelhos.
4. Se aparecer a mensagem *"Removi o objeto antigo Níveis da cena"*, salve a cena com **Ctrl+S**. Isso é esperado: as fases deixaram de ser prefabs.

### O que mudou na Hierarchy

- Antes: `Níveis` com prefabs (`Fase 1`, `Fase 2`...). O jogo usava esses prefabs, e qualquer mudança no `LevelData.cs` era ignorada.
- Agora: aparece **`Níveis (gerados do LevelData)`**. É uma prévia montada direto dos mapas em texto, recriada sozinha depois de cada recompilação e quando você sai do Play. Ela não é salva na cena.
- Para trocar a fase visível: menu **Eco do Tempo > Visualizar fase na Scene**, escolha a fase, a era (Passado/Futuro) e clique **Gerar / atualizar prévia**.
- O prefab que você ajustou na fase 3 foi guardado em `Legacy/Level3_ajustado.prefab` (fora de `Assets`, então a Unity não o carrega). Nada se perdeu.
- Se ainda existirem `Level1.prefab`, `Level2.prefab` ou `Level4.prefab` em `Assets/Resources/Levels` no seu computador, pode apagar a pasta `Levels` inteira: o jogo não lê mais esses arquivos.

## 3. Jogar e testar

Aperte **Play** em qualquer cena. Na tela de título:

- **Enter** começa do início, ou **continua** da última fase alcançada (o progresso é salvo sozinho; **N** começa um jogo novo).
- **1 a 6** abrem uma fase direto (só no editor ou em build de desenvolvimento).
- Também dá pelo Inspector: selecione `EcoDoTempo` durante o Play, menu ⋮ do componente **GameManager** → *Debug > Abrir fase N*.

### Controles

| Tecla | Ação |
| --- | --- |
| A/D ou setas | mover |
| Espaço / W | pular (segure para pular mais alto) |
| E | interagir: examinar, puxar alavanca, plantar, girar relógio |
| **Q** | alternar PASSADO / FUTURO |
| **F** | **Eco**: grava seus passos; F de novo volta no tempo e deixa um eco repetindo tudo |
| **C** | **Fermata**: segura o tempo por 3 s (C de novo solta) |
| R | reiniciar a fase |
| Esc / P | pausar (R reinicia, M volta ao título); nos textos, pula para a última página |
| Enter | avançar textos |

## 4. As regras do tempo (o que o jogador aprende)

Ao morrer, Lia volta ao último chão seguro em que pisou (longe de perigos), na era em que estava ali. R reinicia a fase inteira.

| Regra | Onde aparece |
| --- | --- |
| Objetos só de uma era aparecem como eco translúcido na outra. Dá para trocar de era no ar. | Fase 1 |
| Causa e efeito: o que você faz no passado fica no futuro (semente → árvore, caixa empurrada → caixa enferrujada no mesmo lugar). | Fase 2 |
| Caixas de ferro só deslizam no passado. | Fases 2, 4, 6 |
| Relógio-cadeado: os ponteiros só giram no passado; no futuro ficam onde o passado os deixou. | Fase 2 |
| Máquinas do passado (`m`) param no futuro exatamente onde você as deixou: trocar de era vira um freio. | Fase 3 |
| Alavancas abrem para sempre; placas só seguram enquanto alguém pisa. Losangos da mesma cor ligam alavanca/placa/relógio à comporta. | Fases 3, 4, 6 |
| Eco: grave, volte no tempo, e seu eco pisa nas placas e serve de degrau. | Fases 4, 6 |
| Fermata: congela máquinas, lâminas, caixas caindo e o ritmo das eras. Lia continua se movendo. | Fases 5, 6 |

## 5. Roteiro de teste (com spoilers)

Faça cada fase pelo caminho abaixo. Se algo não funcionar como descrito, anote o número da fase e o passo.

**Fase 1 — Salão do Relógio** (Q)
1. Pegue o fragmento logo depois do cartaz, pulando.
2. No passado, atravesse a ponte e pegue o fragmento dourado sobre ela.
3. Suba nas caixas (só existem no passado). Pule para a direita e aperte Q no ar: você pousa na plataforma do futuro e pega o fragmento azul.

**Fase 2 — O Jardim Esquecido** (Q + causa e efeito + relógio)
1. Leia o diário (E) nas duas eras. Ele diz que o carvalho foi plantado às **7 horas**.
2. No passado, plante as três sementes.
3. Empurre a caixa para a esquerda no passado (pule por cima dela e empurre). Deixe-a perto da pedra que só existe no futuro, lá no alto à esquerda.
4. No futuro, suba na caixa e pule até a pedra: fragmento azul.
5. No futuro, suba as árvores até a borda do relógio. Troque para o passado ali em cima e gire o relógio até 7h. O portão do terraço destranca.
6. Ainda no passado, pule na borda: fragmento dourado.
7. No futuro, da borda vá para a terceira árvore e depois para o terraço (fragmento no meio do salto). Porta.

**Fase 3 — A Oficina** (Q + máquinas do passado + alavancas)
1. No futuro, caia no poço. Pegue o fragmento azul. Troque para o passado perto da alavanca e puxe: a comporta da forja abre. Volte ao futuro e suba pelos degraus.
2. Na torre, suba no elevador no passado. Quando ele chegar lá em cima, troque para o futuro: ele para e a viga some. Passe para a plataforma.
3. Troque para o passado e puxe a alavanca do topo (abre a comporta da saída). Pegue o fragmento.
4. Atravesse a forja no futuro, passe a porta de madeira no futuro, troque para o passado para pegar o fragmento dourado e saia pelo futuro.

**Fase 4 — O Claustro dos Ecos** (Q + Eco)
1. Aperte F, ande até a placa e aperte F de novo. Você volta; seu eco anda até a placa e fica lá. O portão abre.
2. Antes de sair, suba na cabeça do eco (no passado) e pegue o fragmento acima da placa.
3. Use a caixa (ou um eco) como degrau para a plataforma do futuro: fragmento azul.
4. Empurre a caixa (passado) até uma das placas. Grave um eco indo até a outra placa. Fragmento alto entre as placas: suba na caixa. Passe o portão.

**Fase 5 — Ecos Repetidos** (o tempo oscila sozinho + Fermata)
1. Pule a lâmina (ou congele com C) e pegue o fragmento acima dela.
2. Atravesse a ponte quando estiver no passado (pegue o fragmento nela).
3. Suba no degrau do passado à direita. Pule no ritmo para cair no futuro sobre a plataforma alta (fragmento azul). Dica: aperte C para segurar o tempo e solte no momento certo.
4. Corra pelo caminho do futuro sobre os espinhos até a porta. Use C se o futuro estiver acabando.

**Fase 6 — O Coração do Relógio** (tudo)
1. No passado, empurre a caixa para dentro do porão (as tábuas sobre ele só existem no futuro). Ela cai na placa e segura a comporta da forja. Se você cair junto, os degraus para sair só existem no passado.
2. Atravesse o porão pelo futuro.
3. Elevador da torre: suba, espere a lâmina se afastar (ou congele com C) e pule para o topo. Troque para o passado e puxe a alavanca. Fragmento.
4. Grave um eco indo até a placa antes da forja. Volte no tempo; o eco segura a comporta depois da forja.
5. Pegue a plataforma da forja **no futuro** para apanhar o fragmento azul.
6. Porta de madeira no futuro, fragmento dourado no passado, saída no futuro.

## 6. Editar as fases

Os mapas ficam em `Assets/Scripts/Level/LevelData.cs`, desenhados em texto. A legenda completa está no comentário de `LevelDefinition`. Resumo dos símbolos novos:

| Símbolo | O que é |
| --- | --- |
| `c` | caixa de ferro (desliza só no passado) |
| `x` `y` `z` | placas de pressão dos canais 0, 1, 2 (letras vizinhas = uma placa larga) |
| `h` | relógio-cadeado (hora certa e canal em `Clocks`) |
| `m` + trilho | plataforma que só se move no passado |
| `M` + trilho | plataforma que se move em todas as eras |
| `v` + trilho | lâmina que se move em todas as eras |
| `=` / `\|` | trilho horizontal / vertical, encostado na peça |
| `~` | brasa da forja (mata só no passado) |
| `!` | parede invisível: o pilar ou parede pintado no fundo é o que aparece |

Canais: alavanca `L` / placa `x` / comporta `G` (canal 0), `A`/`y`/`a` (canal 1), `B`/`z`/`b` (canal 2).

Cada fase define seus poderes em `Powers` (por exemplo `Power.Era | Power.Echo`).

Depois de editar, confira os mapas sem abrir a Unity:

```bash
python3 tools/check-levels.py
```

Ele aponta linhas de tamanhos diferentes, fragmentos faltando, comportas sem alavanca e trilhos ausentes.

Para ver o mapa desenhado por cima do fundo, nas duas eras (também sem a Unity):

```bash
python3 tools/render-levels.py
```

As imagens vão para a pasta `Previas/`, desenhadas com a arte do jogo (o que só existe na outra era aparece translúcido). Com `--esquema`, o mapa aparece em caixas coloridas com rótulos, e a linha vermelha mostra o chão em que o fundo foi alinhado.

### Alinhamento com os fundos pintados

- Salão (`salao`, fases 1 e 5): o fosso pintado ocupa as colunas **12–17**. Mantenha o chão vazado só ali.
- Oficina (`oficina`, fase 3): 58 colunas; vãos do chão em **13–17**, **38–43** e **54–56**; arcos das comportas nas colunas 36 e 52; porta de madeira em 47–48.
- Coração (`coracao`, fase 6): 58 colunas; vãos em **16–20**, **39–44** e **53–54**.
- Jardim (`jardim`, fase 2): chão contínuo. O relógio pintado no muro fica perto da coluna 16, e o relógio do jogo foi posto por cima dele.
- Claustro (`claustro`, fase 4): chão contínuo; pilares pintados nas colunas 2, 8, 14, 20 e 26 (as paredes da fase usam 8 e 26).

## 7. Arte

Toda a arte da lista já está no projeto, com as configurações de importação prontas (arquivos `.meta`): andar da Lia, caixa, lâmina, brasa, placas, mostrador, plataformas e os fundos do Claustro e do Coração.

**Falta um arquivo:** `Backgrounds/claustro_past.png` chegou cortado (a imagem para na linha 131 de 576). Enquanto ele não vem, o passado do Claustro usa o desenho do futuro com um filtro quente. Para corrigir, gere de novo o claustro do passado (prompt 8a em [PROMPTS_ARTE.md](PROMPTS_ARTE.md)) e salve em `Assets/Resources/Backgrounds/claustro_past.png`; o jogo troca sozinho.

Para pedir mais arte, use os prompts e o script de [PROMPTS_ARTE.md](PROMPTS_ARTE.md).

## 8. Se algo der errado

- **Erro vermelho na Console ao abrir**: copie a primeira linha vermelha e me mande.
- **A fase não aparece na Hierarchy**: menu **Eco do Tempo > Recriar fases na Hierarchy**.
- **A fase no Play é diferente do mapa**: confira se não sobrou um objeto `Níveis` antigo na cena e apague-o.
- **Algo da física parece errado** (caixa não empurra, elevador treme, eco não segura a placa): diga a fase e o passo do roteiro acima.
