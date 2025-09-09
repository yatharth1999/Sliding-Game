using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ResponsiveGrid : MonoBehaviour
{
    public int N = 3;                      // 3..6
    public float spacing = 0f;                 // spacing between tiles
    public Vector2 padding = new Vector2(0,0); // left/right, top/bottom
    public bool updateEveryFrame = true;       // for editor/resizing

    GridLayoutGroup grid;
    RectTransform rt;
    
    public static ResponsiveGrid inst;

    void Awake()
    {
        if (inst == null)
            inst = this;
        else
            Destroy(gameObject);
        grid = GetComponent<GridLayoutGroup>();
        rt   = GetComponent<RectTransform>();
        Apply();
    }

    void OnEnable()  { Apply(); }
    void Update()    { if (updateEveryFrame) Apply(); }

    public void SetN(int newN) { N = Mathf.Clamp(newN, 3, 6); Apply(); }

    void Apply()
    {
        if (!grid || !rt) return;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = N;
        grid.spacing = new Vector2(spacing, spacing);
        grid.padding = new RectOffset(
            Mathf.RoundToInt(padding.x),
            Mathf.RoundToInt(padding.x),
            Mathf.RoundToInt(padding.y),
            Mathf.RoundToInt(padding.y)
        );

        // Available square size
        var size = rt.rect.size; // PuzzleGrid is inside a 1:1 parent (PuzzleFrame)
        float totalSpacingX = spacing * (N - 1) + grid.padding.left + grid.padding.right;
        float totalSpacingY = spacing * (N - 1) + grid.padding.top + grid.padding.bottom;

        float cellW = (size.x - totalSpacingX) / N;
        float cellH = (size.y - totalSpacingY) / N;
        float cell  = Mathf.Floor(Mathf.Min(cellW, cellH)); // square tiles

        if (cell <= 0)
        {
            Debug.LogWarning("ResponsiveGrid: Calculated cell size <= 0, setting to 10.");
            cell = 10f;
        }

        grid.cellSize = new Vector2(cell, cell);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
    }
}
