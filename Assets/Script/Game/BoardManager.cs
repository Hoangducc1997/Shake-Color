using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int rows = 3, cols = 3;
    public GameObject cellPrefab;
    public GameObject[] blockPrefabs;
    public float spawnChance = 0.7f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CreateBoard();
    }

    // METHOD ĐỂ CẤU HÌNH LẠI BOARD
    public void ConfigureBoard(int newRows, int newCols)
    {
        rows = newRows;
        cols = newCols;
        ResetBoard();
    }

    public void ResetBoard()
    {
        // XÓA TOÀN BỘ CELL CŨ
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // TẠO BOARD MỚI
        CreateBoard();
    }

    void CreateBoard()
    {
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        if (!grid)
        {
            Debug.LogError("Add GridLayoutGroup");
            return;
        }

        // ĐIỀU CHỈNH GRID LAYOUT THEO SỐ LƯỢNG ROWS/COLS
        AdjustGridLayout();

        for (int i = 0; i < rows * cols; i++)
        {
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.name = $"Cell_{i}";
            EnsureCorners(cell);
            SpawnRandomBlocks(cell, false);
        }

        Debug.Log($"Created board: {rows}x{cols} with {rows * cols} cells");
    }

    private void AdjustGridLayout()
    {
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            // TỰ ĐỘNG TÍNH TOÁN KÍCH THƯỚC CELL DỰA TRÊN SỐ LƯỢNG ROWS/COLS
            RectTransform rectTransform = GetComponent<RectTransform>();
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            grid.cellSize = new Vector2(width / cols, height / rows);
            grid.spacing = new Vector2(10, 10); // Có thể điều chỉnh
        }
    }

    void EnsureCorners(GameObject cell)
    {
        string[] corners = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
        foreach (string name in corners)
        {
            Transform t = cell.transform.Find(name);
            if (!t)
            {
                GameObject point = new GameObject(name, typeof(RectTransform));
                point.transform.SetParent(cell.transform);
                point.transform.localScale = Vector3.one;
                point.transform.localPosition = Vector3.zero;

                RectTransform rt = point.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(20, 20);

                switch (name)
                {
                    case "TopLeft": rt.anchoredPosition = new Vector2(-40, 40); break;
                    case "TopRight": rt.anchoredPosition = new Vector2(40, 40); break;
                    case "BottomLeft": rt.anchoredPosition = new Vector2(-40, -40); break;
                    case "BottomRight": rt.anchoredPosition = new Vector2(40, -40); break;
                }
            }
        }
    }

    public void SpawnRandomBlocks(GameObject cell, bool isDraggable)
    {
        string[] corners = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
        List<string> available = new List<string>(corners);
        Cell cellComp = cell.GetComponent<Cell>();

        int blockCount = Random.Range(1, 5);

        for (int i = 0; i < blockCount; i++)
        {
            if (available.Count == 0) break;

            int randIndex = Random.Range(0, available.Count);
            string corner = available[randIndex];
            available.RemoveAt(randIndex);

            Transform point = cell.transform.Find(corner);
            if (point != null && blockPrefabs.Length > 0 && Random.value <= spawnChance)
            {
                int randBlock = Random.Range(0, blockPrefabs.Length);
                GameObject block = Instantiate(blockPrefabs[randBlock], point);
                block.transform.localPosition = Vector3.zero;
                block.transform.localScale = Vector3.one;

                if (!isDraggable && block.GetComponent<DraggableCell>() != null)
                {
                    Destroy(block.GetComponent<DraggableCell>());
                }

                cellComp.AddBlock(block, corner);
            }
        }
    }

    public Cell GetNearestCell(Vector3 pos, float maxDist = 100f)
    {
        Cell nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform t in transform)
        {
            float dist = Vector3.Distance(pos, t.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t.GetComponent<Cell>();
            }
        }

        return minDist <= maxDist ? nearest : null;
    }

    public Cell GetCellAt(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols) return null;
        int index = row * cols + col;
        return index < transform.childCount ? transform.GetChild(index).GetComponent<Cell>() : null;
    }

    public List<Cell> GetAllCells()
    {
        List<Cell> allCells = new List<Cell>();
        foreach (Transform child in transform)
        {
            Cell cell = child.GetComponent<Cell>();
            if (cell != null) allCells.Add(cell);
        }
        return allCells;
    }
}