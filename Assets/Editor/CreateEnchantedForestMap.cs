// Assets/Editor/CreateEnchantedForestMap.cs
// Unity 6000.x  –  Menu: Tools > Enchanted Forest > Build Map A (80×80)
//
// Run this ONCE after opening the scene:
//   Assets/Scenes/Levels/OutDoors/OutDoors_A_EnchantedForest.unity
//
// It procedurally fills the four Tilemap layers (Ground, Water, Details, Objects)
// with tiles that approximate the "Enchanted Forest 80×80" reference layout.
//
// Requirements:
//   - Unity 2D Tilemap package (com.unity.2d.tilemap) must be installed
//   - The tile assets in Assets/Tilemaps/Tiles/EnchantedForest/ must be present
//   - PPU = 100, tile size = 16×16 → Cell size = 0.16 Unity units

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OutDoors.Editor
{
    public static class CreateEnchantedForestMap
    {
        // ── Tile asset paths ────────────────────────────────────────────────
        private const string TILES = "Assets/Tilemaps/Tiles/EnchantedForest/";

        // Ground grass tiles  (GrassTile_0 … GrassTile_5 = plain grass variants)
        private static readonly string[] GrassPaths =
        {
            TILES + "GrassTile_0.asset",
            TILES + "GrassTile_1.asset",
            TILES + "GrassTile_2.asset",
            TILES + "GrassTile_3.asset",
            TILES + "GrassTile_4.asset",
            TILES + "GrassTile_5.asset",
        };

        // Summer grass / dirt tiles  (SummerGrassTile_col_row)
        private static readonly string[] DirtPaths =
        {
            TILES + "SummerGrassTile_0_0.asset",
            TILES + "SummerGrassTile_1_0.asset",
            TILES + "SummerGrassTile_2_0.asset",
            TILES + "SummerGrassTile_3_0.asset",
            TILES + "SummerGrassTile_4_0.asset",
            TILES + "SummerGrassTile_0_1.asset",
            TILES + "SummerGrassTile_1_1.asset",
        };

        // Path / brick tiles  (PathTile_0 … PathTile_3)
        private static readonly string[] PathPaths =
        {
            TILES + "PathTile_0.asset",
            TILES + "PathTile_1.asset",
            TILES + "PathTile_2.asset",
            TILES + "PathTile_3.asset",
        };

        // Water tiles  (WaterTile_0 … WaterTile_7 from Water_ground sheets +
        //              GBWaterTile_0 … GBWaterTile_8 from Grass and Brick sheet)
        private static readonly string[] WaterPaths =
        {
            TILES + "WaterTile_0.asset",
            TILES + "WaterTile_1.asset",
            TILES + "WaterTile_2.asset",
            TILES + "WaterTile_3.asset",
            TILES + "WaterTile_4.asset",
            TILES + "WaterTile_5.asset",
            TILES + "WaterTile_6.asset",
            TILES + "WaterTile_7.asset",
            // Water tiles sliced from Grass and Brick.png
            TILES + "GBWaterTile_0.asset",  // Grass and Brick_2_5  – teal water
            TILES + "GBWaterTile_1.asset",  // Grass and Brick_1_6  – water left edge
            TILES + "GBWaterTile_2.asset",  // Grass and Brick_2_6  – bright-blue centre
            TILES + "GBWaterTile_3.asset",  // Grass and Brick_3_6  – water right edge
            TILES + "GBWaterTile_4.asset",  // Grass and Brick_2_7  – water top edge
            TILES + "GBWaterTile_5.asset",  // Grass and Brick_3_11 – teal variant
            TILES + "GBWaterTile_6.asset",  // Grass and Brick_4_11 – teal variant
            TILES + "GBWaterTile_7.asset",  // Grass and Brick_3_12 – teal variant
            TILES + "GBWaterTile_8.asset",  // Grass and Brick_4_12 – teal variant
        };

        // Flower / detail tiles
        private static readonly string[] FlowerPaths =
        {
            TILES + "flower_1_tile.asset",
            TILES + "flower_2_tile.asset",
            TILES + "flower_3_tile.asset",
            TILES + "flower_4_tile.asset",
            TILES + "flower_5_tile.asset",
            TILES + "flower_6_tile.asset",
            TILES + "flower_7_tile.asset",
        };

        // Bush / rock / tree tiles for Objects layer
        private static readonly string[] BushPaths =
        {
            TILES + "bush_1_tile.asset",
            TILES + "bush_2_tile.asset",
            TILES + "bush_3_tile.asset",
        };

        private static readonly string[] RockPaths =
        {
            TILES + "rock_1_tile.asset",
            TILES + "rock_2_tile.asset",
            TILES + "rock_3_tile.asset",
            TILES + "rock_water1_tile.asset",
            TILES + "rock_water2_tile.asset",
        };

        private static readonly string[] TreePaths =
        {
            TILES + "tree_1_tile.asset",
            TILES + "tree_2_tile.asset",
            TILES + "tree_3_tile.asset",
            TILES + "tree_4_tile.asset",
        };

        private static readonly string[] StemPaths =
        {
            TILES + "stem_1_tile.asset",
            TILES + "stem_2_tile.asset",
            TILES + "stem_3_tile.asset",
        };

        // ── Map dimensions ──────────────────────────────────────────────────
        private const int MAP_W = 80;
        private const int MAP_H = 80;

        // ── Entry point ─────────────────────────────────────────────────────
        [MenuItem("Tools/Enchanted Forest/Build Map A (80x80)")]
        public static void BuildMap()
        {
            // Load all tile assets
            TileBase[] grass   = LoadTiles(GrassPaths);
            TileBase[] dirt    = LoadTiles(DirtPaths);
            TileBase[] path    = LoadTiles(PathPaths);
            TileBase[] water   = LoadTiles(WaterPaths);
            TileBase[] flowers = LoadTiles(FlowerPaths);
            TileBase[] bushes  = LoadTiles(BushPaths);
            TileBase[] rocks   = LoadTiles(RockPaths);
            TileBase[] trees   = LoadTiles(TreePaths);
            TileBase[] stems   = LoadTiles(StemPaths);

            if (grass.Length == 0)
            {
                Debug.LogError("[Enchanted Forest] Grass tile assets not found. " +
                    "Make sure Assets/Tilemaps/Tiles/EnchantedForest/ exists.");
                return;
            }

            // Find layers in scene
            Tilemap groundTm  = FindLayer("Ground");
            Tilemap waterTm   = FindLayer("Water");
            Tilemap detailsTm = FindLayer("Details");
            Tilemap objectsTm = FindLayer("Objects");

            if (groundTm == null)
            {
                Debug.LogError("[Enchanted Forest] 'Ground' Tilemap not found. " +
                    "Open Assets/Scenes/Levels/OutDoors/OutDoors_A_EnchantedForest.unity first.");
                return;
            }

            // Clear existing tiles
            groundTm.ClearAllTiles();
            waterTm?.ClearAllTiles();
            detailsTm?.ClearAllTiles();
            objectsTm?.ClearAllTiles();

            System.Random rng = new System.Random(42);

            // ── Pass 1: Fill GROUND layer ───────────────────────────────────
            // Layout regions:
            //   Border (2 tiles): dense forest edge with dirt variation
            //   Inner area:       grass with sparse dirt patches
            //   Central path:     diagonal brick path (cols 20-23 rising to 60-63)
            for (int x = 0; x < MAP_W; x++)
            {
                for (int y = 0; y < MAP_H; y++)
                {
                    TileBase tile;

                    if (IsBorder(x, y))
                    {
                        // Forest edge: earthy ground
                        tile = Pick(dirt, rng);
                    }
                    else if (IsPath(x, y))
                    {
                        // Stone/dirt path through the forest
                        tile = Pick(path, rng);
                    }
                    else
                    {
                        // Base: green grass with occasional dirt variation
                        double r = rng.NextDouble();
                        if (r < 0.08) tile = Pick(dirt, rng);
                        else          tile = Pick(grass, rng);
                    }

                    SetTile(groundTm, x, y, tile);
                }
            }

            // ── Pass 2: Fill WATER layer ────────────────────────────────────
            // A meandering stream running roughly from (10,70) to (70,10)
            if (waterTm != null && water.Length > 0)
            {
                FillRiver(waterTm, water, rng);
            }

            // ── Pass 3: Fill DETAILS layer ──────────────────────────────────
            // Scatter flowers and stems in non-path, non-water, non-border areas
            if (detailsTm != null && (flowers.Length > 0 || stems.Length > 0))
            {
                for (int x = 3; x < MAP_W - 3; x++)
                {
                    for (int y = 3; y < MAP_H - 3; y++)
                    {
                        if (IsPath(x, y) || IsRiver(x, y)) continue;
                        double r = rng.NextDouble();
                        if (r < 0.07 && flowers.Length > 0)
                            SetTile(detailsTm, x, y, Pick(flowers, rng));
                        else if (r < 0.09 && stems.Length > 0)
                            SetTile(detailsTm, x, y, Pick(stems, rng));
                    }
                }
            }

            // ── Pass 4: Fill OBJECTS layer ──────────────────────────────────
            // Trees, bushes, rocks – denser near border, sparse in centre
            if (objectsTm != null)
            {
                for (int x = 0; x < MAP_W; x++)
                {
                    for (int y = 0; y < MAP_H; y++)
                    {
                        if (IsPath(x, y) || IsRiver(x, y)) continue;

                        double treeDensity = IsBorder(x, y) ? 0.35 : 0.06;
                        double bushDensity = IsBorder(x, y) ? 0.15 : 0.04;
                        double rockDensity = IsRiver(x, y + 1) || IsRiver(x, y - 1) ? 0.25 : 0.02;

                        double r = rng.NextDouble();
                        if (r < treeDensity && trees.Length > 0)
                            SetTile(objectsTm, x, y, Pick(trees, rng));
                        else if (r < treeDensity + bushDensity && bushes.Length > 0)
                            SetTile(objectsTm, x, y, Pick(bushes, rng));
                        else if (r < treeDensity + bushDensity + rockDensity && rocks.Length > 0)
                            SetTile(objectsTm, x, y, Pick(rocks, rng));
                    }
                }
            }

            // Refresh + save scene
            groundTm.RefreshAllTiles();
            waterTm?.RefreshAllTiles();
            detailsTm?.RefreshAllTiles();
            objectsTm?.RefreshAllTiles();

            EditorUtility.SetDirty(groundTm);
            if (waterTm   != null) EditorUtility.SetDirty(waterTm);
            if (detailsTm != null) EditorUtility.SetDirty(detailsTm);
            if (objectsTm != null) EditorUtility.SetDirty(objectsTm);

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[Enchanted Forest] Map A (80\u00d780) built successfully! " +
                $"Ground={groundTm.name} Water={waterTm?.name} " +
                $"Details={detailsTm?.name} Objects={objectsTm?.name}");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static TileBase[] LoadTiles(string[] paths)
        {
            var list = new List<TileBase>();
            foreach (string p in paths)
            {
                var t = AssetDatabase.LoadAssetAtPath<TileBase>(p);
                if (t != null) list.Add(t);
            }
            return list.ToArray();
        }

        private static Tilemap FindLayer(string name)
        {
            foreach (var tm in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
                if (tm.name == name) return tm;
            return null;
        }

        private static void SetTile(Tilemap tm, int x, int y, TileBase tile)
        {
            tm.SetTile(new Vector3Int(x, y, 0), tile);
        }

        private static TileBase Pick(TileBase[] arr, System.Random rng)
        {
            return arr[rng.Next(arr.Length)];
        }

        // 2-tile border region
        private static bool IsBorder(int x, int y)
        {
            return x < 3 || y < 3 || x >= MAP_W - 3 || y >= MAP_H - 3;
        }

        // Diagonal path: a 4-tile-wide stone path running SW→NE through the map
        // Approximates the winding forest trail in the reference image
        private static bool IsPath(int x, int y)
        {
            // Trail equation: y = x * (MAP_H / MAP_W) + slight S-curve offset
            float cx = MAP_W * 0.5f;
            float cy = MAP_H * 0.5f;
            float angle = Mathf.PI / 4f;
            // Rotate coordinates, check within ±2 tiles of diagonal
            float rx = (x - cx) * Mathf.Cos(angle) + (y - cy) * Mathf.Sin(angle);
            if (Mathf.Abs(rx) <= 2f) return true;

            // Second narrower S-curve branch (horizontal path across centre)
            if (y >= 36 && y <= 40 && x >= 15 && x <= 65) return true;
            return false;
        }

        // River: meandering stream from top-left area to bottom-right area
        // Stored as a lookup set for quick queries
        private static HashSet<long> _riverCells;

        private static void BuildRiverSet(System.Random rng)
        {
            _riverCells = new HashSet<long>();
            // Control points for the river: start near (8, 72), end near (72, 8)
            var pts = new List<Vector2Int>
            {
                new Vector2Int(8,  72),
                new Vector2Int(14, 65),
                new Vector2Int(20, 58),
                new Vector2Int(25, 52),
                new Vector2Int(30, 48),
                new Vector2Int(38, 42),
                new Vector2Int(44, 36),
                new Vector2Int(50, 30),
                new Vector2Int(58, 22),
                new Vector2Int(65, 16),
                new Vector2Int(72, 8),
            };
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var a = pts[i]; var b = pts[i + 1];
                int steps = Mathf.Max(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y)) + 1;
                for (int s = 0; s <= steps; s++)
                {
                    float t = steps == 0 ? 0 : (float)s / steps;
                    int rx = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, t));
                    int ry = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, t));
                    for (int dx = -2; dx <= 2; dx++)
                        for (int dy = -2; dy <= 2; dy++)
                            _riverCells.Add(Key(rx + dx, ry + dy));
                }
            }
        }

        private static bool IsRiver(int x, int y) =>
            _riverCells != null && _riverCells.Contains(Key(x, y));

        private static long Key(int x, int y) => ((long)x << 16) | (uint)y;

        private static void FillRiver(Tilemap tm, TileBase[] water, System.Random rng)
        {
            BuildRiverSet(rng);
            foreach (long key in _riverCells)
            {
                int x = (int)(key >> 16);
                int y = (int)(key & 0xFFFF);
                if (x < 0 || y < 0 || x >= MAP_W || y >= MAP_H) continue;
                SetTile(tm, x, y, Pick(water, rng));
            }
        }
    }
}
