using UnityEngine;

namespace EcoDoTempo
{
    public struct BuildResult
    {
        public Vector2 Spawn;
        public Vector2Int Size;
        public int FragmentCount;
    }

    // Turns the ASCII maps in LevelData into GameObjects. One map cell = one world unit.
    public static class LevelBuilder
    {
        public static BuildResult Build(LevelDefinition level, Transform root)
        {
            var result = new BuildResult();
            int height = level.Map.Length;
            int width = 0;
            foreach (var row in level.Map) width = Mathf.Max(width, row.Length);
            result.Size = new Vector2Int(width, height);

            int noteIndex = 0;
            for (int row = 0; row < height; row++)
            {
                string line = level.Map[row];
                for (int col = 0; col < line.Length; col++)
                {
                    var pos = new Vector2(col, height - 1 - row);
                    switch (line[col])
                    {
                        case '#': Block(root, pos, EraPresence.Both, Palette.PastWall, Palette.FutureWall); break;
                        case 'p': Block(root, pos, EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock); break;
                        case 'f': Block(root, pos, EraPresence.FutureOnly, Palette.FutureBlock, Palette.FutureBlock); break;
                        case '^': Spikes(root, pos); break;
                        case '@': result.Spawn = pos; break;
                        case '*': Fragment(root, pos, EraPresence.Both, result.FragmentCount++); break;
                        case '1': Fragment(root, pos, EraPresence.PastOnly, result.FragmentCount++); break;
                        case '2': Fragment(root, pos, EraPresence.FutureOnly, result.FragmentCount++); break;
                        case 'D': ExitDoor.Create(root, pos); break;
                        case 'L': Lever.Create(root, pos); break;
                        case 'G': Gate.Create(root, pos); break;
                        case 'S': SeedSpot.Create(root, pos); break;
                        case 'i':
                            var note = noteIndex < level.Notes.Length
                                ? level.Notes[noteIndex]
                                : new InspectNote("Nada de especial.", "Nada de especial.");
                            noteIndex++;
                            Inspectable.Create(root, pos, note);
                            break;
                    }
                }
            }
            return result;
        }

        public static TimeObject Block(Transform root, Vector2 pos, EraPresence presence, Color past, Color future)
        {
            var go = SpriteFactory.CreateObject("Block", root, pos, Vector2.one, SpriteFactory.Square, past);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return TimeObject.Add(go, presence, past, future, solid: true);
        }

        static void Spikes(Transform root, Vector2 pos)
        {
            var go = SpriteFactory.CreateObject("Spikes", root, pos, Vector2.one, SpriteFactory.Triangle, Palette.PastSpikes);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.5f);
            col.offset = new Vector2(0f, -0.2f);
            go.AddComponent<Hazard>();
            TimeObject.Add(go, EraPresence.Both, Palette.PastSpikes, Palette.FutureSpikes, solid: false);
        }

        static void Fragment(Transform root, Vector2 pos, EraPresence presence, int order)
        {
            var color = presence == EraPresence.FutureOnly ? Palette.FutureFragment : Palette.Fragment;
            var go = SpriteFactory.CreateObject("Fragment", root, pos, Vector2.one * 0.55f, SpriteFactory.Diamond, color, 5);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;
            go.AddComponent<MelodyFragment>().Order = order;
            TimeObject.Add(go, presence, Palette.Fragment, Palette.FutureFragment, solid: false);
        }
    }
}
