// Assets/Editor/CreateCozyTownMap.cs
// Unity 6000.x  –  Menu: Tools > Cozy Town > Build Map C (120×120)
//
// Run this ONCE after opening the scene:
//   Assets/Scenes/Levels/OutDoors/OutDoors_C_CozyTown.unity
//
// It procedurally fills the four Tilemap layers (Ground, Water, Details, Objects)
// with tiles that represent a cozy top-down town with residential areas, a town
// square, a park, a pond, and tree-lined streets.
//
// Layout (120×120 tiles):
//   Border (3 tiles wide):      Dense tree/bush border
//   Outer residential belt:     Grass plots separated by stone paths, with houses
//   Inner ring roads:           Path tiles forming a cross through the map
//   Town square (centre):       Path tiles with flower/fountain decoration
//   Park (NW quadrant):         Grass + flowers + trees + pond
//   Market/Blacksmith (NE):     Path tiles + blacksmith house
//   Farmer quarter (SW):        Grass + farmer house + fence rows
//   Better homes (SE):          Better houses, fenced gardens

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OutDoors.Editor
{
    public static class CreateCozyTownMap
    {
        private const float CellSize = 0.16f;

        // ── Tile asset paths ─────────────────────────────────────────────────
        private const string CT_TILES = "Assets/Tilemaps/Tiles/CozyTown/";
        private const string EF_TILES = "Assets/Tilemaps/Tiles/EnchantedForest/";

        // Ground variants
        private static readonly string[] GrassPaths =
        {
            CT_TILES + "TownGrassTile_0.asset",
            CT_TILES + "TownGrassTile_1.asset",
            CT_TILES + "TownGrassTile_2.asset",
        };

        // Path/brick tiles (used for roads and town square)
        private static readonly string[] PathPaths =
        {
            CT_TILES + "TownPathTile_0.asset",
            CT_TILES + "TownPathTile_1.asset",
            CT_TILES + "TownPathTile_2.asset",
        };

        // Water tiles (pond in the park)
        private static readonly string[] WaterPaths =
        {
            CT_TILES + "TownWaterTile_0.asset",
            CT_TILES + "TownWaterTile_1.asset",
        };

        // Fence tiles
        private static readonly string[] FencePaths =
        {
            CT_TILES + "TownFenceTile_0.asset",
            CT_TILES + "TownFenceTile_1.asset",
        };

        // Flower detail tiles
        private static readonly string[] FlowerPaths =
        {
            CT_TILES + "TownFlowerTile_0.asset",
            CT_TILES + "TownFlowerTile_1.asset",
        };

        // Tree object tiles (border + park)
        private static readonly string[] TreePaths =
        {
            CT_TILES + "TownTreeTile_0.asset",
            CT_TILES + "TownTreeTile_1.asset",
        };

        // Bush object tiles
        private static readonly string[] BushPaths =
        {
            CT_TILES + "TownBushTile_0.asset",
        };

        // House object tiles
        private static readonly string[] NormalHousePaths  = { CT_TILES + "NormalHouseTile.asset" };
        private static readonly string[] FarmerHousePaths  = { CT_TILES + "FarmerHouseTile.asset" };
        private static readonly string[] SmithHousePaths   = { CT_TILES + "BlacksmithHouseTile.asset" };

        // ── Map dimensions ────────────────────────────────────────────────────
        private const int MAP_W = 120;
        private const int MAP_H = 120;

        // Town centre square (crossroads hub, 20x20 tiles in the exact centre)
        private const int SQUARE_X  = 50;
        private const int SQUARE_Y  = 50;
        private const int SQUARE_W  = 20;
        private const int SQUARE_H  = 20;

        // Main N-S road (column band, 4 tiles wide, centred at x=60)
        private const int NS_ROAD_X = 58;
        private const int NS_ROAD_W = 4;

        // Main E-W road (row band, 4 tiles wide, centred at y=60)
        private const int EW_ROAD_Y = 58;
        private const int EW_ROAD_H = 4;

        // Park pond (NW region)
        private const int POND_X = 15;
        private const int POND_Y = 70;
        private const int POND_W = 20;
        private const int POND_H = 15;

        // ── Entry point ───────────────────────────────────────────────────────
        [MenuItem("Tools/Cozy Town/Build Map C (120x120)")]
        public static void BuildMap()
        {
            TileBase[] grass  = LoadTiles(GrassPaths);
            TileBase[] paths  = LoadTiles(PathPaths);
            TileBase[] water  = LoadTiles(WaterPaths);
            TileBase[] fences = LoadTiles(FencePaths);
            TileBase[] flowers= LoadTiles(FlowerPaths);
            TileBase[] trees  = LoadTiles(TreePaths);
            TileBase[] bushes = LoadTiles(BushPaths);
            TileBase[] norHouses = LoadTiles(NormalHousePaths);
            TileBase[] farHouses = LoadTiles(FarmerHousePaths);
            TileBase[] smHouses  = LoadTiles(SmithHousePaths);

            // Fallbacks: borrow from EnchantedForest if CozyTown tiles not imported yet
            if (grass.Length  == 0) grass  = LoadTilesByContains(EF_TILES, "grass");
            if (paths.Length  == 0) paths  = LoadTilesByContains(EF_TILES, "path");
            if (water.Length  == 0) water  = LoadTilesByContains(EF_TILES, "water");
            if (fences.Length == 0) fences = LoadTilesByContains(EF_TILES, "fence");
            if (flowers.Length== 0) flowers= LoadTilesByContains(EF_TILES, "flower");
            if (trees.Length  == 0) trees  = LoadTilesByContains(EF_TILES, "tree");
            if (bushes.Length == 0) bushes = LoadTilesByContains(EF_TILES, "bush");
            if (norHouses.Length== 0) norHouses = grass.Length > 0 ? new[] { grass[0] } : new TileBase[0];
            if (farHouses.Length== 0) farHouses = norHouses;
            if (smHouses.Length == 0) smHouses  = norHouses;

            if (grass.Length == 0)
            {
                Debug.LogError("[Cozy Town] Grass tile assets not found. " +
                    "Make sure Assets/Tilemaps/Tiles/CozyTown/ exists and tiles are imported.");
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
                Debug.LogError("[Cozy Town] Could not create/find 'Ground' Tilemap. " +
                    "Open Assets/Scenes/Levels/OutDoors/OutDoors_C_CozyTown.unity first.");
                return;
            }

            groundTm.ClearAllTiles();
            waterTm?.ClearAllTiles();
            detailsTm?.ClearAllTiles();
            objectsTm?.ClearAllTiles();

            System.Random rng = new System.Random(99);

            TileBase primaryGrass = grass[0];
            TileBase primaryPath  = paths.Length > 0  ? paths[0]  : primaryGrass;
            TileBase primaryWater = water.Length > 0  ? water[0]  : primaryGrass;

            // ── Pass 1: GROUND layer ──────────────────────────────────────────
            for (int x = 0; x < MAP_W; x++)
            {
                for (int y = 0; y < MAP_H; y++)
                {
                    TileBase tile;

                    if (IsMainRoad(x, y) || IsTownSquare(x, y))
                    {
                        // Roads and town square use path tiles
                        tile = paths.Length > 1 && rng.NextDouble() < 0.25
                            ? paths[rng.Next(paths.Length)]
                            : primaryPath;
                    }
                    else if (IsPond(x, y))
                    {
                        // Pond base: grass underneath
                        tile = primaryGrass;
                    }
                    else
                    {
                        // Varied grass across the map
                        double r = rng.NextDouble();
                        tile = (r < 0.15 && grass.Length > 1)
                            ? grass[rng.Next(grass.Length)]
                            : primaryGrass;
                    }

                    SetTile(groundTm, x, y, tile);
                }
            }

            // ── Pass 2: WATER layer (pond) ────────────────────────────────────
            if (waterTm != null && water.Length > 0)
            {
                for (int x = POND_X; x < POND_X + POND_W; x++)
                {
                    for (int y = POND_Y; y < POND_Y + POND_H; y++)
                    {
                        TileBase w = (water.Length > 1 && rng.NextDouble() < 0.3)
                            ? water[rng.Next(water.Length)]
                            : primaryWater;
                        SetTile(waterTm, x, y, w);
                    }
                }
            }

            // ── Pass 3: DETAILS layer ─────────────────────────────────────────
            // Flowers in the town square, along roads, and in the park
            if (detailsTm != null && flowers.Length > 0)
            {
                // Town square: flower borders at edges of the square
                for (int x = SQUARE_X; x < SQUARE_X + SQUARE_W; x++)
                {
                    for (int y = SQUARE_Y; y < SQUARE_Y + SQUARE_H; y++)
                    {
                        bool atEdge = (x == SQUARE_X || x == SQUARE_X + SQUARE_W - 1 ||
                                       y == SQUARE_Y || y == SQUARE_Y + SQUARE_H - 1);
                        if (atEdge && rng.NextDouble() < 0.5)
                            SetTile(detailsTm, x, y, Pick(flowers, rng));
                    }
                }

                // Park area (NW): scatter flowers around pond edges
                int parkX1 = 8, parkY1 = 65, parkX2 = 45, parkY2 = 95;
                for (int x = parkX1; x < parkX2; x++)
                {
                    for (int y = parkY1; y < parkY2; y++)
                    {
                        if (IsPond(x, y)) continue;
                        double dist = System.Math.Sqrt(
                            System.Math.Pow(x - (POND_X + POND_W / 2.0), 2) +
                            System.Math.Pow(y - (POND_Y + POND_H / 2.0), 2));
                        double density = dist < 12 ? 0.18 : 0.06;
                        if (rng.NextDouble() < density)
                            SetTile(detailsTm, x, y, Pick(flowers, rng));
                    }
                }

                // Road sides: occasional flower accent
                for (int x = 3; x < MAP_W - 3; x++)
                {
                    for (int y = 3; y < MAP_H - 3; y++)
                    {
                        if (IsMainRoad(x, y) || IsTownSquare(x, y)) continue;
                        if (IsAdjacentToRoad(x, y) && rng.NextDouble() < 0.08)
                            SetTile(detailsTm, x, y, Pick(flowers, rng));
                    }
                }
            }

            // ── Pass 4: OBJECTS layer ─────────────────────────────────────────
            if (objectsTm != null)
            {
                // Dense tree border (3 tiles wide)
                PlaceBorderTrees(objectsTm, trees, bushes, rng);

                // Park trees (NW quadrant away from pond)
                if (trees.Length > 0)
                {
                    for (int x = 8; x < 48; x++)
                    {
                        for (int y = 65; y < 95; y++)
                        {
                            if (IsBorder(x, y)) continue;
                            if (IsPond(x, y)) continue;
                            if (IsMainRoad(x, y) || IsTownSquare(x, y)) continue;
                            if (rng.NextDouble() < 0.06)
                                SetTile(objectsTm, x, y, Pick(trees, rng));
                        }
                    }
                }

                // Fences around residential plots (SW quadrant)
                if (fences.Length > 0)
                    PlaceFenceRows(objectsTm, fences, rng);

                // Houses in the four quadrants
                PlaceHouses(objectsTm, norHouses, farHouses, smHouses, rng);

                // Bushes scattered in residential areas
                if (bushes.Length > 0)
                {
                    for (int x = 5; x < MAP_W - 5; x++)
                    {
                        for (int y = 5; y < MAP_H - 5; y++)
                        {
                            if (IsBorder(x, y) || IsMainRoad(x, y) || IsTownSquare(x, y)) continue;
                            if (IsPond(x, y)) continue;
                            if (rng.NextDouble() < 0.018)
                                SetTile(objectsTm, x, y, Pick(bushes, rng));
                        }
                    }
                }
            }

            // Refresh all
            groundTm.RefreshAllTiles();
            waterTm?.RefreshAllTiles();
            detailsTm?.RefreshAllTiles();
            objectsTm?.RefreshAllTiles();

            EditorUtility.SetDirty(groundTm);
            if (waterTm   != null) EditorUtility.SetDirty(waterTm);
            if (detailsTm != null) EditorUtility.SetDirty(detailsTm);
            if (objectsTm != null) EditorUtility.SetDirty(objectsTm);

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[Cozy Town] Map C (120\u00d7120) built successfully! " +
                $"Ground={groundTm.name} Water={waterTm?.name} " +
                $"Details={detailsTm?.name} Objects={objectsTm?.name}");
        }

        // ── Layout helpers ────────────────────────────────────────────────────

        private static bool IsBorder(int x, int y)
            => x < 3 || y < 3 || x >= MAP_W - 3 || y >= MAP_H - 3;

        private static bool IsTownSquare(int x, int y)
            => x >= SQUARE_X && x < SQUARE_X + SQUARE_W &&
               y >= SQUARE_Y && y < SQUARE_Y + SQUARE_H;

        private static bool IsMainRoad(int x, int y)
        {
            bool ns = x >= NS_ROAD_X && x < NS_ROAD_X + NS_ROAD_W;
            bool ew = y >= EW_ROAD_Y && y < EW_ROAD_Y + EW_ROAD_H;
            return ns || ew;
        }

        private static bool IsAdjacentToRoad(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if (IsMainRoad(x + dx, y + dy)) return true;
            return false;
        }

        private static bool IsPond(int x, int y)
            => x >= POND_X && x < POND_X + POND_W &&
               y >= POND_Y && y < POND_Y + POND_H;

        // Place dense trees/bushes along the map border
        private static void PlaceBorderTrees(Tilemap tm, TileBase[] trees, TileBase[] bushes,
            System.Random rng)
        {
            if (trees.Length == 0 && bushes.Length == 0) return;
            TileBase[] pool = trees.Length > 0 ? trees : bushes;

            for (int x = 0; x < MAP_W; x++)
            {
                for (int y = 0; y < MAP_H; y++)
                {
                    if (!IsBorder(x, y)) continue;
                    if (rng.NextDouble() < 0.7)
                        SetTile(tm, x, y, Pick(pool, rng));
                }
            }
        }

        // Horizontal fence rows in the SW farming quarter
        private static void PlaceFenceRows(Tilemap tm, TileBase[] fences, System.Random rng)
        {
            // Fence lines every 8 tiles in y, from y=8..55, x=8..55
            for (int row = 8; row < 56; row += 8)
            {
                for (int x = 8; x < 56; x++)
                {
                    if (IsMainRoad(x, row) || IsTownSquare(x, row)) continue;
                    SetTile(tm, x, row, Pick(fences, rng));
                }
            }
        }

        // Scatter houses across four quadrants (avoiding roads and border)
        private static void PlaceHouses(Tilemap tm,
            TileBase[] norHouses, TileBase[] farHouses, TileBase[] smHouses,
            System.Random rng)
        {
            // NE quadrant → normal houses (grid placement, every 10 tiles)
            PlaceHouseGrid(tm, norHouses, rng, 65, 65, 112, 112, 10);
            // SW quadrant → farmer houses
            PlaceHouseGrid(tm, farHouses, rng, 8,  8,  55,  55,  10);
            // NW quadrant (near park) → normal houses, sparser
            PlaceHouseGrid(tm, norHouses, rng, 8,  65, 45,  112, 14);
            // SE quadrant → blacksmith / normal mix
            PlaceHouseGrid(tm, smHouses,  rng, 65, 8,  112, 55,  12);
        }

        private static void PlaceHouseGrid(Tilemap tm, TileBase[] houses, System.Random rng,
            int x0, int y0, int x1, int y1, int step)
        {
            if (houses.Length == 0) return;
            for (int x = x0; x < x1; x += step)
            {
                for (int y = y0; y < y1; y += step)
                {
                    if (IsBorder(x, y) || IsMainRoad(x, y) || IsTownSquare(x, y)) continue;
                    if (IsPond(x, y)) continue;
                    // Small jitter so houses don't form a perfect grid
                    int jx = x + rng.Next(-1, 2);
                    int jy = y + rng.Next(-1, 2);
                    jx = System.Math.Clamp(jx, 3, MAP_W - 4);
                    jy = System.Math.Clamp(jy, 3, MAP_H - 4);
                    if (IsMainRoad(jx, jy) || IsTownSquare(jx, jy) || IsPond(jx, jy)) continue;
                    SetTile(tm, jx, jy, Pick(houses, rng));
                }
            }
        }

        // ── Tile loading helpers ──────────────────────────────────────────────

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

        // ── Tilemap/Grid helpers ──────────────────────────────────────────────

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
            var known = GameObject.Find("CozyTown_Grid");
            if (known != null) { EnsureGridComponent(known); return known.transform; }

            foreach (var g in Object.FindObjectsByType<Grid>(FindObjectsSortMode.None))
            {
                g.cellSize = new Vector3(CellSize, CellSize, 0f);
                return g.transform;
            }

            var created = new GameObject("CozyTown_Grid");
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
                var gridRoot = new GameObject("CozyTown_Grid");
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
