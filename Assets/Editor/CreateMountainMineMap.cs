// Assets/Editor/CreateMountainMineMap.cs
// Unity 6000.x  –  Menu: Tools > Mountain Mine > Build Map B (80×80)
//
// Run this ONCE after opening the scene:
//   Assets/Scenes/Levels/OutDoors/OutDoors_B_MountainMine.unity
//
// It procedurally fills the four Tilemap layers (Ground, Water, Details, Objects)
// with tiles that represent a mountain area with a mine entrance leading into a cave.
//
// Layout (80×80 tiles):
//   Border (3 tiles wide):     Rocky cliff walls using rock tiles
//   Outer mountain area:       Cave floor with rock scatter
//   Mine entrance corridor:    Horizontal passage at rows 37-43 entering from the right
//   Interior cave area:        Cave floor using muddy cave tiles
//   Underground water pool:    Centre-left area (cols 15-30, rows 25-55)

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OutDoors.Editor
{
    public static class CreateMountainMineMap
    {
        private const float CellSize = 0.16f;

        // ── Tile asset paths ────────────────────────────────────────────────
        private const string TILES = "Assets/Tilemaps/Tiles/MountainMine/";

        // Cave floor tiles (from muddy cave 16x16 v1)
        private static readonly string[] CaveFloorPaths =
        {
            TILES + "CaveFloorTile_0.asset",
            TILES + "CaveFloorTile_1.asset",
            TILES + "CaveFloorTile_2.asset",
            TILES + "CaveFloorTile_3.asset",
            TILES + "CaveFloorTile_4.asset",
        };

        // Rock / cliff tiles (mountain border and objects)
        private static readonly string[] RockPaths =
        {
            TILES + "RockTile_0.asset",
            TILES + "RockTile_1.asset",
            TILES + "RockTile_2.asset",
            TILES + "RockTile_3.asset",
        };

        // Underground water tiles
        private static readonly string[] WaterPaths =
        {
            TILES + "CaveWaterTile_0.asset",
            TILES + "CaveWaterTile_1.asset",
        };

        // Fallback: reuse EnchantedForest rock tiles if MountainMine tiles not found
        private const string EF_TILES = "Assets/Tilemaps/Tiles/EnchantedForest/";

        // ── Map dimensions ──────────────────────────────────────────────────
        private const int MAP_W = 80;
        private const int MAP_H = 80;

        // Mine entrance corridor: horizontal passage, rows 37-43, cols 45-79 (right side)
        private const int ENTRANCE_Y_MIN = 37;
        private const int ENTRANCE_Y_MAX = 43;
        private const int ENTRANCE_X_START = 45;  // entrance begins here from outside

        // Underground water pool region
        private const int POOL_X_MIN = 15;
        private const int POOL_X_MAX = 30;
        private const int POOL_Y_MIN = 25;
        private const int POOL_Y_MAX = 55;

        // ── Entry point ─────────────────────────────────────────────────────
        [MenuItem("Tools/Mountain Mine/Build Map B (80x80)")]
        public static void BuildMap()
        {
            TileBase[] caveFloor = LoadTiles(CaveFloorPaths);
            TileBase[] rocks     = LoadTiles(RockPaths);
            TileBase[] water     = LoadTiles(WaterPaths);

            // Fallback to EnchantedForest tiles when MountainMine tiles not yet imported
            if (caveFloor.Length == 0)
                caveFloor = LoadTilesByContains(TILES, "cave", "floor", "muddy");
            if (caveFloor.Length == 0)
                caveFloor = LoadTilesByContains(EF_TILES, "grass", "path", "summer");
            if (rocks.Length == 0)
                rocks = LoadTilesByContains(TILES, "rock");
            if (rocks.Length == 0)
                rocks = LoadTilesByContains(EF_TILES, "rock");
            if (water.Length == 0)
                water = LoadTilesByContains(TILES, "water");
            if (water.Length == 0)
                water = LoadTilesByContains(EF_TILES, "water");

            if (caveFloor.Length == 0)
            {
                Debug.LogError("[Mountain Mine] Cave floor tile assets not found. " +
                    "Make sure Assets/Tilemaps/Tiles/MountainMine/ exists and tiles are imported.");
                return;
            }

            Tilemap groundTm  = EnsureLayer("Ground",  0);
            Tilemap waterTm   = EnsureLayer("Water",   10);
            Tilemap detailsTm = EnsureLayer("Details", 20);
            Tilemap objectsTm = EnsureLayer("Objects", 30);

            EnsureGridForTilemap(groundTm);
            EnsureGridForTilemap(waterTm);
            EnsureGridForTilemap(detailsTm);
            EnsureGridForTilemap(objectsTm);

            if (groundTm == null)
            {
                Debug.LogError("[Mountain Mine] Could not create/find 'Ground' Tilemap. " +
                    "Open Assets/Scenes/Levels/OutDoors/OutDoors_B_MountainMine.unity first.");
                return;
            }

            groundTm.ClearAllTiles();
            waterTm?.ClearAllTiles();
            detailsTm?.ClearAllTiles();
            objectsTm?.ClearAllTiles();

            System.Random rng = new System.Random(77);

            TileBase primaryFloor  = caveFloor[0];
            TileBase primaryRock   = rocks.Length > 0 ? rocks[0] : primaryFloor;
            TileBase primaryWater  = water.Length > 0 ? water[0] : primaryFloor;

            // ── Pass 1: GROUND layer ────────────────────────────────────────
            // Border = solid rock cliffs
            // Mine entrance corridor = cave floor
            // Interior cave = cave floor with variation
            for (int x = 0; x < MAP_W; x++)
            {
                for (int y = 0; y < MAP_H; y++)
                {
                    TileBase tile;

                    if (IsBorder(x, y))
                    {
                        // Cliff walls: alternate rock variants
                        tile = Pick(rocks.Length > 0 ? rocks : caveFloor, rng);
                    }
                    else if (IsMineEntrance(x, y))
                    {
                        // Entrance corridor floor
                        tile = primaryFloor;
                    }
                    else if (IsWaterPool(x, y))
                    {
                        // Water pool base – use cave floor underneath
                        tile = primaryFloor;
                    }
                    else
                    {
                        // Interior cave floor with slight variation
                        double r = rng.NextDouble();
                        if (r < 0.12 && caveFloor.Length > 1)
                            tile = caveFloor[rng.Next(caveFloor.Length)];
                        else
                            tile = primaryFloor;
                    }

                    SetTile(groundTm, x, y, tile);
                }
            }

            // ── Pass 2: WATER layer ─────────────────────────────────────────
            // Underground pool in the left-centre region
            if (waterTm != null && water.Length > 0)
            {
                for (int x = POOL_X_MIN; x <= POOL_X_MAX; x++)
                {
                    for (int y = POOL_Y_MIN; y <= POOL_Y_MAX; y++)
                    {
                        TileBase w = water.Length > 1 && rng.NextDouble() < 0.3
                            ? water[rng.Next(water.Length)]
                            : primaryWater;
                        SetTile(waterTm, x, y, w);
                    }
                }
            }

            // ── Pass 3: DETAILS layer ───────────────────────────────────────
            // Scatter small rock details and cave floor variants inside the cave
            if (detailsTm != null && caveFloor.Length > 1)
            {
                for (int x = 3; x < MAP_W - 3; x++)
                {
                    for (int y = 3; y < MAP_H - 3; y++)
                    {
                        if (IsBorder(x, y)) continue;
                        if (IsMineEntrance(x, y)) continue;
                        if (IsWaterPool(x, y)) continue;

                        double r = rng.NextDouble();
                        // Occasional cave floor accent tiles on detail layer
                        if (r < 0.05 && caveFloor.Length > 2)
                            SetTile(detailsTm, x, y, caveFloor[2 + rng.Next(caveFloor.Length - 2)]);
                    }
                }
            }

            // ── Pass 4: OBJECTS layer ───────────────────────────────────────
            // Large rocks scattered through the cave; dense near border; none in pool/entrance
            if (objectsTm != null && rocks.Length > 0)
            {
                for (int x = 0; x < MAP_W; x++)
                {
                    for (int y = 0; y < MAP_H; y++)
                    {
                        if (IsMineEntrance(x, y)) continue;
                        if (IsWaterPool(x, y)) continue;

                        double rockDensity = IsBorder(x, y) ? 0.45 : IsNearPool(x, y) ? 0.15 : 0.05;
                        if (rng.NextDouble() < rockDensity)
                            SetTile(objectsTm, x, y, Pick(rocks, rng));
                    }
                }

                // Mine entrance arch rocks (top/bottom of entrance opening)
                PlaceMineEntranceArch(objectsTm, rocks, rng);
            }

            // Refresh
            groundTm.RefreshAllTiles();
            waterTm?.RefreshAllTiles();
            detailsTm?.RefreshAllTiles();
            objectsTm?.RefreshAllTiles();

            EditorUtility.SetDirty(groundTm);
            if (waterTm   != null) EditorUtility.SetDirty(waterTm);
            if (detailsTm != null) EditorUtility.SetDirty(detailsTm);
            if (objectsTm != null) EditorUtility.SetDirty(objectsTm);

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[Mountain Mine] Map B (80\u00d780) built successfully! " +
                $"Ground={groundTm.name} Water={waterTm?.name} " +
                $"Details={detailsTm?.name} Objects={objectsTm?.name}");
        }

        // ── Layout helpers ───────────────────────────────────────────────────

        // 3-tile solid rock border (cliff walls)
        private static bool IsBorder(int x, int y)
            => x < 3 || y < 3 || x >= MAP_W - 3 || y >= MAP_H - 3;

        // Horizontal mine entrance corridor coming from the right edge
        // Rows 37-43, cols ENTRANCE_X_START to right wall
        private static bool IsMineEntrance(int x, int y)
            => y >= ENTRANCE_Y_MIN && y <= ENTRANCE_Y_MAX && x >= ENTRANCE_X_START;

        // Underground water pool
        private static bool IsWaterPool(int x, int y)
            => x >= POOL_X_MIN && x <= POOL_X_MAX && y >= POOL_Y_MIN && y <= POOL_Y_MAX;

        // One tile margin around the pool (for rock border of pool)
        private static bool IsNearPool(int x, int y)
            => x >= POOL_X_MIN - 2 && x <= POOL_X_MAX + 2 &&
               y >= POOL_Y_MIN - 2 && y <= POOL_Y_MAX + 2;

        // Place a rock arch framing the mine entrance opening
        private static void PlaceMineEntranceArch(Tilemap tm, TileBase[] rocks, System.Random rng)
        {
            if (rocks.Length == 0) return;
            int archX = ENTRANCE_X_START;

            // Place rocks above and below the opening at the entrance column
            for (int col = archX; col < archX + 3; col++)
            {
                for (int dy = 1; dy <= 3; dy++)
                {
                    // Above entrance
                    int topY = ENTRANCE_Y_MAX + dy;
                    if (topY < MAP_H - 3)
                        SetTile(tm, col, topY, Pick(rocks, rng));

                    // Below entrance
                    int botY = ENTRANCE_Y_MIN - dy;
                    if (botY >= 3)
                        SetTile(tm, col, botY, Pick(rocks, rng));
                }
            }
        }

        // ── Tile loading helpers ─────────────────────────────────────────────

        private static TileBase[] LoadTiles(string[] paths)
        {
            var list = new List<TileBase>();
            foreach (string p in paths)
            {
                var t = AssetDatabase.LoadAssetAtPath<TileBase>(p);
                if (IsRenderableTile(t)) list.Add(t);
            }
            return list.ToArray();
        }

        private static TileBase[] LoadTilesByContains(string folder, params string[] keywords)
        {
            var list = new List<TileBase>();
            var seen = new HashSet<string>();
            string dir = folder.TrimEnd('/');
            var guids = AssetDatabase.FindAssets("t:Tile", new[] { dir });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string file = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                bool match = keywords == null || keywords.Length == 0;
                if (!match)
                {
                    foreach (var kw in keywords)
                    {
                        if (!string.IsNullOrEmpty(kw) && file.Contains(kw.ToLowerInvariant()))
                        {
                            match = true;
                            break;
                        }
                    }
                }
                if (!match) continue;
                if (!seen.Add(path)) continue;
                var tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                if (IsRenderableTile(tile)) list.Add(tile);
            }
            return list.ToArray();
        }

        private static bool IsRenderableTile(TileBase tile)
        {
            if (tile == null) return false;
            if (tile is Tile t) return t.sprite != null;
            return true;
        }

        // ── Tilemap/Grid helpers ─────────────────────────────────────────────

        private static Tilemap FindLayer(string name)
        {
            foreach (var tm in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
                if (tm.name == name) return tm;
            return null;
        }

        private static Tilemap EnsureLayer(string name, int sortingOrder)
        {
            var existing = FindLayer(name);
            if (existing != null)
            {
                var r = existing.GetComponent<TilemapRenderer>();
                if (r == null) r = existing.gameObject.AddComponent<TilemapRenderer>();
                r.sortingOrder = sortingOrder;
                return existing;
            }

            GameObject go = GameObject.Find(name) ?? new GameObject(name);
            var grid = FindGridTransform();
            if (grid != null) go.transform.SetParent(grid, false);

            var tilemap = go.GetComponent<Tilemap>() ?? go.AddComponent<Tilemap>();
            var renderer = go.GetComponent<TilemapRenderer>() ?? go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            return tilemap;
        }

        private static Transform FindGridTransform()
        {
            var known = GameObject.Find("MountainMine_Grid");
            if (known != null) { EnsureGridComponent(known); return known.transform; }

            foreach (var g in Object.FindObjectsByType<Grid>(FindObjectsSortMode.None))
            {
                g.cellSize = new Vector3(CellSize, CellSize, 0f);
                return g.transform;
            }

            var created = new GameObject("MountainMine_Grid");
            EnsureGridComponent(created);
            return created.transform;
        }

        private static void EnsureGridComponent(GameObject go)
        {
            var grid = go.GetComponent<Grid>() ?? go.AddComponent<Grid>();
            grid.enabled = true;
            grid.cellSize = new Vector3(CellSize, CellSize, 0f);
        }

        private static void EnsureGridForTilemap(Tilemap tm)
        {
            if (tm == null) return;
            var parentGrid = tm.GetComponentInParent<Grid>();
            if (parentGrid != null)
            {
                parentGrid.enabled = true;
                parentGrid.cellSize = new Vector3(CellSize, CellSize, 0f);
                return;
            }
            Transform parent = tm.transform.parent;
            if (parent == null)
            {
                var gridRoot = new GameObject("MountainMine_Grid");
                EnsureGridComponent(gridRoot);
                tm.transform.SetParent(gridRoot.transform, true);
                return;
            }
            EnsureGridComponent(parent.gameObject);
        }

        private static void SetTile(Tilemap tm, int x, int y, TileBase tile)
            => tm.SetTile(new Vector3Int(x, y, 0), tile);

        private static TileBase Pick(TileBase[] arr, System.Random rng)
            => arr[rng.Next(arr.Length)];
    }
}
