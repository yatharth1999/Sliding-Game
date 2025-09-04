using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleSolverUI : MonoBehaviour
{
    public SlidingPuzzleSolver solver;
    public Simple3x3SceneBuilder puzzleManager;
    public Button solveButton;
    public enum SearchType { BFS, DFS, AStar }
    public SearchType searchType = SearchType.BFS;
    public int dfsMaxDepth = 20;

    void Start()
    {
        if (solveButton != null)
            solveButton.onClick.AddListener(OnSolveClicked);
    }

    void OnSolveClicked()
    {
        int n = puzzleManager.gridSize.x;
        int[,] board = new int[n, n];
        Vector2Int empty = Vector2Int.zero;
        // Read current board from puzzleManager (flip y for solver)
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            var tile = puzzleManager.board[n - 1 - y, x]; // Flip y
            if (tile == null)
            {
                board[y, x] = 0;
                empty = new Vector2Int(x, y);
            }
            else
            {
                int num = 0;
                var parts = tile.name.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[1], out num))
                    board[y, x] = num;
                else
                    board[y, x] = 0;
            }
        }
        // Print board state (row-major, flipped y)
        Debug.Log("Current board state:");
        for (int y = 0; y < n; y++)
        {
            string row = "";
            for (int x = 0; x < n; x++)
                row += board[y, x] + " ";
            Debug.Log(row);
        }
        // Goal state (row-major)
        int[,] goal = new int[n, n];
        int idx = 1;
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            if (idx < n * n)
                goal[y, x] = idx++;
            else
                goal[y, x] = 0;
        }
        // Print goal state
        Debug.Log("Goal board state:");
        for (int y = 0; y < n; y++)
        {
            string row = "";
            for (int x = 0; x < n; x++)
                row += goal[y, x] + " ";
            Debug.Log(row);
        }
        // Solvability check
        if (!IsSolvable(board, n))
        {
            Debug.LogWarning("This puzzle configuration is unsolvable!");
            return;
        }
        List<string> solution = null;
        if (searchType == SearchType.BFS)
            solution = solver.SolveBFS(board, empty, goal, n);
        else if (searchType == SearchType.DFS)
            solution = solver.SolveDFS(board, empty, goal, dfsMaxDepth, n);
        else
            solution = solver.SolveAStar(board, empty, goal, n);
        if (solution != null)
        {
            Debug.Log($"Solution found: {string.Join(", ", solution)}");
            StartCoroutine(AnimateSolution(solution, n));
        }
        else
        {
            Debug.Log("No solution found.");
        }
    }

    // Solvability check for n x n sliding puzzle
    private bool IsSolvable(int[,] board, int n)
    {
        List<int> tiles = new List<int>();
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            if (board[y, x] != 0)
                tiles.Add(board[y, x]);
        }
        int inversions = 0;
        for (int i = 0; i < tiles.Count; i++)
        for (int j = i + 1; j < tiles.Count; j++)
        {
            if (tiles[i] > tiles[j])
                inversions++;
        }
        // Find blank row
        int blankRow = 0;
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            if (board[y, x] == 0)
                blankRow = y;
        }
        if (n % 2 == 1)
            return inversions % 2 == 0;
        else
            return (inversions + blankRow) % 2 == 0;
    }

    private System.Collections.IEnumerator AnimateSolution(List<string> moves, int n)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            yield return new WaitForSeconds(0.5f); // Wait between moves
            MovePuzzle(moves[i]);
        }
        // Print final board state after solving (flip y)
        Debug.Log("Final board state after solving:");
        for (int y = 0; y < n; y++)
        {
            string row = "";
            for (int x = 0; x < n; x++)
            {
                var tile = puzzleManager.board[n - 1 - y, x]; // Flip y
                if (tile == null)
                    row += "0 ";
                else
                {
                    int num = 0;
                    var parts = tile.name.Split(' ');
                    if (parts.Length > 1 && int.TryParse(parts[1], out num))
                        row += num + " ";
                    else
                        row += "? ";
                }
            }
            Debug.Log(row);
        }
    }

    private void MovePuzzle(string move)
    {
        Vector2Int dir = Vector2Int.zero;
        // Use correct inverse directions to match solver logic
        switch (move)
        {
            case "Up": dir = new Vector2Int(0, -1); break; // Empty slot moved up, so tile below moved.
            case "Down": dir = new Vector2Int(0, 1); break;  // Empty slot moved down, so tile above moved.
            case "Left": dir = new Vector2Int(-1, 0); break;  // Empty slot moved left, so tile to the right moved? Wait, no: tile to the left moved.
            case "Right": dir = new Vector2Int(1, 0); break; // Empty slot moved right, so tile to the left moved? Wait, no: tile to the right moved.
        }
        Vector2Int tilePos = puzzleManager.emptyPos + dir;
        if (puzzleManager.InBounds(tilePos) && puzzleManager.board[tilePos.y, tilePos.x] != null)
        {
            puzzleManager.MoveTile(tilePos);
        }
    }
}
