# OutDoors – Maps A, B, C

| Map | Scene | Size | Status |
|-----|-------|------|--------|
| A – Enchanted Forest | `OutDoors_A_EnchantedForest.unity` | 80×80 | ✅ Done |
| B – Mountain Mine | `OutDoors_B_MountainMine.unity` | 80×80 | ✅ Scene + Editor Script ready |
| C – Cozy Town | `OutDoors_C_CozyTown.unity` | 120×120 | ✅ Scene + Editor Script ready |

---

## Map A: Enchanted Forest (80×80)

## Setup Instructions

### 1. Import Sprites
All required sprite PNGs are already included in the project under `Assets/Sprites/`:
- `Grass.png` (192×320 – 12×20 tiles)
- `Grass and Brick.png` (256×256 – 16×16 tiles)
- `Water_ground_1.png` (128×32 – 8×2 tiles)
- `Water_ground_2.png` (192×48 – 12×3 tiles)
- `bonus summer grass and dirt.png` (80×96 – 5×6 tiles)
- `fence_tileset.png` (64×64 – 4×4 tiles)
- `bush_1–3.png`, `flower_1–7.png`, `rock_1–7.png`, `tree_1–4.png`, `stem_1–3.png` (16×16 singles)

Each PNG is accompanied by a `.meta` file that configures:
- **Pixels Per Unit = 100**
- **Sprite Mode = Multiple** (for tilesets) or **Single** (for individual sprites)
- **16×16 slice grid** (for tilesets)
- **Filter Mode = Point (no filter)**

### 2. Open the Scene
Open:  
`Assets/Scenes/Levels/OutDoors/OutDoors_A_EnchantedForest.unity`

The scene contains:
| GameObject | Component | Sorting Order |
|---|---|---|
| `EnchantedForest_Grid` | Grid (cell 0.16 × 0.16) | – |
| └ `Ground` | Tilemap + TilemapRenderer | 0 |
| └ `Water` | Tilemap + TilemapRenderer | 1 |
| └ `Details` | Tilemap + TilemapRenderer | 2 |
| └ `Objects` | Tilemap + TilemapRenderer | 3 |

### 3. Build the Map
From the Unity menu bar run:  
**Tools → Enchanted Forest → Build Map A (80×80)**

This executes `Assets/Editor/CreateEnchantedForestMap.cs` which procedurally fills all four Tilemap layers to approximate the reference Enchanted Forest layout:
- **Ground** – grass + dirt paths
- **Water** – meandering river/stream
- **Details** – scattered flowers and plants
- **Objects** – trees, bushes, and rocks (denser at forest edges)

### 4. Tile Palette
The palette prefab is at:  
`Assets/Tilemaps/Palettes/EnchantedForest_Palette.prefab`

Open it via **Window → 2D → Tile Palette** to manually paint additional tiles.

### Unity Version
Tested configuration: **Unity 6000.x**, **URP or Built-in RP**, **PPU = 100**, **Tile size = 16×16**.

---

## Map B: Mountain Mine (80×80)

### Scene
Open:  
`Assets/Scenes/Levels/OutDoors/OutDoors_B_MountainMine.unity`

### Scene Layout
| GameObject | Component | Sorting Order |
|---|---|---|
| `MountainMine_Grid` | Grid (cell 0.16 × 0.16) | – |
| └ `Ground` | Tilemap + TilemapRenderer | 0 |
| └ `Water` | Tilemap + TilemapRenderer | 10 |
| └ `Details` | Tilemap + TilemapRenderer | 20 |
| └ `Objects` | Tilemap + TilemapRenderer | 30 |

### Build the Map
From the Unity menu bar run:  
**Tools → Mountain Mine → Build Map B (80×80)**

This executes `Assets/Editor/CreateMountainMineMap.cs` which fills the tilemaps with:
- **Ground** – cave floor tiles (with subtle variation)
- **Water** – underground pool (cols 15–30, rows 25–55)
- **Details** – scattered cave floor accents
- **Objects** – rocky cliff border, scattered rocks, mine entrance arch

### Tile Assets
`Assets/Tilemaps/Tiles/MountainMine/`
- `CaveFloorTile_0`–`CaveFloorTile_4` (from muddy cave tileset)
- `RockTile_0`–`RockTile_3`
- `CaveWaterTile_0`, `CaveWaterTile_1`

---

## Map C: Cozy Town (120×120)

### Scene
Open:  
`Assets/Scenes/Levels/OutDoors/OutDoors_C_CozyTown.unity`

### Scene Layout
| GameObject | Component | Sorting Order |
|---|---|---|
| `CozyTown_Grid` | Grid (cell 0.16 × 0.16) | – |
| └ `Ground` | Tilemap + TilemapRenderer | 0 |
| └ `Water` | Tilemap + TilemapRenderer | 10 |
| └ `Details` | Tilemap + TilemapRenderer | 20 |
| └ `Objects` | Tilemap + TilemapRenderer | 30 |

### Build the Map
From the Unity menu bar run:  
**Tools → Cozy Town → Build Map C (120×120)**

This executes `Assets/Editor/CreateCozyTownMap.cs` which fills the tilemaps with:
- **Ground** – varied grass across 120×120, path tiles for roads and town square
- **Water** – small pond in the NW park area
- **Details** – flowers in the town square, along road edges, around the pond
- **Objects** – tree border, scattered trees in the park, fence rows in the SW farming area, houses in each quadrant (NE: normal houses, SW: farmer houses, SE: blacksmith/shops, NW: park houses)

### Layout Zones
| Zone | Location (approx) | Contents |
|---|---|---|
| Tree border | 3-tile perimeter | Dense tree/bush border |
| Town square | cols 50–70, rows 50–70 | Path tiles + flower accents |
| N-S road | cols 58–62, full height | Path tiles |
| E-W road | rows 58–62, full width | Path tiles |
| NW Park | cols 8–45, rows 65–95 | Grass + trees + pond + flowers |
| SW Farming | cols 8–55, rows 8–55 | Grass + farmer houses + fence rows |
| NE Market | cols 65–112, rows 65–112 | Grass + normal houses |
| SE Crafting | cols 65–112, rows 8–55 | Grass + blacksmith house |

### Tile Assets
`Assets/Tilemaps/Tiles/CozyTown/`
- `TownGrassTile_0`–`TownGrassTile_2` (grass variants)
- `TownPathTile_0`–`TownPathTile_2` (stone path/brick)
- `TownWaterTile_0`, `TownWaterTile_1`
- `TownFenceTile_0`, `TownFenceTile_1`
- `TownFlowerTile_0`, `TownFlowerTile_1`
- `TownTreeTile_0`, `TownTreeTile_1`
- `TownBushTile_0`
- `NormalHouseTile`, `FarmerHouseTile`, `BlacksmithHouseTile`

All CozyTown tiles reuse sprites that are already in the project (`Grass.png`, `Grass and Brick.png`, water/fence/flower/tree/bush/house PNGs from `Assets/Content/New Sprites and tiles/`).

### Unity Version
Tested configuration: **Unity 6000.x**, **URP or Built-in RP**, **PPU = 100**, **Tile size = 16×16**.
