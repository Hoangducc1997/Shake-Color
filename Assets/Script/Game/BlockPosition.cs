using UnityEngine;

public class BlockPosition : MonoBehaviour
{
    public Vector2Int gridPosition; // Vị trí trong grid của board
    public string cornerName;       // Tên corner (TopLeft, TopRight, etc.)
    public Cell parentCell;         // Cell chứa block này

    private void Start()
    {
        parentCell = GetComponentInParent<Cell>();
        CalculateGridPosition();
    }

    public void CalculateGridPosition()
    {
        if (parentCell == null) return;

        int cellIndex = parentCell.transform.GetSiblingIndex();
        int rows = BoardManager.Instance.rows;
        int cols = BoardManager.Instance.cols;

        int cellRow = cellIndex / cols;
        int cellCol = cellIndex % cols;

        // Tính toán vị trí grid dựa trên corner
        switch (cornerName)
        {
            case "TopLeft":
                gridPosition = new Vector2Int(cellCol * 2, cellRow * 2 + 1);
                break;
            case "TopRight":
                gridPosition = new Vector2Int(cellCol * 2 + 1, cellRow * 2 + 1);
                break;
            case "BottomLeft":
                gridPosition = new Vector2Int(cellCol * 2, cellRow * 2);
                break;
            case "BottomRight":
                gridPosition = new Vector2Int(cellCol * 2 + 1, cellRow * 2);
                break;
        }
    }

    public bool IsAdjacent(BlockPosition other)
    {
        // Kiểm tra kế cận theo 4 hướng
        int dx = Mathf.Abs(gridPosition.x - other.gridPosition.x);
        int dy = Mathf.Abs(gridPosition.y - other.gridPosition.y);

        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }
}