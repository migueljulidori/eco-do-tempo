using UnityEngine;

namespace EcoDoTempo
{
    public class InspectNote
    {
        public string Past;
        public string Future;
        public InspectNote(string past, string future) { Past = past; Future = future; }
    }

    public class LevelDefinition
    {
        public string Name;
        public string[] Intro;
        public Era StartEra = Era.Past;
        // When > 0 the era flips by itself every N seconds and Q is disabled.
        public float AutoSwitchInterval;
        public InspectNote[] Notes = new InspectNote[0];
        // Resource key under Resources/Backgrounds/{key}_past and {key}_future.
        public string Background;
        // "garden" uses the garden floor, wall and soil tiles. Empty keeps the hall set.
        public string Tileset;
        // Map legend (top row first):
        //  #  wall in both eras          p  block only in the past     f  block only in the future
        //  ^  spikes                     @  player start               D  exit door
        //  *  melody fragment (both)     1  fragment only in the past  2  fragment only in the future
        //  L/G  lever + gate channel 0   A/a channel 1   B/b channel 2
        //  S  soil: plant in the past, a tree grows in the future
        //  i  inspectable object (texts come from Notes, in reading order)
        public string[] Map;
    }

    public static class LevelData
    {
        public const int FragmentsPerLevel = 3;

        // MIDI notes of the "Canção do Eco", one per fragment.
        static readonly int[] Melody = { 72, 76, 79, 76, 74, 77, 81, 77, 79, 83, 86, 84 };

        public static int MelodyLength => Melody.Length;

        public static float MelodyNote(int index) =>
            440f * Mathf.Pow(2f, (Melody[Mathf.Clamp(index, 0, Melody.Length - 1)] - 69) / 12f);

        public static readonly string[] Intro =
        {
            "Na vila de Ressonância, o Grande Relógio tocava a Canção do Eco a cada amanhecer. Diziam que era ela que mantinha o passado e o futuro em seus lugares.",
            "Na noite em que a torre ruiu, a melodia se partiu em doze fragmentos, espalhados entre as eras.",
            "Você é Lia, aprendiz do velho relojoeiro. Com o diapasão que ele lhe deixou, você consegue ouvir o eco de outros tempos... e atravessá-los.",
            "Reúna os fragmentos. Faça o relógio tocar outra vez.",
        };

        public static readonly string[] Ending =
        {
            "Os doze fragmentos se unem em suas mãos. O diapasão vibra, e a Canção do Eco ressoa pela torre.",
            "No passado, o avô sorri ao ouvir a melodia. No futuro, a torre se ergue outra vez.",
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
                    "As ruínas do salão principal. No passado, uma ponte cruzava o fosso.",
                    "Pressione Q para alternar entre PASSADO e FUTURO.\nObjetos de outra era aparecem como ecos translúcidos.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Um cartaz novo: \"Hoje, ao amanhecer, o Grande Relógio tocará a Canção do Eco para toda a vila.\"",
                        "O mesmo cartaz, desbotado e rasgado. Só se lê: \"...a Canção do Eco... nunca mais...\""),
                },
                // Fosso: ponte de caixas no passado. Pilha 2×3 trava a porta (pulo ~2.6u).
                // No futuro a pilha some — cruze no passado, passe no futuro.
                Map = new[]
                {
                    "..............................",
                    "..............................",
                    "......................pp......",
                    ".......*......1.......pp.2....",
                    "..@..i................pp...D..",
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
                    "Três canteiros marcados na terra. Plante cada um no passado.",
                    "No futuro as árvores sobem em degraus do centro até a plataforma à direita.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Diário do avô: \"Plantei aqui uma semente de carvalho. Lia, um dia ela será mais alta que nós dois.\"",
                        "Uma placa coberta de musgo: \"Jardim do Relojoeiro. Fechado desde a queda da torre.\""),
                },
                // Plataforma na direita, 5 blocos acima do chão.
                // Canteiros S do centro para a direita: árvores de 2, 3 e 4 blocos.
                Map = new[]
                {
                    "..............................",
                    "..............................",
                    ".......................2...D..",
                    ".....................#########",
                    "..............................",
                    "..............................",
                    "..............................",
                    "@..1.i.*......................",
                    "############S##S##S###########",
                },
            },
            new LevelDefinition
            {
                Name = "A Oficina",
                Background = "oficina",
                Intro = new[]
                {
                    "A oficina é uma sala só. O poço, a forja e a saída estão abertos no chão.",
                    "Madeira no passado, metal no futuro, pedra nos dois. O que você abre no passado continua aberto.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Bilhete na tábua: \"Troque de era aqui e caia no poço. A alavanca lá embaixo abre a forja, mas só gira no passado.\"",
                        "A tábua sumiu. A escada de metal sobe do poço. A forja, à direita, é uma ponte deste tempo."),
                    new InspectNote(
                        "Na parede: \"A porta de madeira ainda fecha o corredor. Atrás dela, a segunda alavanca abre o vão da saída.\"",
                        "A porta apodreceu. O vão da saída só tem chão de metal agora. Quem pisa no vazio cai nos espinhos."),
                },
                // Fundo 2900×1154, lábio no pixel 664. 50 px = 1 coluna.
                // Poço 14–17 (4 blocos, 7 de fundo). Forja 38–43 (6, brasas a 2).
                // Porta pintada ~45. Vão da saída 54–55 (2).
                Map = new[]
                {
                    "..........................................................",
                    ".....................................#.......pp......#....",
                    ".............................................pp......###..",
                    "..@.i.................i..............G.......pp..1A..a.*.D",
                    "##############pppp####################ffffff##########ff##",
                    ".............#...f#.......................................",
                    ".............#..f.#...................^^^^^^..........^^..",
                    ".............#...f#...................######..........##..",
                    ".............#..f.#.......................................",
                    ".............#...f#.......................................",
                    ".............#2Lf.#.......................................",
                    ".............######.......................................",
                    "..........................................................",
                },
            },
            new LevelDefinition
            {
                Name = "Ecos Repetidos",
                Intro = new[]
                {
                    "Esta sala... você já esteve aqui? O tempo está instável e oscila sozinho.",
                    "Q não funciona aqui. Observe a barra de ritmo e salte no momento certo.",
                },
                AutoSwitchInterval = 2.5f,
                Notes = new[]
                {
                    new InspectNote(
                        "Rabiscado na parede: \"O tempo aqui não espera ninguém. Ouça o ritmo e salte com ele.\"",
                        "Sobre o rabisco antigo, outra letra, igual à sua: \"Eu já estive aqui. Ou estarei.\""),
                },
                Map = new[]
                {
                    "..............................",
                    "..............................",
                    "..............................",
                    "..............*...............",
                    ".................1.....2......",
                    ".@i.........................D.",
                    "###ppppp#fffff#ppppp#fffff####",
                    "...^^^^^.^^^^^.^^^^^.^^^^^....",
                    "...#####.#####.#####.#####....",
                },
            },
        };
    }
}
