using UnityEngine;

public class TouchMoveTile : MonoBehaviour
{
    private Simple3x3SceneBuilder puzzleManager;

    void Start()
    {
        // Find the puzzle manager in the scene
        puzzleManager = FindObjectOfType<Simple3x3SceneBuilder>();
    }

    void OnMouseDown()
    {
        // Raycast from mouse position for 2D collider
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        Collider2D col = Physics2D.OverlapPoint(mousePos2D);
        if (col != null)
        {
            Debug.Log($"2D Raycast hit: {col.gameObject.name}");
            if (col.gameObject == this.gameObject)
            {
                Debug.Log($"Mouse clicked on tile: {gameObject.name}");
                TryMove();
            }
        }
        else
        {
            Debug.Log("2D Raycast did not hit any object.");
        }
    }

    void Update()
    {
        // For mobile or touch screens
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(touch.position);
                Vector2 touchPos = new Vector2(wp.x, wp.y);
                Collider2D col = Physics2D.OverlapPoint(touchPos);
                if (col != null && col.gameObject == this.gameObject)
                {
                    Debug.Log($"Touch detected on tile: {gameObject.name}");
                    TryMove();
                }
            }
        }
    }

    private void TryMove()
    {
        if (puzzleManager == null)
        {
            Debug.LogWarning("Puzzle manager not found!");
            return;
        }

        // Use row-major board indexing
        for (int y = 0; y < puzzleManager.gridSize.y; y++)
        for (int x = 0; x < puzzleManager.gridSize.x; x++)
        {
            if (puzzleManager.board[y, x] == this.gameObject && puzzleManager.IsAdjacent(new Vector2Int(x, y), puzzleManager.emptyPos))
            {
                Debug.Log($"Moving tile {gameObject.name} from ({x},{y}) to empty slot at {puzzleManager.emptyPos}");
                puzzleManager.MoveTile(new Vector2Int(x, y));
                return;
            }
        }
        Debug.Log($"Tile {gameObject.name} is not adjacent to empty slot and cannot move.");
    }
}
