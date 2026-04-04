# OutDoors – Map A: Enchanted Forest (80×80)

## Setup Instructions

### 1. Import Sprites
Copy / sync the following PNG files from the repo root into the same root of your Unity project:
- `Grass.png` (192×320 – 12×20 tiles)
- `Grass and Brick.png` (256×256 – 16×16 tiles)
- `Water_ground_1.png` (128×32 – 8×2 tiles)
- `Water_ground_2.png` (192×48 – 12×3 tiles)
- `bonus summer grass and dirt.png` (80×96 – 5×6 tiles)
- `fence_tileset.png` (64×64 – 4×4 tiles)
- `Maple Tree.png` (160×48 – 10×3 tiles)
- `bush_1–3.png`, `flower_1–7.png`, `rock_1–7.png`, `tree_1–4.png` (16×16 singles)

The accompanying `.meta` files configure each asset with:
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
