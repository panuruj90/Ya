// Assets/Editor/ExportMountainMinePNG.cs
// Unity 6000.x  –  Menu: Tools > Mountain Mine > Export PNG
//
// Renders the MountainMine tilemap scene to a PNG file saved under:
//   Assets/Scenes/Levels/OutDoors/Exports/MountainMine_export_YYYYMMDD_HHmmss.png
//
// Usage:
//   1. Open the scene Assets/Scenes/Levels/OutDoors/OutDoors_B_MountainMine.unity
//   2. Run Tools > Mountain Mine > Build Map B (80x80)  (if not already built)
//   3. Run Tools > Mountain Mine > Export PNG
//
// The export captures the full 80×80 tile area at native tile resolution
// (80 tiles × 16 px/tile = 1280×1280 px) using a temporary orthographic camera
// and a RenderTexture. The result is written as a 32-bit RGBA PNG.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace OutDoors.Editor
{
    public static class ExportMountainMinePNG
    {
        private const int TILES  = 80;           // 80×80 map
        private const int PPU    = 100;          // pixels per unit
        private const float CELL = 0.16f;        // 0.16 units per tile (16px @ 100 PPU)
        private const string EXPORT_FOLDER = "Assets/Scenes/Levels/OutDoors/Exports";

        [MenuItem("Tools/Mountain Mine/Export PNG")]
        public static void ExportPNG()
        {
            // ── 1. Resolve the tilemap bounds ───────────────────────────────
            // Find Ground tilemap to determine actual painted bounds
            Tilemap groundTm = null;
            foreach (var tm in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                if (tm.name == "Ground") { groundTm = tm; break; }
            }

            // Compute world-space bounds of the 80×80 grid
            float mapWorldSize = TILES * CELL;   // 80 * 0.16 = 12.8 units
            Vector3 originWorld = Vector3.zero;  // bottom-left of grid

            if (groundTm != null)
            {
                // Use the tilemap's own cell bounds if it has painted tiles
                BoundsInt cellBounds = groundTm.cellBounds;
                if (cellBounds.size.x > 0 && cellBounds.size.y > 0)
                {
                    originWorld = groundTm.CellToWorld(cellBounds.min);
                    mapWorldSize = Mathf.Max(cellBounds.size.x, cellBounds.size.y) * CELL;
                }
            }

            // ── 2. Pixel dimensions ─────────────────────────────────────────
            // 80 tiles × 16 px = 1280 px, but clamp to max 4096 for safety
            int texW = Mathf.Min(Mathf.RoundToInt(mapWorldSize * PPU), 4096);
            int texH = texW;

            // ── 3. Create temporary RenderTexture + Camera ──────────────────
            var rt = new RenderTexture(texW, texH, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 1;

            var camGo = new GameObject("__MountainMineExportCam__");
            var cam   = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = mapWorldSize * 0.5f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.1f, 0.08f, 0.06f, 1f);
            cam.cullingMask      = ~0;  // render everything
            cam.targetTexture    = rt;

            // Position camera at centre of map
            Vector3 centre = originWorld + new Vector3(mapWorldSize * 0.5f, mapWorldSize * 0.5f, -10f);
            cam.transform.position = centre;

            // ── 4. Render ───────────────────────────────────────────────────
            cam.Render();

            // ── 5. Read pixels ──────────────────────────────────────────────
            RenderTexture prevActive = RenderTexture.active;
            RenderTexture.active = rt;

            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, texW, texH), 0, 0);
            tex.Apply();

            RenderTexture.active = prevActive;

            // ── 6. Clean up temporary objects ───────────────────────────────
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(camGo);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);

            // ── 7. Save PNG ─────────────────────────────────────────────────
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename  = $"MountainMine_export_{timestamp}.png";

            // Ensure the Exports folder exists on disk
            string exportDirFull = Path.Combine(
                Application.dataPath,
                "Scenes/Levels/OutDoors/Exports");
            Directory.CreateDirectory(exportDirFull);

            string fullPath = Path.Combine(exportDirFull, filename);
            byte[] pngBytes = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngBytes);
            UnityEngine.Object.DestroyImmediate(tex);

            // ── 8. Import into AssetDatabase ────────────────────────────────
            string assetPath = $"{EXPORT_FOLDER}/{filename}";
            AssetDatabase.Refresh();

            Debug.Log($"[Mountain Mine] PNG exported ({texW}×{texH} px) → {fullPath}");
            EditorUtility.RevealInFinder(fullPath);
        }
    }
}
