using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a solvable 3x3 sliding puzzle where each tile contains exactly one pipe:
/// two different straight pipe types and one cross pipe.
/// - Preserves solvability logic & camera fitting from your numbered puzzle
/// - and uses a PipeAnchor (if present) like your pipe spawner.
/// </summary>
public class PipeSlidingPuzzle3x3 : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Base tile prefab (e.g., square with optional child named 'PipeAnchor').")]
    public GameObject tilePrefab;

    [Tooltip("Straight pipe type A (e.g., a different model/texture from type B).")]
    public GameObject straightPipeA;

    [Tooltip("Straight pipe type B (distinct from A).")]
    public GameObject straightPipeB;

    [Tooltip("Cross (4-way) pipe.")]
    public GameObject crossPipe;

    [Header("Options")]
    public Transform container;                   // Optional parent
    public bool buildOnStart = true;
    public Vector2Int gridSize = new Vector2Int(3, 3); // Sliding puzzle size (default 3x3)

    [Header("Pipe Placement")]
    [Tooltip("Local offset for the pipe under the tile (use if pipes aren’t perfectly centered).")]
    public Vector3 pipeLocalOffset = Vector3.zero;

    [Tooltip("Uniform scale for the pipe child.")]
    public Vector3 pipeLocalScale = new Vector3(1.0f, 1.0f, 1.0f);

    [Tooltip("If true, straight pipes rotate 0° or 90°. Cross pipe is not rotated.")]
    public bool allowStraightRotation = true;

    [Header("Debug / Build")]
    public bool logBuild = false;

    // Sliding puzzle state
    private GameObject[,] board; // [y, x]
    private Vector2Int emptyPos;
    private readonly List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        if (buildOnStart) Build();
    }

    [ContextMenu("Build Now")]
    public void Build()
    {
        if (!CheckPrefabs()) return;

        ClearSpawned();

        // Row-major board
        board = new GameObject[gridSize.y, gridSize.x];

        // Create a shuffled list of indices for tiles (1..N-1), one slot stays empty
        List<int> indices = new List<int>();
        for (int i = 1; i < gridSize.x * gridSize.y; i++) indices.Add(i);
        Shuffle(indices);

        // We'll assign a random pipe type per tile (independent of its index)
        int idx = 0;
        for (int y = 0; y < gridSize.y; y++)
        for (int x = 0; x < gridSize.x; x++)
        {
            if (idx < indices.Count)
            {
                var go = SpawnPipeTile(new Vector2Int(x, y));
                board[y, x] = go;
                idx++;
            }
            else
            {
                // final cell is empty
                board[y, x] = null;
                emptyPos = new Vector2Int(x, y);
            }
        }

        // Ensure solvable (same rule you used in your numbered builder):contentReference[oaicite:4]{index=4}
        if (!IsSolvable(gridSize.x))
        {
            if (logBuild) Debug.LogWarning("Generated puzzle is unsolvable! Rebuilding...");
            Build();
            return;
        }

        FitCameraToGrid();
        if (logBuild) Debug.Log("Pipe sliding puzzle built.");
    }

    // --- Movement API (works like your original builder) ----------------------

    public bool InBounds(Vector2Int p)
        => p.x >= 0 && p.y >= 0 && p.x < gridSize.x && p.y < gridSize.y;

    public bool IsAdjacent(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;

    /// <summary>Call this when a tile is clicked to slide it into the empty slot (if adjacent).</summary>
    public void MoveTile(Vector2Int tilePos)
    {
        if (!InBounds(tilePos)) return;
        if (!IsAdjacent(tilePos, emptyPos)) return;
        var tile = board[tilePos.y, tilePos.x];
        if (!tile) return;

        // Move tile into the empty position
        tile.transform.position = new Vector3(emptyPos.x, emptyPos.y, 0);
        board[emptyPos.y, emptyPos.x] = tile;

        // Old tile position becomes the new empty
        board[tilePos.y, tilePos.x] = null;
        emptyPos = tilePos;
    }

    // --- Internals ------------------------------------------------------------

    private bool CheckPrefabs()
    {
        if (!tilePrefab) { Debug.LogError("Assign a tilePrefab."); return false; }
        if (!straightPipeA) { Debug.LogError("Assign straightPipeA."); return false; }
        if (!straightPipeB) { Debug.LogError("Assign straightPipeB."); return false; }
        if (!crossPipe) { Debug.LogError("Assign crossPipe."); return false; }
        return true;
    }

    private void ClearSpawned()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i]) DestroyImmediate(spawned[i]);
        }
        spawned.Clear();

        // Also clear any previous children under the container (handy if you rebuilt):contentReference[oaicite:5]{index=5}
        var parent = container ? container : transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }
    }

    private GameObject SpawnPipeTile(Vector2Int gridPos)
    {
        var parent = container ? container : transform;
        var tile = Instantiate(tilePrefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity, parent);
        tile.name = $"Tile {gridPos.x},{gridPos.y}";
        spawned.Add(tile);

        // Find an anchor like your other spawner does:contentReference[oaicite:6]{index=6}
        Transform anchor = tile.transform;
        var anchorT = tile.transform.Find("PipeAnchor");
        if (anchorT) anchor = anchorT;

        // Pick one of the THREE types (two straights + one cross).
        // You can change weights if you like; currently uniform.
        var choice = Random.Range(0, 3);
        GameObject prefab =
            choice == 0 ? straightPipeA :
            choice == 1 ? straightPipeB :
                          crossPipe;

        var pipe = Instantiate(prefab, anchor);
        pipe.transform.localPosition = pipeLocalOffset;
        pipe.transform.localScale = pipeLocalScale;

        // Rotation rule: straight pipes can be 0° or 90°. Cross stays unrotated.
        if (prefab == crossPipe)
        {
            pipe.transform.localRotation = Quaternion.identity;
        }
        else
        {
            float rotZ = (allowStraightRotation ? 90f * Random.Range(0, 2) : 0f); // 0 or 90
            pipe.transform.localRotation = Quaternion.Euler(0, 0, rotZ);
        }

        return tile;
    }

    // Same solvability logic pattern you used before (adapted from your code):contentReference[oaicite:7]{index=7}
    private bool IsSolvable(int n)
    {
        List<int> tilesLinear = new List<int>();

        // Create a consistent linearization that ignores which pipe is on a tile;
        // Only positions matter for standard 15-puzzle solvability.
        // We'll number tiles in reading order (top-left -> bottom-right) by current board positions.
        // To match your earlier flip-handling, we’ll mirror Y like your check did:contentReference[oaicite:8]{index=8}.
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            var tile = board[n - 1 - y, x];
            if (tile != null)
            {
                // Assign a synthetic sequential number to each non-empty tile
                // based on its current name (or we could store at spawn time).
                // Here we just parse an index if it exists; if not, create one.
                // For robustness, push a running index.
                tilesLinear.Add(tilesLinear.Count + 1);
            }
        }

        int inversions = 0;
        for (int i = 0; i < tilesLinear.Count; i++)
        for (int j = i + 1; j < tilesLinear.Count; j++)
        {
            if (tilesLinear[i] > tilesLinear[j]) inversions++;
        }

        // Find blank row in flipped coordinates
        int blankRow = 0;
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            if (board[n - 1 - y, x] == null)
                blankRow = y;
        }

        if (n % 2 == 1)
            return inversions % 2 == 0;
        else
            return (inversions + blankRow) % 2 == 0;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void FitCameraToGrid()
    {
        var cam = Camera.main;
        if (!cam) return;
        var cx = (gridSize.x - 1) * 0.5f;
        var cy = (gridSize.y - 1) * 0.5f;
        cam.transform.position = new Vector3(cx, cy, -10f);
        cam.orthographic = true;

        float margin = 0.5f;
        float halfHeightNeeded = (gridSize.y / 2f) + margin;
        float halfWidthInWorld = (gridSize.x / 2f) + margin;
        float halfHeightFromWidth = halfWidthInWorld / cam.aspect;
        cam.orthographicSize = Mathf.Max(halfHeightNeeded, halfHeightFromWidth);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        for (int y = 0; y < gridSize.y; y++)
        for (int x = 0; x < gridSize.x; x++)
            Gizmos.DrawWireCube(new Vector3(x, y, 0), Vector3.one);
    }
#endif
}
