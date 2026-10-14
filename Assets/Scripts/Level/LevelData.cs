using UnityEngine;

namespace EcoDoTempo
{
    public class InspectNote
    {
        public string Past;
        public string Future;
        public InspectNote(string past, string future) { Past = past; Future = future; }
    }

    // A clock 'h' in the map: turn it to Target in the past to open its channel for good.
    public class ClockPuzzle
    {
        public int Target;
        public int Channel;
        public int Start = 12;
        public ClockPuzzle(int target, int channel, int start = 12) { Target = target; Channel = channel; Start = start; }
    }

    public class LevelDefinition
    {
        public string Name;
        public string[] Intro;
        public Era StartEra = Era.Past;
        // Powers Lia can use here (see Power). New ones are explained in the intro.
        public Power Powers = Power.Era;
        // When > 0 the era flips by itself every N seconds and Q is disabled.
        public float AutoSwitchInterval;
        public InspectNote[] Notes = new InspectNote[0];
        // Clocks 'h', in reading order.
        public ClockPuzzle[] Clocks = new ClockPuzzle[0];
        // Resource key under Resources/Backgrounds/{key}_past and {key}_future.
        public string Background;
        // "garden" uses the garden floor, wall and soil tiles. Empty keeps the hall set.
        public string Tileset;
        // Map legend (top row first, one character = one 1×1 cell):
        //  #  wall in both eras          p  block only in the past     f  block only in the future
        //  !  invisible wall (only where the painted background clearly shows a wall)
        //  ^  spikes (both eras)         ~  forge fire (past only)     @  player start   D  exit door
        //  *  melody fragment (both)     1  fragment only in the past  2  fragment only in the future
        //  S  soil: plant in the past, a tree grows in the future (heights 2, 3, 4... in reading order)
        //  i  inspectable object (texts come from Notes, in reading order)
        //  c  iron crate: pushed only in the past, rusted in place in the future
        //  h  clock lock (settings come from Clocks, in reading order)
        //  Channels — a lever or clock opens its gates for good; plates hold them open while pressed
        //  (a channel with several plates needs all of them pressed at once):
        //    channel 0: lever L, plate x, gate G    channel 1: lever A, plate y, gate a
        //    channel 2: lever B, plate z, gate b
        //  Moving parts ping-pong along a track next to them: '=' sideways, '|' up or down.
        //    m  platform that moves only in the past (rusts where it stopped)
        //    M  platform that moves in every era      v  moving blade (deadly, every era)
        //  Fermata (C) stops every moving part, the era rhythm and falling crates.
        public string[] Map;
    }

    public static class LevelData
    {
        public const int FragmentsPerLevel = 3;

        // MIDI notes of the "Canção do Eco", one per fragment (6 levels × 3).
        static readonly int[] Melody =
        {
            72, 76, 79, 76, 74, 77, 81, 77, 79, 83, 86, 84, 81, 77, 79, 76, 74, 72,
        };

        public static int MelodyLength => Melody.Length;

        public static float MelodyNote(int index) =>
            440f * Mathf.Pow(2f, (Melody[Mathf.Clamp(index, 0, Melody.Length - 1)] - 69) / 12f);

        public static readonly string[] Intro =
        {
            "Na vila de Ressonância, o Grande Relógio tocava a Canção do Eco a cada amanhecer. Diziam que era ela que mantinha o passado e o futuro em seus lugares.",
            "Na noite em que a torre ruiu, a melodia se partiu em dezoito fragmentos, espalhados entre as eras.",
            "Você é Lia, aprendiz do velho relojoeiro. Com o diapasão que ele lhe deixou, você consegue ouvir o eco de outros tempos... e atravessá-los.",
            "Quanto mais notas o diapasão ouvir, mais ele aprende. Reúna os fragmentos. Faça o relógio tocar outra vez.",
        };

        public static readonly string[] Ending =
        {
            "Os dezoito fragmentos se unem em suas mãos. O diapasão vibra, e a Canção do Eco ressoa pela torre.",
            "No passado, o avô para o trabalho e sorri ao ouvir a melodia. No futuro, a torre se ergue outra vez.",
            "Em algum lugar entre os dois, um eco seu acena e se desfaz.",
            "Passado e futuro, finalmente, em harmonia.\n\nFim... por enquanto.",
        };

        public static readonly LevelDefinition[] Levels =
        {
            new LevelDefinition
            {
                Name = "Salão do Relógio",
                Background = "salao",
                Intro = new[]
                {
                    "As ruínas do salão principal. No passado, uma ponte de tábuas cruzava o fosso.",
                    "Pressione Q para alternar entre PASSADO e FUTURO.\nO que só existe na outra era aparece como um eco translúcido.",
                    "O diapasão responde até no meio de um salto.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Um cartaz novo: \"Hoje, ao amanhecer, o Grande Relógio tocará a Canção do Eco para toda a vila.\"",
                        "O mesmo cartaz, desbotado e rasgado. Só se lê: \"...a Canção do Eco... nunca mais...\" Atrás dele, alguém riscou: \"suba no velho, pouse no novo\"."),
                },
                // Ponte e caixas só no passado. A plataforma alta só existe no futuro:
                // suba nas caixas no passado e troque de era no ar para pousar nela.
                Map = new[]
                {
                    "..............................",
                    "..........................2...",
                    ".........................fff..",
                    ".......*......1......pp.......",
                    "..@..i...............pp.....D.",
                    "############pppppp############",
                    "............^^^^^^............",
                    "............######............",
                },
            },
            new LevelDefinition
            {
                Name = "O Jardim Esquecido",
                Background = "jardim",
                Tileset = "garden",
                Intro = new[]
                {
                    "O jardim do avô. Três canteiros de terra esperam sementes, e o terraço da saída está trancado.",
                    "O que você faz no PASSADO permanece no FUTURO: uma semente vira árvore, uma caixa empurrada fica onde você a deixou.",
                    "Caixas de ferro só deslizam no passado. No futuro, a ferrugem as prende ao chão.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Um bilhete sob o relógio do muro: \"O portão do terraço só destranca quando os ponteiros marcam a hora em que o carvalho foi plantado.\"",
                        "O bilhete virou pó. Os ponteiros enferrujaram. Só no passado eles ainda giram."),
                    new InspectNote(
                        "Diário do avô: \"Plantei o carvalho na manhã em que Lia nasceu. Os sinos da torre bateram sete vezes.\"",
                        "Uma placa coberta de musgo: \"Jardim do Relojoeiro. Fechado desde a queda da torre.\" No alto do muro, à esquerda, uma pedra nova reluz."),
                },
                Clocks = new[] { new ClockPuzzle(7, 0) },
                // Árvores de 2, 3 e 4 blocos levam ao relógio e ao terraço, mas só no futuro.
                // No relógio, troque para o passado para girar os ponteiros (7h abre o portão G).
                // A caixa precisa ser empurrada no passado até perto da pedra do futuro, à esquerda.
                Map = new[]
                {
                    "........................######",
                    "..........................#...",
                    "...............1..............",
                    ".....................*....G.D.",
                    "...............ih.......######",
                    "2..............###............",
                    "f.............................",
                    "..............................",
                    ".@.i...c......................",
                    "#########S###S######S#########",
                },
            },
            new LevelDefinition
            {
                Name = "A Oficina",
                Background = "oficina",
                Intro = new[]
                {
                    "A oficina sob a torre. No passado, as máquinas ainda giram.",
                    "Elevadores e engrenagens só se movem no PASSADO. No FUTURO a ferrugem os trava exatamente onde você os deixou.",
                    "Alavancas só giram no passado, mas o que abrem continua aberto em todas as eras. A cor do losango mostra o que cada uma abre.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Bilhete na tábua: \"Troque de era aqui e caia no poço. A alavanca lá embaixo abre a forja, mas só gira no passado.\"",
                        "A tábua sumiu. No poço, um degrau em cada parede: segure o pulo para chegar ao seguinte. A forja, à direita, é uma ponte deste tempo."),
                    new InspectNote(
                        "Placa do elevador: \"Uma viga de madeira tranca o topo da torre. Ninguém passa para a alavanca de cima.\"",
                        "A viga apodreceu faz tempo. Mas o elevador também parou... e só para onde alguém o deixou no passado."),
                },
                // Fundo 2900×1154, lábio no pixel 664. 50 px = 1 coluna.
                // Poço 13–17, forja 38–43 (brasa no passado, metal no futuro), porta de madeira 47–48, saída 54–56.
                // Torre: elevador 'm' sobe até o nicho de cima no passado; a viga 'p' só existe no passado.
                Map = new[]
                {
                    "...........................p........#...............#.....",
                    "...........................p........#...............#.....",
                    "...........................p..A*....#...............#.....",
                    ".........................|..####....#...............#.....",
                    ".........................|..........#...............#.....",
                    ".........................|..........#...............#.....",
                    ".........................|..........#...............#.....",
                    ".........................|..........#...............#.....",
                    ".........................|..........#...............#.....",
                    ".........................|..........#..........pp...#.....",
                    ".........................|.....................pp.........",
                    "..@.i.................i..mm.........G..........pp.1.a....D",
                    "#############ppppp####################ffffff##########fff#",
                    "............#....f#...................~~~~~~..............",
                    "............#.....#.......................................",
                    "............#.f...#.......................................",
                    "............#.....#.......................................",
                    "............#....f#.......................................",
                    "............#.2L..#...................................^^^.",
                    "............#######...................................###.",
                    "..........................................................",
                },
            },
            new LevelDefinition
            {
                Name = "O Claustro dos Ecos",
                Background = "claustro",
                Powers = Power.Era | Power.Echo,
                Intro = new[]
                {
                    "Um pátio de pedra atrás do jardim. Placas no chão seguram os portões abertos, mas só enquanto alguém pisa nelas.",
                    "O diapasão aprendeu a ECOAR. Aperte F para gravar seus passos. Aperte F de novo: o tempo volta, você retorna ao ponto de partida... e um eco seu repete tudo o que você fez.",
                    "O eco fica parado onde terminou. Ele segura placas, aperta o que você apertou e aguenta o seu peso.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Rabisco no muro: \"Para atravessar sozinha, seja duas.\"",
                        "Embaixo, outra letra, igual à sua: \"Eu já pisei aqui antes de você.\""),
                    new InspectNote(
                        "Duas placas e um portão. Uma caixa de ferro pesa tanto quanto uma pessoa.",
                        "As placas ainda funcionam. A ferrugem, não: no futuro nada se arrasta."),
                },
                // Fundo do claustro: chão contínuo; as paredes ficam sobre os pilares pintados das colunas 8 e 26.
                // x segura G. Os dois y seguram a: um para a caixa, outro para o eco.
                Map = new[]
                {
                    "........#.................#...",
                    "........#.................#...",
                    "........#.................#...",
                    "........#.................#...",
                    "....1...#............*....#...",
                    "........#......2..........#...",
                    "........#.....fff.........#...",
                    "..............................",
                    ".@i.x...G...c.....i.y..y..a..D",
                    "##############################",
                },
            },
            new LevelDefinition
            {
                Name = "Ecos Repetidos",
                Background = "salao",
                Powers = Power.Fermata,
                AutoSwitchInterval = 2.5f,
                Intro = new[]
                {
                    "Esta sala... você já esteve aqui? O tempo está instável e oscila sozinho.",
                    "Q não funciona aqui. Observe a barra de ritmo e salte com ela.",
                    "O diapasão aprendeu a FERMATA. Aperte C para segurar o tempo por alguns segundos: o ritmo, as lâminas e as máquinas param. Só você se move. Aperte C de novo para soltar.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Rabiscado na parede: \"O tempo aqui não espera ninguém. Ouça o ritmo e salte com ele.\"",
                        "Sobre o rabisco antigo, outra letra, igual à sua: \"Eu já estive aqui. Ou estarei. Segure a nota quando faltar chão.\""),
                },
                // A ponte só existe no passado; o caminho alto só no futuro, sobre espinhos.
                Map = new[]
                {
                    "..............................",
                    "..............................",
                    "....................2.........",
                    "........*..........fff.......D",
                    "..................p...ffffff##",
                    "..@i..v====...1....^^^^^^^^^^^",
                    "############pppppp############",
                    "............^^^^^^............",
                    "............######............",
                },
            },
            new LevelDefinition
            {
                Name = "O Coração do Relógio",
                Background = "coracao",
                Powers = Power.All,
                Intro = new[]
                {
                    "De volta à oficina, muitos ecos depois. Aqui embaixo bate o coração do Grande Relógio.",
                    "As engrenagens desta sala giram em TODAS as eras. Só a fermata (C) as faz parar.",
                    "Tudo o que você aprendeu vale aqui: eras (Q), causa e efeito, ecos (F) e fermata (C).",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Esquema do avô: \"A placa do porão segura a comporta da forja. Algo pesado precisa ficar lá embaixo para sempre.\"",
                        "O esquema desbotou. As tábuas novas sobre o porão só existem neste tempo; os degraus lá dentro, só no passado."),
                    new InspectNote(
                        "\"O elevador do coração nunca para. A lâmina do pêndulo guarda a alavanca do topo.\"",
                        "Alguém riscou por cima, com a sua letra: \"Segure a nota. Depois, deixe alguém pisando por você.\""),
                },
                // Fundo 2900×1154, lábio no pixel 664, 50 px = 1 coluna. Vãos pintados: 16–20, 39–44, 53–54.
                // Caixa → porão (placa x segura G). Topo da torre: alavanca B abre b.
                // Placa y antes da forja segura a (precisa do eco). Plataforma M cruza a forja.
                Map = new[]
                {
                    ".....................................#.......#.....#......",
                    "...........................v.........#.......#.....#......",
                    "...........................|.........#.......#.....#......",
                    "...........................|..B*.....#.......#.....#......",
                    ".........................|.|####.....#.......#.....#......",
                    ".........................|.|.........#.......#.....#......",
                    ".........................|...........#.......#.....#......",
                    ".........................|...........#.......#.....#......",
                    ".........................|...........#.......#.....#......",
                    ".........................|...........#.......#.....#......",
                    ".........................|...........#.......#pp...#......",
                    ".........................|...........#........pp..........",
                    "..@.i....c.............i.MM........y.G....2..app.1.b.....D",
                    "################fffff##################MM====########ff###",
                    "...............#....p#....................................",
                    "...............#.....#.................^^^^^^.............",
                    "...............#..p..#.................######.............",
                    "...............#.....#....................................",
                    "...............#....p#....................................",
                    "...............#xxxxx#...............................^^...",
                    "...............#######...............................##...",
                    "..........................................................",
                },
            },
        };
    }
}
