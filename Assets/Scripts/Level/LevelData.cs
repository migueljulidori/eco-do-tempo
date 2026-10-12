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
        //  L  lever (works in the past)  G  gate opened by the lever
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
                    "O jardim onde o avô passava as tardes. Algumas coisas levam anos para crescer...",
                    "Use E para interagir com objetos.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Diário do avô: \"Plantei aqui uma semente de carvalho. Lia, um dia ela será mais alta que nós dois.\"",
                        "Uma placa coberta de musgo: \"Jardim do Relojoeiro. Fechado desde a queda da torre.\""),
                },
                // Sem teto nem muro. O tronco (depois de plantar) é o degrau até o terraço.
                Map = new[]
                {
                    "......................2..*..D.",
                    "...................###########",
                    ".1.........@....i.............",
                    "##################S...........",
                },
            },
            new LevelDefinition
            {
                Name = "A Oficina",
                Intro = new[]
                {
                    "A oficina do relojoeiro. Uma comporta pesada bloqueia a saída.",
                    "O que você muda no passado permanece no futuro.",
                },
                Notes = new[]
                {
                    new InspectNote(
                        "Uma bancada cheia de engrenagens. Um bilhete: \"A alavanca abre a comporta da torre. Cuide para que nunca enferruje.\"",
                        "A bancada apodreceu. Entre as engrenagens, um desenho infantil: Lia e o avô em frente ao relógio."),
                },
                Map = new[]
                {
                    "..................................",
                    ".................p..........G.....",
                    ".................p......1...G.....",
                    ".................p..........G.....",
                    ".................p......p...G.....",
                    ".................p..........G.....",
                    "..........2......p....p.....G.....",
                    "..@.i............p...L......G.*.D.",
                    "########ffffff####################",
                    "........^^^^^^....................",
                    "........######....................",
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
