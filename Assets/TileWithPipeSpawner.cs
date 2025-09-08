using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Rendering;
// Optional: uncomment if you use TextMeshPro on the tile
// using TMPro;

public class TileWithPipesSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;      // your Tile prefab (square sprite + text)
    [SerializeField] private GameObject[] pipePrefabs;   // all your pipe prefabs

    [Header("Grid")]
    [SerializeField] private int columns = 6;
    [SerializeField] private int rows = 4;
    [SerializeField] private Vector2 cellSize = new Vector2(1.4f, 1.4f);
    [SerializeField] private bool centerGrid = true;

    [Header("Pipe Options")]
    [SerializeField] private bool randomizePipeRotation = true; // Z-rotation in 2D
    [SerializeField] private Vector3 pipeLocalOffset = Vector3.zero; // tweak if pipe doesn’t sit centered
    private Vector3 pipeLocalScale = new Vector3(1.7f, 1.7f, 1.7f);   // scale pipe inside tile if needed

    [Header("Pipe Colors")]
    [SerializeField] private Color[] pipePalette = new Color[]
    {
        new Color(0.95f, 0.25f, 0.25f),
        new Color(0.25f, 0.65f, 1.00f),
        new Color(0.25f, 0.85f, 0.40f),
        new Color(1.00f, 0.85f, 0.25f),
        new Color(0.85f, 0.35f, 0.95f),
        new Color(0.95f, 0.55f, 0.25f),
        new Color(0.65f, 0.65f, 0.65f)
    };

    [Header("Material Override")]
    [SerializeField] private bool overrideMaterialForPipes = true; // turn on to force Unlit
    [SerializeField] private Material pipeUnlitMaterial;            // assign Unlit/Color or URP/Unlit in Inspector

    [Header("Hierarchy")]
    [SerializeField] private Transform container; // optional parent for all tiles

    private System.Random rng;

    private void Reset()
    {
        container = transform;
    }

    private void Start()
    {
        rng = new System.Random();
        Regenerate();
    }

    [ContextMenu("Regenerate")]
    public void Regenerate()
    {
        if (!tilePrefab)
        {
            Debug.LogWarning("Assign your Tile prefab (with 2D sprite + text) to 'tilePrefab'.");
            return;
        }
        if (pipePrefabs == null || pipePrefabs.Length == 0)
        {
            Debug.LogWarning("Assign your pipe prefabs to 'pipePrefabs'.");
            return;
        }
        if (!container) container = transform;

        // clear previous
        ClearChildren(container);

        // compute origin so grid is centered if requested
        Vector2 origin = Vector2.zero;
        if (centerGrid)
        {
            var total = new Vector2((columns - 1) * cellSize.x, (rows - 1) * cellSize.y);
            origin = -0.5f * total;
        }

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < columns; x++)
        {
            Vector3 worldPos = new Vector3(origin.x + x * cellSize.x, origin.y + y * cellSize.y, 0f);
            var tileGO = Instantiate(tilePrefab, worldPos, Quaternion.identity, container);
            tileGO.name = $"Tile ({x},{y})";

            // find anchor inside tile (optional). If your Tile prefab has a child named "PipeAnchor", we use it.
            Transform anchor = tileGO.transform;
            var anchorT = tileGO.transform.Find("PipeAnchor");
            if (anchorT) anchor = anchorT;

            // pick a random pipe
            var pipePrefab = pipePrefabs[rng.Next(pipePrefabs.Length)];
            if (pipePrefab == null) continue;

            // instantiate pipe under the tile/anchor
            var pipeGO = Instantiate(pipePrefab, anchor);
            pipeGO.transform.localPosition = pipeLocalOffset;   // start near center
            pipeGO.transform.localScale = pipeLocalScale;

            // rotate in 2D around Z (multiples of 90 keeps it neat)
            float rotZ = randomizePipeRotation ? 90f * rng.Next(4) : 0f;
            pipeGO.transform.localRotation = Quaternion.Euler(0, 0, rotZ);

            // MATERIAL override first (prevents black pipes if scene lacks lights)
            OverrideMaterialAllLODs(pipeGO);

            // COLOR applied to every LOD renderer
            var color = pipePalette[rng.Next(pipePalette.Length)];
            ApplyColorAllLODs(pipeGO, color);

            // CENTER after any scaling/rotation so the visuals sit exactly on the tile
            AutoCenterByBoundsIncludingLODs(pipeGO, anchor);

            // OPTIONAL: write text on the tile (works with either TMP or legacy Text if present)
            TrySetTileText(tileGO, pipePrefab.name);
        }
    }

    private void TrySetTileText(GameObject tileGO, string text)
    {
        // Try TextMeshProUGUI / TextMeshPro
        #if TMP_PRESENT
        var tmps = tileGO.GetComponentsInChildren<TMPro.TMP_Text>(true);
        if (tmps != null && tmps.Length > 0)
        {
            foreach (var t in tmps) t.text = text;
            return;
        }
        #endif

        // Try legacy UI.Text
        var utexts = tileGO.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        if (utexts != null && utexts.Length > 0)
        {
            foreach (var t in utexts) t.text = text;
            return;
        }

        // Try TextMesh (3D)
        var tms = tileGO.GetComponentsInChildren<TextMesh>(true);
        if (tms != null && tms.Length > 0)
        {
            foreach (var t in tms) t.text = text;
        }
    }

    private void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(t.GetChild(i).gameObject);
            else
                Destroy(t.GetChild(i).gameObject);
            #else
            Destroy(t.GetChild(i).gameObject);
            #endif
        }
    }

    #region LOD-aware helpers

    // Call an action for every Renderer on the object, including all LODs.
    private void ForEachRendererIncludingLODs(GameObject root, Action<Renderer> action)
    {
        // 1) Regular renderers
        var direct = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in direct) action(r);

        // 2) LODGroup renderers (just in case some are disabled by default)
        var lodGroups = root.GetComponentsInChildren<LODGroup>(true);
        foreach (var lg in lodGroups)
        {
            foreach (var lod in lg.GetLODs())
            {
                if (lod.renderers == null) continue;
                foreach (var r in lod.renderers) if (r) action(r);
            }
        }
    }

    // Call an action for every Renderer in the first LOD only.
    private void ForEachRendererFirstLOD(GameObject root, Action<Renderer> action)
    {
        // 1) Regular renderers
        var direct = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in direct) action(r);

        // 2) Only first LOD (LOD0)
        var lodGroups = root.GetComponentsInChildren<LODGroup>(true);
        foreach (var lg in lodGroups)
        {
            var lods = lg.GetLODs();
            if (lods.Length > 0)
            {
                var firstLod = lods[0];
                if (firstLod.renderers != null)
                {
                    foreach (var r in firstLod.renderers) if (r) action(r);
                }
            }
        }
    }

    // Optional: force an Unlit material on first LOD renderers to avoid black pipes.
    private void OverrideMaterialAllLODs(GameObject pipeGO)
    {
        if (!overrideMaterialForPipes || pipeUnlitMaterial == null) return;

        ForEachRendererFirstLOD(pipeGO, r =>
        {
            // If a renderer has multiple materials, assign the unlit to all slots.
            var count = r.sharedMaterials?.Length ?? 0;
            if (count <= 1)
            {
                r.material = pipeUnlitMaterial;
            }
            else
            {
                var mats = Enumerable.Repeat(pipeUnlitMaterial, count).ToArray();
                r.materials = mats;
            }
        });
    }

    // Robust color apply that works for both SpriteRenderer and MeshRenderer across LODs.
    private void ApplyColorAllLODs(GameObject pipeGO, Color color)
    {
        // 2D sprites first
        var srs = pipeGO.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs) sr.color = color;

        // 3D meshes via MPB
        ForEachRendererIncludingLODs(pipeGO, r =>
        {
            if (!r.sharedMaterial) return;
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);

            // Common color slots
            if (r.sharedMaterial.HasProperty("_BaseColor")) mpb.SetColor("_BaseColor", color); // URP/HDRP
            if (r.sharedMaterial.HasProperty("_Color"))     mpb.SetColor("_Color", color);     // Built-in/Unlit/Simple

            r.SetPropertyBlock(mpb);
        });
    }

    // Center the pipe by bounds across ALL LOD renderers
    private void AutoCenterByBoundsIncludingLODs(GameObject pipeGO, Transform anchor)
    {
        var boundsWS = new Bounds(pipeGO.transform.position, Vector3.zero);
        bool hasAny = false;

        ForEachRendererIncludingLODs(pipeGO, r =>
        {
            if (!hasAny)
            {
                boundsWS = r.bounds;
                hasAny = true;
            }
            else
            {
                boundsWS.Encapsulate(r.bounds);
            }
        });

        if (!hasAny) return;

        // Convert world center to anchor local space and offset the pipe
        var centerLS = anchor.InverseTransformPoint(boundsWS.center);
        pipeGO.transform.localPosition -= centerLS;
    }
    #endregion
}
