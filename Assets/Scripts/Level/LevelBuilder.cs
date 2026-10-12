using System.Collections.Generic;
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

            var bridgeCells = new HashSet<Vector2Int>();
            PlaceBridgeRuns(level.Map, root, height, bridgeCells);

            int noteIndex = 0;
            for (int row = 0; row < height; row++)
            {
                string line = level.Map[row];
                for (int col = 0; col < line.Length; col++)
                {
                    var pos = new Vector2(col, height - 1 - row);
                    char cell = line[col];

                    // Horizontal bridge runs already created colliders + bridge art.
                    if (cell == 'p' && bridgeCells.Contains(new Vector2Int(col, row)))
                        continue;

                    switch (cell)
                    {
                        case '#':
                            Block(root, pos, EraPresence.Both, Palette.PastWall, Palette.FutureWall,
                                SolidArt(level.Map, row, col, pos, height));
                            break;
                        case 'p':
                            Block(root, pos, EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock,
                                ArtCatalog.PickPastBlock(pos));
                            break;
                        case 'f':
                            Block(root, pos, EraPresence.FutureOnly, Palette.FutureBlock, Palette.FutureBlock,
                                ArtCatalog.PickFutureBlock(pos));
                            break;
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

        // Long horizontal 'p' runs become one bridge_past span + wood deck colliders.
        static void PlaceBridgeRuns(string[] map, Transform root, int height, HashSet<Vector2Int> claimed)
        {
            if (!ArtCatalog.Has("Tiles/bridge_past")) return;

            for (int row = 0; row < height; row++)
            {
                string line = map[row];
                int col = 0;
                while (col < line.Length)
                {
                    if (line[col] != 'p') { col++; continue; }
                    int start = col;
                    while (col < line.Length && line[col] == 'p') col++;
                    int length = col - start;
                    // Need a real span; short pillars stay as wood/stone tiles.
                    if (length < 3) continue;

                    float worldY = height - 1 - row;
                    float deckTop = worldY + 0.5f;

                    for (int x = start; x < start + length; x++)
                    {
                        claimed.Add(new Vector2Int(x, row));
                        var support = Block(root, new Vector2(x, worldY), EraPresence.PastOnly,
                            Palette.PastBlock, Palette.PastBlock, "Tiles/block_wood_past");
                        var sr = support.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.enabled = false;

                        // Collider only on the deck thickness (design: tablado).
                        var col2d = support.GetComponent<BoxCollider2D>();
                        if (col2d != null)
                        {
                            col2d.size = new Vector2(1f, 0.35f);
                            col2d.offset = new Vector2(0f, 0.325f);
                        }
                    }

                    float centerX = start + (length - 1) * 0.5f;
                    // Native bridge is 4×1 units; scale width to the run, keep aspect.
                    var bridge = ArtCatalog.Create("Bridge", root, new Vector2(centerX, worldY),
                        new Vector2(length, 1f), SpriteFactory.Square, Palette.PastBlock, 1,
                        "Tiles/bridge_past", new Vector2(length, 0f));
                    // Art has ~10px empty pad above the deck on a 32px canvas — put deck on collider top.
                    var bridgeSr = bridge.GetComponent<SpriteRenderer>();
                    if (bridgeSr != null && bridgeSr.sprite != null)
                    {
                        const float topPad = 10f / 32f;
                        float h = bridgeSr.bounds.size.y;
                        ArtCatalog.SnapBottomTo(bridge, deckTop - h * (1f - topPad), lift: 0f);
                    }
                    TimeObject.Add(bridge, EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock, solid: false);
                }
            }
        }

        static bool IsSolidChar(char c) =>
            c == '#' || c == 'p' || c == 'f' || c == 'G' || c == 'S';

        static string SolidArt(string[] map, int row, int col, Vector2 pos, int height)
        {
            char above = '#';
            if (row > 0 && col < map[row - 1].Length) above = map[row - 1][col];
            else if (row == 0) above = '#';

            bool airAbove = !IsSolidChar(above);
            if (airAbove) return ArtCatalog.PickFloor(pos);
            return ArtCatalog.PickWall(pos);
        }

        public static TimeObject Block(Transform root, Vector2 pos, EraPresence presence, Color past, Color future,
            string artPath = null)
        {
            if (string.IsNullOrEmpty(artPath))
            {
                artPath = presence == EraPresence.FutureOnly ? "Tiles/block_future"
                        : presence == EraPresence.PastOnly ? "Tiles/block_past"
                        : "Tiles/wall";
            }

            var go = ArtCatalog.Create("Block", root, pos, Vector2.one, SpriteFactory.Square, past, 0,
                artPath, Vector2.one);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return TimeObject.Add(go, presence, past, future, solid: true);
        }

        static void Spikes(Transform root, Vector2 pos)
        {
            var go = ArtCatalog.Create("Spikes", root, pos, Vector2.one, SpriteFactory.Triangle, Palette.PastSpikes, 2,
                "Props/spikes", Vector2.one);
            if (ArtCatalog.Has("Props/spikes"))
                ArtCatalog.SnapBottomTo(go, ArtCatalog.FloorTop(pos));
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.85f, 0.5f);
            col.offset = new Vector2(0f, 0.15f);
            go.AddComponent<Hazard>();
            TimeObject.Add(go, EraPresence.Both, Palette.PastSpikes, Palette.FutureSpikes, solid: false);
        }

        static void Fragment(Transform root, Vector2 pos, EraPresence presence, int order)
        {
            var color = presence == EraPresence.FutureOnly ? Palette.FutureFragment : Palette.Fragment;
            string art = presence == EraPresence.FutureOnly ? "Props/fragment_future" : "Props/fragment";
            var go = ArtCatalog.Create("Fragment", root, pos, Vector2.one * 0.55f, SpriteFactory.Diamond, color, 5,
                art, Vector2.one * 0.85f);
            if (!ArtCatalog.Has(art)) ArtCatalog.TryApply(go, "Props/fragment", Vector2.one * 0.85f);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            var frag = go.AddComponent<MelodyFragment>();
            frag.Order = order;
            frag.UseCustomArt = ArtCatalog.Has("Props/fragment") || ArtCatalog.Has(art);
            TimeObject.Add(go, presence, Palette.Fragment, Palette.FutureFragment, solid: false);
        }
    }
}
