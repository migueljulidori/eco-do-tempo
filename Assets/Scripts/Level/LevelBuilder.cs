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
            PlacePastBlockades(level.Map, root, height, claimed);

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

        // Horizontal 'p' runs (3+) → bridge_past flush with the stone ledge.
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
                    float deckTop = worldY + 0.5f;
                    float centerX = start + (length - 1) * 0.5f;

                    for (int x = start; x < start + length; x++)
                        claimed.Add(new Vector2Int(x, row));

                    if (ArtCatalog.Has("Tiles/bridge_past"))
                    {
                        for (int x = start; x < start + length; x++)
                            PlaceHiddenSupport(root, new Vector2(x, worldY), EraPresence.PastOnly);

                        // Exact cell height so the walking surface matches neighboring floors.
                        var bridge = ArtCatalog.Create("Bridge", root, new Vector2(centerX, worldY),
                            new Vector2(length, 1f), SpriteFactory.Square, Palette.PastBlock, 2,
                            "Tiles/bridge_past", new Vector2(length, 1f));
                        ArtCatalog.SnapTopTo(bridge, deckTop);
                        ArtCatalog.SetEraArt(bridge, "Tiles/bridge_past", "Tiles/bridge_past", new Vector2(length, 1f));
                        TimeObject.Add(bridge, EraPresence.PastOnly, Palette.PastBlock, Palette.PastBlock, solid: false);
                    }
                    else
                    {
                        for (int x = start; x < start + length; x++)
                        {
                            var wood = Block(root, new Vector2(x, worldY), EraPresence.PastOnly,
                                Palette.PastBlock, Palette.PastBlock, "Tiles/block_wood_past");
                            var sr = wood.GetComponent<SpriteRenderer>();
                            if (sr != null) sr.sortingOrder = 2;
                        }
                    }
                }
            }
        }

        // Vertical 'p' stacks (2+) → one iron blockade instead of a wood crate tower.
        static void PlacePastBlockades(string[] map, Transform root, int height, HashSet<Vector2Int> claimed)
        {
            if (!ArtCatalog.Has("Props/blockade")) return;

            int width = 0;
            foreach (var line in map) width = Mathf.Max(width, line.Length);

            for (int col = 0; col < width; col++)
            {
                int row = 0;
                while (row < height)
                {
                    if (CharAt(map, row, col, height) != 'p' || claimed.Contains(new Vector2Int(col, row)))
                    {
                        row++;
                        continue;
                    }

                    int start = row;
                    while (row < height && CharAt(map, row, col, height) == 'p'
                           && !claimed.Contains(new Vector2Int(col, row)))
                        row++;
                    int length = row - start;
                    if (length < 2) continue;

                    for (int r = start; r < start + length; r++)
                        claimed.Add(new Vector2Int(col, r));

                    // Bottom cell of the stack in world space.
                    int bottomRow = start + length - 1;
                    float bottomY = height - 1 - bottomRow;
                    float feetY = bottomY - 0.5f;
                    float artH = Mathf.Clamp(length, 2, 3);

                    var go = ArtCatalog.Create("Blockade", root, new Vector2(col, bottomY),
                        new Vector2(1.2f, artH), SpriteFactory.Square, Palette.PastGate, 3,
                        "Props/blockade", new Vector2(1.2f, artH));
                    ArtCatalog.SnapBottomTo(go, feetY, lift: 0.02f);

                    var sr = go.GetComponent<SpriteRenderer>();
                    var col2d = go.AddComponent<BoxCollider2D>();
                    if (sr != null && sr.sprite != null)
                    {
                        col2d.size = new Vector2(1.05f, sr.bounds.size.y * 0.92f);
                        col2d.offset = new Vector2(0f, sr.bounds.center.y - go.transform.position.y);
                    }
                    else
                    {
                        col2d.size = new Vector2(1.05f, artH);
                        col2d.offset = new Vector2(0f, artH * 0.5f - 0.5f);
                    }

                    ArtCatalog.SetEraArt(go, "Props/blockade", "Props/blockade", new Vector2(1.2f, artH));
                    TimeObject.Add(go, EraPresence.PastOnly, Palette.PastGate, Palette.FutureGate, solid: true);
                }
            }
        }

        static void PlaceHiddenSupport(Transform root, Vector2 pos, EraPresence presence)
        {
            var go = new GameObject("BridgeSupport");
            go.transform.SetParent(root, false);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 0.4f);
            col.offset = new Vector2(0f, 0.3f);
            TimeObject.Add(go, presence, Palette.PastBlock, Palette.PastBlock, solid: true);
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
            ArtCatalog.SetEraArt(go, artPath, artPath, Vector2.one);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return TimeObject.Add(go, presence, past, future, solid: true);
        }

        static void Spikes(Transform root, Vector2 pos)
        {
            var go = ArtCatalog.Create("Spikes", root, pos, Vector2.one, SpriteFactory.Triangle, Palette.PastSpikes, 4,
                "Props/spikes", Vector2.one);
            // Flush on the pit floor (top of the solid under this air cell).
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
