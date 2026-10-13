using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    public struct BuildResult
    {
        public Vector2 Spawn;
        public Vector2Int Size;
        public int FragmentCount;
        // World Y of the walkable floor top (aligned to the painted background ledge).
        public float FloorTopY;
    }

    // Turns the ASCII maps in LevelData into GameObjects. One map cell = one world unit.
    public static class LevelBuilder
    {
        // Raises the whole stage so tiles/props sit on the painted floor of salao_past.
        public const float SceneYAlign = 0.12f;

        struct BridgeSpan
        {
            public int Row, Start, Length;
        }

        static string tileset;
        static int plantOrdinal;
        static readonly System.Collections.Generic.HashSet<int> plantColumns = new System.Collections.Generic.HashSet<int>();

        static Vector2 Cell(int col, int row, int height) =>
            new Vector2(col, height - 1 - row + SceneYAlign);

        public static BuildResult Build(LevelDefinition level, Transform root)
        {
            tileset = level.Tileset;
            plantOrdinal = 0;
            plantColumns.Clear();
            foreach (var line in level.Map)
                for (int c = 0; c < line.Length; c++)
                    if (line[c] == 'S') plantColumns.Add(c);
            var result = new BuildResult();
            int height = level.Map.Length;
            int width = 0;
            foreach (var row in level.Map) width = Mathf.Max(width, row.Length);
            result.Size = new Vector2Int(width, height);

            var claimed = new HashSet<Vector2Int>();
            var spans = FindBridgeSpans(level.Map, height, claimed);
            PlaceCratePiles(level.Map, root, height, claimed);

            int noteIndex = 0;
            for (int row = 0; row < height; row++)
            {
                string line = level.Map[row];
                for (int col = 0; col < line.Length; col++)
                {
                    var pos = Cell(col, row, height);
                    char cell = line[col];

                    if (claimed.Contains(new Vector2Int(col, row)))
                        continue;

                    switch (cell)
                    {
                        case '#':
                            PlaceSolid(root, pos, level.Map, row, col, height);
                            break;
                        case 'p':
                            PlaceCrate(root, pos);
                            break;
                        case 'f':
                            Block(root, pos, EraPresence.FutureOnly, Palette.FutureBlock, Palette.FutureBlock,
                                tileset == "garden"
                                    ? ArtCatalog.FirstExisting("Tiles/wall_garden_future", "Tiles/floor_garden_future", "Tiles/block_stone_future")
                                    : ArtCatalog.PickFutureBlock(pos));
                            break;
                        case '^': Spikes(root, pos); break;
                        case '@': result.Spawn = pos; break;
                        case '*': Fragment(root, pos, EraPresence.Both, result.FragmentCount++); break;
                        case '1': Fragment(root, pos, EraPresence.PastOnly, result.FragmentCount++); break;
                        case '2': Fragment(root, pos, EraPresence.FutureOnly, result.FragmentCount++); break;
                        case 'D': ExitDoor.Create(root, pos); break;
                        case 'L': Lever.Create(root, pos); break;
                        case 'G': Gate.Create(root, pos); break;
                        case 'S':
                            SeedSpot.Create(root, pos, 2 + plantOrdinal);
                            plantOrdinal++;
                            break;
                        case 'i':
                            var note = noteIndex < level.Notes.Length
                                ? level.Notes[noteIndex]
                                : new InspectNote("Nada de especial.", "Nada de especial.");
                            noteIndex++;
                            Inspectable.Create(root, pos, note, tileset == "garden");
                            break;
                    }
                }
            }

            PlaceBridgeSpans(spans, root, height);
            PlaceLevelBounds(root, width, height);
            // Top of the solid under Lia — not the open tops of the side walls.
            result.FloorTopY = result.Spawn.y - 0.5f;
            return result;
        }

        // Invisible walls so Lia cannot leave the left or right edge of the map.
        static void PlaceLevelBounds(Transform root, int width, int height)
        {
            float midY = (height - 1) * 0.5f + SceneYAlign;
            float tall = height + 12f;
            AddBound(root, new Vector2(-1f, midY), new Vector2(1f, tall));
            AddBound(root, new Vector2(width, midY), new Vector2(1f, tall));
        }

        static void AddBound(Transform root, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("LevelBound");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.AddComponent<BoxCollider2D>().size = size;
        }

        static List<BridgeSpan> FindBridgeSpans(string[] map, int height, HashSet<Vector2Int> claimed)
        {
            var spans = new List<BridgeSpan>();
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

                    spans.Add(new BridgeSpan { Row = row, Start = start, Length = length });
                    for (int x = start; x < start + length; x++)
                        claimed.Add(new Vector2Int(x, row));
                }
            }
            return spans;
        }

        static void PlaceBridgeSpans(List<BridgeSpan> spans, Transform root, int height)
        {
            string plankArt = ArtCatalog.FirstExisting(
                "Tiles/bridge_plank_tile", "Tiles/bridge_plank", "Tiles/block_wood_past", "Tiles/floor_a");

            foreach (var span in spans)
            {
                float worldY = height - 1 - span.Row + SceneYAlign;
                float centerX = span.Start + (span.Length - 1) * 0.5f;

                for (int i = 0; i < span.Length; i++)
                {
                    int x = span.Start + i;
                    var plank = PlaceGridTile(root, new Vector2(x, worldY), plankArt,
                        EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock);
                    var sr = plank.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sortingOrder = 1;
                }

                if (ArtCatalog.Has("Tiles/bridge_plank"))
                {
                    var strip = SpriteFactory.CreateObject("BridgeStrip", root,
                        new Vector2(centerX, worldY), new Vector2(span.Length, 1f),
                        SpriteFactory.Square, Palette.PastBlock, 2);
                    if (ArtCatalog.TryApply(strip, "Tiles/bridge_plank", new Vector2(span.Length, 1f)))
                    {
                        strip.transform.position = new Vector3(centerX, worldY, 0f);
                        ArtCatalog.SetEraArt(strip, "Tiles/bridge_plank", "Tiles/bridge_plank",
                            new Vector2(span.Length, 1f));
                        TimeObject.Add(strip, EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock, solid: false);
                    }
                    else
                    {
                        Object.Destroy(strip);
                    }
                }
            }
        }

        static void PlaceSolid(Transform root, Vector2 pos, string[] map, int row, int col, int height)
        {
            bool airAbove = !IsSolidChar(CharAt(map, row - 1, col, height));
            string past;
            string future;
            if (tileset == "garden")
            {
                bool floating = airAbove && row + 1 < height && !IsSolidChar(CharAt(map, row + 1, col, height));
                if (floating) ArtCatalog.PickGardenLedge(out past, out future);
                else if (airAbove && IsPlantBed(col)) ArtCatalog.PickGardenSoil(out past, out future);
                else if (airAbove) ArtCatalog.PickGardenFloor(pos, out past, out future);
                else ArtCatalog.PickGardenWall(out past, out future);
            }
            else if (airAbove) ArtCatalog.PickFloorPair(pos, out past, out future);
            else ArtCatalog.PickWallPair(pos, out past, out future);

            var block = Block(root, pos, EraPresence.Both, Palette.PastWall, Palette.FutureWall, past);
            ArtCatalog.SetEraArt(block.gameObject, past, future, Vector2.one);
        }

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
                    if (cells.Count < 2 || maxY - minY + 1 < 2) continue;

                    foreach (var c in cells)
                    {
                        claimed.Add(c);
                        var crate = PlaceCrate(root, Cell(c.x, c.y, height));
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
            var block = PlaceGridTile(root, pos, art, EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock);
            var sr = block.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                int h = (Mathf.RoundToInt(pos.x) * 13 + Mathf.RoundToInt(pos.y) * 7) & 3;
                sr.flipX = (h & 1) == 0;
            }
            return block;
        }

        static TimeObject PlaceGridTile(Transform root, Vector2 pos, string art, EraPresence presence,
            Color past, Color future)
        {
            var go = SpriteFactory.CreateObject("Block", root, pos, Vector2.one, SpriteFactory.Square, past, 0);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            if (!string.IsNullOrEmpty(art) && ArtCatalog.Has(art))
            {
                ArtCatalog.TryApply(go, art, Vector2.one);
                ArtCatalog.SetEraArt(go, art, art, Vector2.one);
            }
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            return TimeObject.Add(go, presence, past, future, solid: true);
        }

        static char CharAt(string[] map, int row, int col, int height)
        {
            if (row < 0) return '#';
            if (row >= height || col < 0) return '.';
            string line = map[row];
            if (col >= line.Length) return '.';
            return line[col];
        }

        static bool IsPlantBed(int col) =>
            plantColumns.Contains(col) || plantColumns.Contains(col - 1) || plantColumns.Contains(col + 1);

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

            return PlaceGridTile(root, pos, artPath, presence, past, future);
        }

        static void Spikes(Transform root, Vector2 pos)
        {
            var go = ArtCatalog.Create("Spikes", root, pos, Vector2.one, SpriteFactory.Triangle, Palette.PastSpikes, 4,
                "Props/spikes", Vector2.one);
            // Sit on the pit floor (the solid cell directly under the spike).
            float floor = pos.y - 0.5f;
            if (ArtCatalog.Has("Props/spikes"))
                ArtCatalog.SnapBottomTo(go, floor, lift: 0f);

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                col.size = new Vector2(0.9f, Mathf.Max(0.55f, sr.bounds.size.y));
                col.offset = new Vector2(0f, sr.bounds.center.y - go.transform.position.y);
            }
            else
            {
                col.size = new Vector2(0.9f, 0.55f);
                col.offset = new Vector2(0f, 0.1f);
            }
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
            var glow = presence == EraPresence.FutureOnly
                ? new Color(0.45f, 0.95f, 1f, 0.7f)
                : new Color(1f, 0.84f, 0.35f, 0.75f);
            SoftGlow.Attach(go, glow, 1.35f, 4);
            TimeObject.Add(go, presence, Palette.Fragment, Palette.FutureFragment, solid: false);
        }
    }
}
