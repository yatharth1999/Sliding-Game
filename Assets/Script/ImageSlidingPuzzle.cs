using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;

public class ImageSlidingPuzzle : MonoBehaviour
{
    [Header("UI")]
    public Transform gridParent;   // panel with GridLayoutGroup
    public GameObject tilePrefab;  // the Tile prefab
    public int N = 3;              // 3..6

    [Header("Image Source")]
    public int imageIndex = 0;
    public List<Texture2D> imageTextures;
    [Tooltip("If true, will error when texture not readable. If false, will attempt slicing anyway.")]
    public bool requireReadable = false;

    [Header("Shuffle")]
    public int shuffleMoves = 60;  // random valid moves
    public int seed = 12345;       // fixed seed = reproducible

    [Header("AI Solver")]
    public SlidingPuzzleSolver solver; // Assign in Inspector

    [Header("Gameplay")]
    [SerializeField] private bool inputLocked = false;
    [SerializeField] private bool isAnimating = false;
    public UnityEvent onPuzzleSolved;

    public bool InputLocked => inputLocked;
    public bool IsAnimating => isAnimating;

    private List<PuzzleTile> tiles = new();
    private System.Random rng;
    private Coroutine activeSolve;
    private Coroutine hintDisplayCoroutine;

    public static ImageSlidingPuzzle inst;

    public Button nextButton;
    public Button hintButton;
    public TextMeshProUGUI solvedText;

    public Image hintImage;
    public TextMeshProUGUI hintText;
  
    public Image hintBubbleImage;
    public TextMeshProUGUI hintBubbleText;
    public TextMeshProUGUI timerText;

    void Awake()
    {
        if (inst == null)
            inst = this;
        else
            Destroy(gameObject);
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
    }

    void Start()
    {
        rng = new System.Random(seed);
        // Set N from GameMgr if available
        if (GameMgr.inst != null)
            N = GameMgr.inst.gameLevel;
        if (BuildFromTexture(imageTextures[imageIndex]))
            ShuffleSolvable();
        // ensure bubble starts hidden
        if (hintBubbleImage != null) hintBubbleImage.gameObject.SetActive(false);
    }

    public void SetN(int newN)
    {
        N = Mathf.Clamp(newN, 3, 6);
        Rebuild();
    }

    public void Rebuild()
    {
        if (BuildFromTexture(imageTextures[imageIndex]))
            ShuffleSolvable();
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
        if (solvedText != null)
            solvedText.gameObject.SetActive(false);
        if (hintButton != null)
            hintButton.gameObject.SetActive(true);
    }

    bool BuildFromTexture(Texture2D tex)
    {
        // clear old
        foreach (Transform c in gridParent) Destroy(c.gameObject);
        tiles.Clear();
        if (!tex)
        {
            return false;
        }
        if (!tex.isReadable)
        {
            if (requireReadable)
            {
                return false;
            }
        }

        // slice into N×N sprites
        var sprites = Slice(tex, N);

        // build tiles
        int total = N * N;
        for (int i = 0; i < total; i++)
        {
            var go = Instantiate(tilePrefab, gridParent);
            var tile = go.GetComponent<PuzzleTile>();
            if (tile == null)
            {
                continue;
            }

            tile.correctIndex = i;
            tile.currentIndex = i;
            tile.isBlank = (i == total - 1);
            tile.onClick = TryMove;

            if (tile.image == null) tile.image = go.GetComponent<Image>();
            if (tile.numberText == null) tile.numberText = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tile.image == null)
            {
                continue;
            }

            if (tile.isBlank) tile.image.enabled = false;
            else if (i < sprites.Length && sprites[i] != null) tile.image.sprite = sprites[i];

            if (!tile.isBlank && tile.numberText != null) tile.numberText.text = (tile.correctIndex + 1).ToString();
            else if (tile.numberText != null) tile.numberText.text = "";

            tiles.Add(tile);
        }
        return true;
    }

    Sprite[] Slice(Texture2D tex, int n)
    {
        int cw = tex.width / n;
        int ch = tex.height / n;
        var result = new Sprite[n * n];
        int k = 0;

        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
            {
                var rect = new Rect(c * cw, tex.height - (r + 1) * ch, cw, ch);
                result[k++] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            }
        return result;
    }

    // player click path
    public void TryMove(PuzzleTile t)
    {
        if (inputLocked || isAnimating || IsSolved()) return;
        var blank = tiles.Find(x => x.isBlank);
        if (!IsAdjacent(t.currentIndex, blank.currentIndex)) return;

        SwapTiles(t, blank);
        if (IsSolved()) OnSolved();
    }

    // shuffle by making valid blank moves from the goal
    void ShuffleSolvable()
    {
        if (tiles.Count == 0)
        {
            return;
        }
        var blank = tiles.Find(x => x.isBlank);
        if (blank == null)
        {
            return;
        }
        for (int i = 0; i < shuffleMoves; i++)
        {
            var opts = Neighbors(blank.currentIndex);
            if (opts.Count == 0) break;
            var chosen = opts[rng.Next(opts.Count)];
            SwapTiles(chosen, blank);
        }
    }

    void SwapTiles(PuzzleTile a, PuzzleTile b)
    {
        // swap siblings (visual position in Grid)
        int ia = a.transform.GetSiblingIndex();
        int ib = b.transform.GetSiblingIndex();
        a.transform.SetSiblingIndex(ib);
        b.transform.SetSiblingIndex(ia);

        // swap indices (logical)
        (a.currentIndex, b.currentIndex) = (b.currentIndex, a.currentIndex);
    }

    bool IsAdjacent(int a, int b)
    {
        int ra = a / N, ca = a % N;
        int rb = b / N, cb = b % N;
        return Mathf.Abs(ra - rb) + Mathf.Abs(ca - cb) == 1;
    }

    List<PuzzleTile> Neighbors(int idx)
    {
        var res = new List<PuzzleTile>();
        int r = idx / N, c = idx % N;

        foreach (var t in tiles)
        {
            int tr = t.currentIndex / N, tc = t.currentIndex % N;
            if (Mathf.Abs(tr - r) + Mathf.Abs(tc - c) == 1) res.Add(t);
        }
        return res;
    }

    bool IsSolved()
    {
        foreach (var t in tiles)
            if (!t.isBlank && t.currentIndex != t.correctIndex) return false;
        return true;
    }

    void OnSolved()
    {
        onPuzzleSolved?.Invoke();
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);
        // Stop the timer
        if (PanelSwitcher.inst != null)
            PanelSwitcher.inst.StopTimer();
        // Optional: reveal full image, play sound, advance story, etc.
    }

    public void SolvePuzzle()
    {
        if (activeSolve != null)
        {
            StopCoroutine(activeSolve);
            activeSolve = null;
            inputLocked = false;
            isAnimating = false;
        }
        if (solver == null)
        {
            return;
        }
        int[,] board = BuildBoard();
        int[,] goal = BuildGoal();
        List<string> solution = null;
        if (GameMgr.inst.solverType == GameMgr.aiSolverType.BFS)
            solution = solver.SolveBFS(board, GetEmptyPos(), goal, N);
        else if (GameMgr.inst.solverType == GameMgr.aiSolverType.DFS)
            solution = solver.SolveDFS(board, GetEmptyPos(), goal, 20, N);
        else
            solution = solver.SolveAStar(board, GetEmptyPos(), goal, N);
        if (solution == null)
        {
            return;
        }
        activeSolve = StartCoroutine(AnimateSolution(solution));
    }

    private int[,] BuildBoard()
    {
        int[,] board = new int[N, N];
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                int idx = y * N + x;
                var tile = tiles.Find(t => t.currentIndex == idx);
                board[y, x] = tile.isBlank ? 0 : tile.correctIndex + 1;
            }
        return board;
    }

    private int[,] BuildGoal()
    {
        int[,] goal = new int[N, N];
        int idx = 1;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                if (idx < N * N)
                    goal[y, x] = idx++;
                else
                    goal[y, x] = 0;
            }
        return goal;
    }

    private Vector2Int GetEmptyPos()
    {
        var blank = tiles.Find(t => t.isBlank);
        int y = blank.currentIndex / N;
        int x = blank.currentIndex % N;
        return new Vector2Int(x, y);
    }

    private System.Collections.IEnumerator AnimateSolution(List<string> moves)
    {
        inputLocked = true;
        for (int i = 0; i < moves.Count; i++)
        {
            MoveByDirection(moves[i]);
            while (isAnimating) yield return null;
            yield return new WaitForSeconds(0.1f);
        }
        inputLocked = false;
        activeSolve = null;
    }

    private void MoveByDirection(string dir)
    {
        var blank = tiles.Find(t => t.isBlank);
        int by = blank.currentIndex / N;
        int bx = blank.currentIndex % N;
        int ty = by, tx = bx;
        switch (dir)
        {
            case "Up": ty--; break;
            case "Down": ty++; break;
            case "Left": tx--; break;
            case "Right": tx++; break;
        }
        if (ty >= 0 && ty < N && tx >= 0 && tx < N)
        {
            int tIdx = ty * N + tx;
            var tile = tiles.Find(t => t.currentIndex == tIdx);
            if (tile != null && !tile.isBlank)
            {
                SwapTiles(tile, blank);
            }
        }
    }
    public void HintButton()
    {
        if (!IsReady())
        {
            ShowHint("Puzzle not ready");
            return;
        }

        // Stop any existing hint display
        if (hintDisplayCoroutine != null)
        {
            StopCoroutine(hintDisplayCoroutine);
            hintDisplayCoroutine = null;
        }

        // Build states for solver
        int[] currentState = new int[tiles.Count];
        int[] goalState = new int[tiles.Count];
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles.Find(t => t.currentIndex == i);
            currentState[i] = tile.isBlank ? 0 : tile.correctIndex + 1;
            goalState[i] = (i == tiles.Count - 1) ? 0 : (i + 1);
        }

        // Convert to int[,]
        int[,] board = new int[N, N];
        int[,] goal = new int[N, N];
        for (int i = 0; i < currentState.Length; i++)
        {
            int y = i / N;
            int x = i % N;
            board[y, x] = currentState[i];
            goal[y, x] = goalState[i];
        }

        // Choose solver
        List<string> solution = null;
        if (GameMgr.inst.solverType == GameMgr.aiSolverType.BFS)
            solution = solver.SolveBFS(board, GetEmptyPos(), goal, N);
        else if (GameMgr.inst.solverType == GameMgr.aiSolverType.DFS)
            solution = solver.SolveDFS(board, GetEmptyPos(), goal, 20, N);
        else
            solution = solver.SolveAStar(board, GetEmptyPos(), goal, N);

        if (solution == null || solution.Count == 0)
        {
            ShowHint("No solution found");
            return;
        }

        // Figure out which tile moves into the blank for the first step
        string bestMove = solution[0]; // "Up", "Down", "Left", "Right"
        var blank = tiles.Find(t => t.isBlank);
        if (blank == null)
        {
            ShowHint("No blank tile!");
            return;
        }

        int by = blank.currentIndex / N;
        int bx = blank.currentIndex % N;
        int ty = by, tx = bx;

        switch (bestMove)
        {
            case "Up":    ty--; break;  // the tile above slides down into the blank
            case "Down":  ty++; break;
            case "Left":  tx--; break;
            case "Right": tx++; break;
        }

        // bounds check
        if (ty < 0 || ty >= N || tx < 0 || tx >= N)
        {
            ShowHint("Invalid next move");
            return;
        }

        int targetIdx = ty * N + tx;
        var tileToMove = tiles.Find(t => t.currentIndex == targetIdx);
        if (tileToMove == null)
        {
            ShowHint("Target tile not found");
            return;
        }

        // simple, fixed-position message
        string tileDir = "";
        switch (bestMove)
        {
            case "Up": tileDir = "down"; break;
            case "Down": tileDir = "up"; break;
            case "Left": tileDir = "right"; break;
            case "Right": tileDir = "left"; break;
        }
        string msg = $"Shade: move tile {tileToMove.correctIndex + 1} {tileDir}";
        ShowHint(msg);
    }
    public void HideHint()
    {
        if (hintBubbleImage != null)
            hintBubbleImage.gameObject.SetActive(false);
        if (hintDisplayCoroutine != null)
        {
            StopCoroutine(hintDisplayCoroutine);
            hintDisplayCoroutine = null;
        }
    }

    private void ShowHint(string text)
    {
        if (hintBubbleImage == null || hintBubbleText == null) return;
        hintBubbleText.text = text;
        hintBubbleImage.gameObject.SetActive(true);
        // Start coroutine to hide after 5 seconds
        if (hintDisplayCoroutine != null) StopCoroutine(hintDisplayCoroutine);
        hintDisplayCoroutine = StartCoroutine(HideHintAfterDelay(5f));
    }

    private System.Collections.IEnumerator HideHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideHint();
        hintDisplayCoroutine = null;
    }

    private bool IsReady()
    {
        return N > 1 && tiles != null && tiles.Count == N * N && GameMgr.inst != null;
    }
    public void NextButton()
    {
        imageIndex = imageIndex + 1;
        StartCoroutine(TypewriterEffect.inst.ShowText());
        nextButton.gameObject.SetActive(false);
        PanelSwitcher.inst.ShowGamePanel();
    }
}
