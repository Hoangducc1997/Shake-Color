using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public int rows = 5, cols = 5;
    public GameObject cellPrefab;
    public GameObject[] blockPrefabs;
    public float spawnChance = 0.7f;

    private void Awake() => Instance = this;

    private void Start() => CreateBoard();

    void CreateBoard()
    {
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        if (!grid)
        {
            Debug.LogError("Add GridLayoutGroup");
            return;
        }

        for (int i = 0; i < rows * cols; i++)
        {
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.name = $"Cell_{i}";
            EnsureCorners(cell);

            // Spawn block ngẫu nhiên trên board (không kéo thả được)
            SpawnRandomBlocks(cell, false);
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

    // Sửa method SpawnRandomBlocks để public
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

                // Xóa component DraggableCell nếu có
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

    // Method mới để kiểm tra matches theo 4 hướng
    public void CheckMatchesInFourDirections(Cell centerCell, int colorID)
    {
        List<Cell> matchedCells = new List<Cell>();
        int index = centerCell.transform.GetSiblingIndex();
        int centerRow = index / cols;
        int centerCol = index % cols;

        // Kiểm tra 4 hướng
        CheckDirection(centerRow, centerCol, 0, -1, colorID, matchedCells); // Trái
        CheckDirection(centerRow, centerCol, 0, 1, colorID, matchedCells);  // Phải
        CheckDirection(centerRow, centerCol, -1, 0, colorID, matchedCells); // Trên
        CheckDirection(centerRow, centerCol, 1, 0, colorID, matchedCells);  // Dưới

        // Thêm center cell nếu có block cùng màu
        if (centerCell.HasBlockOfColor(colorID))
        {
            matchedCells.Add(centerCell);
        }

        // Xóa nếu có ít nhất 2 cell match
        if (matchedCells.Count >= 2)
        {
            foreach (Cell matchedCell in matchedCells)
            {
                matchedCell.RemoveBlocksOfColor(colorID);
            }
            Debug.Log($"Cleared {matchedCells.Count} cells of color {colorID}");
        }
    }

    private void CheckDirection(int startRow, int startCol, int rowDir, int colDir, int colorID, List<Cell> matchedCells)
    {
        int currentRow = startRow + rowDir;
        int currentCol = startCol + colDir;

        while (true)
        {
            Cell neighborCell = GetCellAt(currentRow, currentCol);

            if (neighborCell == null || !neighborCell.HasBlockOfColor(colorID))
            {
                break; // Dừng nếu hết cell hoặc không có block cùng màu
            }

            if (!matchedCells.Contains(neighborCell))
            {
                matchedCells.Add(neighborCell);
            }

            currentRow += rowDir;
            currentCol += colDir;
        }
    }
    public void RespawnBlocks(List<DraggableCell.BlockInfo> blocksToRespawn)
    {
        foreach (var blockInfo in blocksToRespawn)
        {
            if (blockInfo.cell != null && !string.IsNullOrEmpty(blockInfo.corner))
            {
                // Spawn block mới ngẫu nhiên
                SpawnNewBlockAtCorner(blockInfo.cell.gameObject, blockInfo.corner);
            }
        }

        // Kiểm tra thêm matches sau khi respawn (để xử lý chain reaction)
        CheckForChainReactions(blocksToRespawn);
    }

    // Thêm method để kiểm tra chain reaction
    private void CheckForChainReactions(List<DraggableCell.BlockInfo> respawnedBlocks)
    {
        // Có thể thêm logic chain reaction ở đây nếu muốn
        foreach (var blockInfo in respawnedBlocks)
        {
            if (blockInfo.cell != null)
            {
                // Kiểm tra xem block mới có tạo match không
                foreach (var block in blockInfo.cell.GetAllBlocks())
                {
                    BlockColor blockColor = block.GetComponent<BlockColor>();
                    if (blockColor != null)
                    {
                        CheckMatchesInFourDirections(blockInfo.cell, blockColor.colorID);
                    }
                }
            }
        }
    }
    private void SpawnNewBlockAtCorner(GameObject cell, string corner)
    {
        Transform point = cell.transform.Find(corner);
        if (point != null && blockPrefabs.Length > 0)
        {
            int randBlock = Random.Range(0, blockPrefabs.Length);
            GameObject newBlock = Instantiate(blockPrefabs[randBlock], point);
            newBlock.transform.localPosition = Vector3.zero;
            newBlock.transform.localScale = Vector3.one;

            // Thêm vào cell
            Cell cellComp = cell.GetComponent<Cell>();
            if (cellComp != null)
            {
                cellComp.AddBlock(newBlock, corner);
            }
        }
    }

    // Thêm method này vào BoardManager.cs
    public List<Cell> GetNeighborCells(Cell centerCell)
    {
        List<Cell> neighbors = new List<Cell>();

        int centerIndex = centerCell.transform.GetSiblingIndex();
        int centerRow = centerIndex / cols;
        int centerCol = centerIndex % cols;

        // Trái
        Cell leftCell = GetCellAt(centerRow, centerCol - 1);
        if (leftCell != null) neighbors.Add(leftCell);

        // Phải
        Cell rightCell = GetCellAt(centerRow, centerCol + 1);
        if (rightCell != null) neighbors.Add(rightCell);

        // Trên
        Cell topCell = GetCellAt(centerRow - 1, centerCol);
        if (topCell != null) neighbors.Add(topCell);

        // Dưới
        Cell bottomCell = GetCellAt(centerRow + 1, centerCol);
        if (bottomCell != null) neighbors.Add(bottomCell);

        return neighbors;
    }
    // Thêm method này vào BoardManager.cs
    // Thêm method này vào BoardManager.cs
    public List<Cell> GetAllCells()
    {
        List<Cell> allCells = new List<Cell>();
        foreach (Transform child in transform)
        {
            Cell cell = child.GetComponent<Cell>();
            if (cell != null)
            {
                allCells.Add(cell);
            }
        }
        return allCells;
    }

    public void ResetBoard()
    {
        // Xoá toàn bộ cell cũ
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Tạo lại board mới
        CreateBoard();
    }

}