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

            var claimed = new HashSet<Vector2Int>();
            PlaceBridgeRuns(level.Map, root, height, claimed);
            PlaceCratePiles(level.Map, root, height, claimed);

            int noteIndex = 0;
            for (int row = 0; row < height; row++)
            {
                string line = level.Map[row];
                for (int col = 0; col < line.Length; col++)
                {
                    var pos = new Vector2(col, height - 1 - row);
                    char cell = line[col];

                    if (claimed.Contains(new Vector2Int(col, row)))
                        continue;

                    switch (cell)
                    {
                        case '#':
                            PlaceSolid(root, pos, level.Map, row, col, height);
                            break;
                        case 'p':
                            // Lone past block → wood crate.
                            PlaceCrate(root, pos);
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

        static void PlaceSolid(Transform root, Vector2 pos, string[] map, int row, int col, int height)
        {
            bool airAbove = !IsSolidChar(CharAt(map, row - 1, col, height));
            string past;
            string future;
            if (airAbove) ArtCatalog.PickFloorPair(pos, out past, out future);
            else ArtCatalog.PickWallPair(pos, out past, out future);

            var block = Block(root, pos, EraPresence.Both, Palette.PastWall, Palette.FutureWall, past);
            ArtCatalog.SetEraArt(block.gameObject, past, future, Vector2.one);
        }

        // Horizontal 'p' runs (3+) → wood crates on the grid (perfect ledge align) + optional bridge trim.
        static void PlaceBridgeRuns(string[] map, Transform root, int height, HashSet<Vector2Int> claimed)
        {
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
                    if (length < 3) continue;

                    float worldY = height - 1 - row;
                    float centerX = start + (length - 1) * 0.5f;

                    for (int x = start; x < start + length; x++)
                    {
                        claimed.Add(new Vector2Int(x, row));
                        // Same placement as stone floors — cannot sink into the pit.
                        var plank = PlaceCrate(root, new Vector2(x, worldY));
                        var sr = plank.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.sortingOrder = 1;
                    }

                    // Decorative bridge strip on top of the crate row (non-solid).
                    if (ArtCatalog.Has("Tiles/bridge_past"))
                    {
                        var bridge = ArtCatalog.Create("BridgeTrim", root, new Vector2(centerX, worldY),
                            new Vector2(length, 1f), SpriteFactory.Square, Palette.PastBlock, 2,
                            "Tiles/bridge_past", new Vector2(length, 1f));
                        // Lock to the same cell center as the wood — no pivot snapping.
                        bridge.transform.position = new Vector3(centerX, worldY, 0f);
                        ArtCatalog.SetEraArt(bridge, "Tiles/bridge_past", "Tiles/bridge_past", new Vector2(length, 1f));
                        TimeObject.Add(bridge, EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock, solid: false);
                    }
                }
            }
        }

        // Compact clusters of 'p' (2+ tall) → pile of wood crates that actually blocks the path.
        static void PlaceCratePiles(string[] map, Transform root, int height, HashSet<Vector2Int> claimed)
        {
            int width = 0;
            foreach (var line in map) width = Mathf.Max(width, line.Length);

            var visited = new HashSet<Vector2Int>();

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    var key = new Vector2Int(col, row);
                    if (claimed.Contains(key) || visited.Contains(key)) continue;
                    if (CharAt(map, row, col, height) != 'p') continue;

                    // Flood-fill this connected past-block blob.
                    var cells = new List<Vector2Int>();
                    var stack = new Stack<Vector2Int>();
                    stack.Push(key);
                    visited.Add(key);
                    while (stack.Count > 0)
                    {
                        var c = stack.Pop();
                        if (claimed.Contains(c)) continue;
                        if (CharAt(map, c.y, c.x, height) != 'p') continue;
                        cells.Add(c);
                        TryPush(c.x + 1, c.y);
                        TryPush(c.x - 1, c.y);
                        TryPush(c.x, c.y + 1);
                        TryPush(c.x, c.y - 1);

                        void TryPush(int x, int y)
                        {
                            var n = new Vector2Int(x, y);
                            if (x < 0 || y < 0 || y >= height || x >= width) return;
                            if (visited.Contains(n) || claimed.Contains(n)) return;
                            if (CharAt(map, y, x, height) != 'p') return;
                            visited.Add(n);
                            stack.Push(n);
                        }
                    }

                    if (cells.Count == 0) continue;

                    int minY = int.MaxValue, maxY = int.MinValue;
                    foreach (var c in cells)
                    {
                        if (c.y < minY) minY = c.y;
                        if (c.y > maxY) maxY = c.y;
                    }
                    int stackHeight = maxY - minY + 1;

                    // Single cells / short rows stay for the main loop; only real piles here.
                    if (cells.Count < 2 || stackHeight < 2) continue;

                    foreach (var c in cells)
                    {
                        claimed.Add(c);
                        float worldY = height - 1 - c.y;
                        var crate = PlaceCrate(root, new Vector2(c.x, worldY));
                        var sr = crate.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.sortingOrder = 3;
                    }
                }
            }
        }

        static TimeObject PlaceCrate(Transform root, Vector2 pos)
        {
            string art = ArtCatalog.FirstExisting(
                "Tiles/block_wood_past", "Tiles/block_past", "Tiles/block_stone_past");
            var block = Block(root, pos, EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock, art);
            // Slight visual variety so a pile reads as stacked crates, not one slab.
            var sr = block.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                int h = (Mathf.RoundToInt(pos.x) * 13 + Mathf.RoundToInt(pos.y) * 7) & 3;
                sr.flipX = (h & 1) == 0;
                // Tiny brightness jitter via color (still reads as wood).
                float shade = 0.92f + (h * 0.03f);
                sr.color = new Color(shade, shade, shade, 1f);
            }
            return block;
        }

        static char CharAt(string[] map, int row, int col, int height)
        {
            if (row < 0 || row >= height || col < 0) return '.';
            string line = map[row];
            if (col >= line.Length) return '.';
            return line[col];
        }

        static bool IsSolidChar(char c) =>
            c == '#' || c == 'p' || c == 'f' || c == 'G' || c == 'S';

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
            // Always center-aligned 1×1 on the grid — no SnapBottom/Top.
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = Vector3.one;
            if (!string.IsNullOrEmpty(artPath))
                ArtCatalog.TryApply(go, artPath, Vector2.one);
            ArtCatalog.SetEraArt(go, artPath, artPath, Vector2.one);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return TimeObject.Add(go, presence, past, future, solid: true);
        }

        static void Spikes(Transform root, Vector2 pos)
        {
            var go = ArtCatalog.Create("Spikes", root, pos, Vector2.one, SpriteFactory.Triangle, Palette.PastSpikes, 4,
                "Props/spikes", Vector2.one);
            if (ArtCatalog.Has("Props/spikes"))
                ArtCatalog.SnapBottomTo(go, pos.y - 0.5f, lift: 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.5f);
            col.offset = new Vector2(0f, 0f);
            go.AddComponent<Hazard>();
            ArtCatalog.SetEraArt(go, "Props/spikes", "Props/spikes", Vector2.one);
            TimeObject.Add(go, EraPresence.Both, Palette.PastSpikes, Palette.FutureSpikes, solid: false);
        }

        static void Fragment(Transform root, Vector2 pos, EraPresence presence, int order)
        {
            var color = presence == EraPresence.FutureOnly ? Palette.FutureFragment : Palette.Fragment;
            string art = presence == EraPresence.FutureOnly ? "Props/fragment_future" : "Props/fragment";
            var go = ArtCatalog.Create("Fragment", root, pos, Vector2.one * 0.55f, SpriteFactory.Diamond, color, 5,
                art, Vector2.one * 0.7f);
            if (!ArtCatalog.Has(art)) ArtCatalog.TryApply(go, "Props/fragment", Vector2.one * 0.7f);
            if (ArtCatalog.Has(art) || ArtCatalog.Has("Props/fragment"))
                ArtCatalog.SnapBottomTo(go, pos.y - 0.5f, lift: 0.35f);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            var frag = go.AddComponent<MelodyFragment>();
            frag.Order = order;
            frag.UseCustomArt = ArtCatalog.Has("Props/fragment") || ArtCatalog.Has(art);
            TimeObject.Add(go, presence, Palette.Fragment, Palette.FutureFragment, solid: false);
        }
    }
}
