using System.Collections.Generic;
using UnityEngine;

public class Simple3x3SceneBuilder : MonoBehaviour
{
    [Header("Prefabs (assign your tiles here)")]
    public GameObject tilePrefab; // Assign a numbered tile prefab

    [Header("Options")]
    public Transform container;          // Optional parent for spawned tiles
    public bool buildOnStart = true;     // Auto-build when scene starts
    public Vector2Int gridSize = new Vector2Int(3, 3); // Keep at 3x3 for this scene

    // Example layout (you can change these in the inspector)
    [Header("Example Layout (grid coords, origin at bottom-left)")]
    // Sliding puzzle state
    public GameObject[,] board;
    public Vector2Int emptyPos;

    // Keep track of what we spawned so we can rebuild cleanly
    private readonly List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        if (buildOnStart)
            Build();
    }

    [ContextMenu("Build Now")]
    public void Build()
    {
        if (!CheckPrefabs()) return;
        ClearSpawned();
        board = new GameObject[gridSize.y, gridSize.x]; // row-major: [y, x]
        List<int> numbers = new List<int>();
        for (int i = 1; i < gridSize.x * gridSize.y; i++) numbers.Add(i);
        Shuffle(numbers);
        int idx = 0;
        for (int y = 0; y < gridSize.y; y++)
        for (int x = 0; x < gridSize.x; x++)
        {
            if (idx < numbers.Count)
            {
                var go = SpawnTile(numbers[idx], new Vector2Int(x, y));
                board[y, x] = go;
                idx++;
            }
            else
            {
                board[y, x] = null;
                emptyPos = new Vector2Int(x, y);
            }
        }
        // Solvability check
        if (!IsSolvable(gridSize.x))
        {
            Debug.LogWarning("Generated puzzle is unsolvable! Rebuilding...");
            Build();
            return;
        }
        FitCameraToGrid();
        Debug.Log("Sliding puzzle built.");
    }

    // Solvability check for n x n sliding puzzle
    private bool IsSolvable(int n)
    {
        List<int> tiles = new List<int>();
        // Flip y-axis to match PuzzleSolverUI
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            var tile = board[n - 1 - y, x];
            if (tile != null)
            {
                int num = 0;
                var parts = tile.name.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[1], out num))
                    tiles.Add(num);
            }
        }
        int inversions = 0;
        for (int i = 0; i < tiles.Count; i++)
        for (int j = i + 1; j < tiles.Count; j++)
        {
            if (tiles[i] > tiles[j])
                inversions++;
        }
        // Find blank row in flipped
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

    private GameObject Spawn(GameObject prefab, Vector3 pos)
    {
    var go = Instantiate(prefab, pos, Quaternion.identity, container ? container : transform);
    spawned.Add(go);
    return go;
    }

    private void ClearSpawned()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i]) DestroyImmediate(spawned[i]);
        }
        spawned.Clear();

        // Also clear any previous children under the container (handy if you rebuilt)
        var parent = container ? container : transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }
    }

    public bool InBounds(Vector2Int p)
        => p.x >= 0 && p.y >= 0 && p.x < gridSize.x && p.y < gridSize.y;

    public void MoveTile(Vector2Int tilePos)
    {
        Debug.Log($"MoveTile called: moving tile at {tilePos} to empty slot at {emptyPos}");
        var tile = board[tilePos.y, tilePos.x];
        tile.transform.position = new Vector3(emptyPos.x, emptyPos.y, 0);
        board[emptyPos.y, emptyPos.x] = tile;
        board[tilePos.y, tilePos.x] = null;
        emptyPos = tilePos;
        Debug.Log($"MoveTile finished: new emptyPos is {emptyPos}");
    }

    public bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    private bool CheckPrefabs()
    {
        if (!tilePrefab)
        {
            Debug.LogError("Assign a numbered tile prefab in the inspector.");
            return false;
        }
        return true;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private GameObject SpawnTile(int number, Vector2Int pos)
    {
        var go = Instantiate(tilePrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, container ? container : transform);
        go.name = "Tile " + number;
        // Try TextMesh
        var text = go.GetComponentInChildren<TextMesh>();
        if (text)
        {
            text.text = number.ToString();
        }
        else
        {
            // Try TextMeshPro
            var tmp = go.GetComponentInChildren<TMPro.TextMeshPro>();
            if (tmp)
                tmp.text = number.ToString();
            else
                Debug.LogWarning($"No TextMesh or TextMeshPro found in tilePrefab '{go.name}'. Please add one as a child.");
        }
        float scaleFactor = 0.9f;
        go.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        spawned.Add(go);
        return go;
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
        // Draw a light grid gizmo for reference
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        for (int y = 0; y < gridSize.y; y++)
        for (int x = 0; x < gridSize.x; x++)
        {
            Gizmos.DrawWireCube(new Vector3(x, y, 0), Vector3.one);
        }
    }
#endif
}

