using System.Collections.Generic;
using UnityEngine;

public class SlidingPuzzleSolver : MonoBehaviour
{
    // Represents a puzzle state for BFS/DFS/A*
    public class PuzzleState
    {
        public int[,] board;
        public Vector2Int emptyPos;
        public List<string> moves; // Move history
        private int n;
        public int g; // cost so far
        public int h; // heuristic
        public int f; // g + h
        public PuzzleState parent; // for path reconstruction

        

        public PuzzleState(int[,] board, Vector2Int emptyPos, List<string> moves = null, int n = 3, int g = 0, PuzzleState parent = null)
        {
            this.board = (int[,])board.Clone();
            this.emptyPos = emptyPos;
            this.moves = moves != null ? new List<string>(moves) : new List<string>();
            this.n = n;
            this.g = g;
            this.parent = parent;
            this.h = ManhattanDistance(board, n);
            this.f = g + h;
        }

        // For hashing and comparison
        public override bool Equals(object obj)
        {
            if (obj is PuzzleState other)
            {
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                    if (board[y, x] != other.board[y, x]) return false;
                return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            int hash = 17;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
                hash = hash * 31 + board[y, x];
            return hash;
        }
    }

    // Directions: up, down, left, right
    private static readonly Vector2Int[] directions = {
        new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0)
    };
    private static readonly string[] dirNames = { "Up", "Down", "Left", "Right" };

    // Manhattan distance heuristic
    private static int ManhattanDistance(int[,] board, int n)
    {
        int distance = 0;
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            int value = board[y, x];
            if (value != 0)
            {
                int targetX = (value - 1) % n;
                int targetY = (value - 1) / n;
                distance += Mathf.Abs(x - targetX) + Mathf.Abs(y - targetY);
            }
        }
        return distance;
    }

    // Simple Priority Queue
    public class PriorityQueue<T>
    {
        public List<T> list = new List<T>();
        private System.Func<T, int> priorityFunc;

        public PriorityQueue(System.Func<T, int> priorityFunc)
        {
            this.priorityFunc = priorityFunc;
        }

        public void Enqueue(T item)
        {
            list.Add(item);
            list.Sort((a, b) => priorityFunc(a).CompareTo(priorityFunc(b)));
        }

        public T Dequeue()
        {
            T item = list[0];
            list.RemoveAt(0);
            return item;
        }

        public int Count => list.Count;
    }

    // BFS solver
    public List<string> SolveBFS(int[,] startBoard, Vector2Int startEmpty, int[,] goalBoard, int n = 3)
    {
        var start = new PuzzleState(startBoard, startEmpty, null, n, 0, null);
        // Find goal empty position
        Vector2Int goalEmpty = Vector2Int.zero;
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
            if (goalBoard[y, x] == 0) goalEmpty = new Vector2Int(x, y);
        var goal = new PuzzleState(goalBoard, goalEmpty, null, n, 0, null);
        var queue = new Queue<PuzzleState>();
        var visited = new HashSet<PuzzleState>();
        queue.Enqueue(start);
        visited.Add(start);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.Equals(goal)) return current.moves;
            for (int d = 0; d < directions.Length; d++)
            {
                Vector2Int newEmpty = current.emptyPos + directions[d];
                if (newEmpty.x < 0 || newEmpty.x >= n || newEmpty.y < 0 || newEmpty.y >= n) continue;
                int[,] newBoard = (int[,])current.board.Clone();
                newBoard[current.emptyPos.y, current.emptyPos.x] = newBoard[newEmpty.y, newEmpty.x];
                newBoard[newEmpty.y, newEmpty.x] = 0;
                var next = new PuzzleState(newBoard, newEmpty, current.moves, n, current.g + 1, current);
                next.moves.Add(dirNames[d]);
                if (!visited.Contains(next))
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
        return null; // No solution
    }

    // DFS solver (limited depth)
    public List<string> SolveDFS(int[,] startBoard, Vector2Int startEmpty, int[,] goalBoard, int maxDepth = 20, int n = 3)
    {
        var start = new PuzzleState(startBoard, startEmpty, null, n, 0, null);
        // Find goal empty position
        Vector2Int goalEmpty = Vector2Int.zero;
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
            if (goalBoard[y, x] == 0) goalEmpty = new Vector2Int(x, y);
        var goal = new PuzzleState(goalBoard, goalEmpty, null, n, 0, null);
        var stack = new Stack<PuzzleState>();
        var visited = new HashSet<PuzzleState>();
        stack.Push(start);
        visited.Add(start);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.Equals(goal)) return current.moves;
            if (current.moves.Count >= maxDepth) continue;
            for (int d = 0; d < directions.Length; d++)
            {
                Vector2Int newEmpty = current.emptyPos + directions[d];
                if (newEmpty.x < 0 || newEmpty.x >= n || newEmpty.y < 0 || newEmpty.y >= n) continue;
                int[,] newBoard = (int[,])current.board.Clone();
                newBoard[current.emptyPos.y, current.emptyPos.x] = newBoard[newEmpty.y, newEmpty.x];
                newBoard[newEmpty.y, newEmpty.x] = 0;
                var next = new PuzzleState(newBoard, newEmpty, current.moves, n, current.g + 1, current);
                next.moves.Add(dirNames[d]);
                if (!visited.Contains(next))
                {
                    stack.Push(next);
                    visited.Add(next);
                }
            }
        }
        return null; // No solution within depth
    }

    // A* solver
    public List<string> SolveAStar(int[,] startBoard, Vector2Int startEmpty, int[,] goalBoard, int n = 3)
    {
        var start = new PuzzleState(startBoard, startEmpty, null, n, 0, null);
        // Find goal empty position
        Vector2Int goalEmpty = Vector2Int.zero;
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
            if (goalBoard[y, x] == 0) goalEmpty = new Vector2Int(x, y);
        var goal = new PuzzleState(goalBoard, goalEmpty, null, n, 0, null);
        var open = new PriorityQueue<PuzzleState>(s => s.f);
        var closed = new HashSet<PuzzleState>();
        open.Enqueue(start);
        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (current.Equals(goal)) return current.moves;
            closed.Add(current);
            for (int d = 0; d < directions.Length; d++)
            {
                Vector2Int newEmpty = current.emptyPos + directions[d];
                if (newEmpty.x < 0 || newEmpty.x >= n || newEmpty.y < 0 || newEmpty.y >= n) continue;
                int[,] newBoard = (int[,])current.board.Clone();
                newBoard[current.emptyPos.y, current.emptyPos.x] = newBoard[newEmpty.y, newEmpty.x];
                newBoard[newEmpty.y, newEmpty.x] = 0;
                var next = new PuzzleState(newBoard, newEmpty, current.moves, n, current.g + 1, current);
                next.moves.Add(dirNames[d]);
                if (closed.Contains(next)) continue;
                // Check if already in open with lower or equal f
                bool skip = false;
                foreach (var item in open.list)
                {
                    if (item.Equals(next) && item.f <= next.f)
                    {
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                    open.Enqueue(next);
            }
        }
        return null; // No solution
    }
}
