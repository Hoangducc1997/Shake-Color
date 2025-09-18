using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DraggableCell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector3 startPos;
    private Transform startParent;
    private Cell sourceCell;
    private BoardManager boardManager; // Thêm biến boardManager

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        sourceCell = GetComponent<Cell>();
        boardManager = BoardManager.Instance; // Khởi tạo boardManager
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rectTransform.position;
        startParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        rectTransform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (boardManager == null)
        {
            boardManager = BoardManager.Instance;
            if (boardManager == null)
            {
                ReturnToOriginalPosition();
                return;
            }
        }

        Cell targetCell = boardManager.GetNearestCell(rectTransform.position);

        if (targetCell != null && !targetCell.HasBlocks())
        {
            // Di chuyển toàn bộ block sang cell đích (trên board)
            MoveBlocksToTargetCell(targetCell);

            // Kiểm tra matches ở các block kế cận
            CheckSurroundingMatches(targetCell);

            // Spawn cell mới trong spawner
            SpawnerManager spawnerManager = FindAnyObjectByType<SpawnerManager>();
            if (spawnerManager != null)
            {
                spawnerManager.SpawnCell();
            }

            // Hủy cell cũ trong spawner
            Destroy(gameObject);
        }
        else
        {
            // Trả về vị trí ban đầu nếu không thể thả
            ReturnToOriginalPosition();
        }
    }

    private void MoveBlocksToTargetCell(Cell targetCell)
    {
        if (sourceCell == null || targetCell == null) return;

        foreach (var pair in sourceCell.blocks)
        {
            string corner = pair.Key;
            GameObject block = pair.Value;

            // Di chuyển block sang cell đích
            Transform cornerTransform = targetCell.transform.Find(corner);
            if (cornerTransform != null)
            {
                block.transform.SetParent(cornerTransform);
                block.transform.localPosition = Vector3.zero;
                block.transform.localScale = Vector3.one;

                // Thêm block vào cell đích
                targetCell.AddBlock(block, corner);
            }
        }

        // Xóa tất cả block khỏi cell nguồn
        sourceCell.blocks.Clear();
    }

    private void CheckSurroundingMatches(Cell centerCell)
    {
        if (centerCell == null || boardManager == null) return;

        // Danh sách các block cần xóa
        List<BlockInfo> blocksToRemove = new List<BlockInfo>();

        // Kiểm tra từng block trong cell mới
        foreach (var pair in centerCell.blocks)
        {
            GameObject block = pair.Value;
            BlockColor blockColor = block.GetComponent<BlockColor>();
            if (blockColor != null)
            {
                // Tìm các block kế cận cùng màu
                FindAdjacentBlocksOfSameColor(centerCell, pair.Key, blockColor.colorID, blocksToRemove);
            }
        }

        // Chỉ xóa nếu có ít nhất 2 block liền kề cùng màu
        if (blocksToRemove.Count >= 2)
        {
            foreach (BlockInfo blockInfo in blocksToRemove)
            {
                // Xóa block cũ
                if (blockInfo.cell != null && blockInfo.block != null)
                {
                    blockInfo.cell.RemoveBlock(blockInfo.corner);
                    Destroy(blockInfo.block);
                }
            }

            Debug.Log($"Removed {blocksToRemove.Count} adjacent blocks of same color");
        }
        else
        {
            Debug.Log("Not enough adjacent blocks found");
        }
    }

    private void FindAdjacentBlocksOfSameColor(Cell centerCell, string centerCorner, int targetColorID, List<BlockInfo> blocksToRemove)
    {
        // Thêm block trung tâm vào danh sách
        AddBlockToRemoveList(centerCell, centerCorner, targetColorID, blocksToRemove);

        // Lấy vị trí cell trong board
        int cellIndex = centerCell.transform.GetSiblingIndex();
        int cellRow = cellIndex / boardManager.cols;
        int cellCol = cellIndex % boardManager.cols;

        // Kiểm tra các block kế cận theo 4 hướng
        CheckAdjacentInSameCell(centerCell, centerCorner, targetColorID, blocksToRemove);
        CheckAdjacentInNeighborCells(cellRow, cellCol, centerCorner, targetColorID, blocksToRemove);
    }

    private void CheckAdjacentInSameCell(Cell cell, string centerCorner, int targetColorID, List<BlockInfo> blocksToRemove)
    {
        // Kiểm tra các corner kế cận trong cùng cell
        switch (centerCorner)
        {
            case "TopLeft":
                CheckCorner(cell, "TopRight", targetColorID, blocksToRemove);
                CheckCorner(cell, "BottomLeft", targetColorID, blocksToRemove);
                break;
            case "TopRight":
                CheckCorner(cell, "TopLeft", targetColorID, blocksToRemove);
                CheckCorner(cell, "BottomRight", targetColorID, blocksToRemove);
                break;
            case "BottomLeft":
                CheckCorner(cell, "TopLeft", targetColorID, blocksToRemove);
                CheckCorner(cell, "BottomRight", targetColorID, blocksToRemove);
                break;
            case "BottomRight":
                CheckCorner(cell, "TopRight", targetColorID, blocksToRemove);
                CheckCorner(cell, "BottomLeft", targetColorID, blocksToRemove);
                break;
        }
    }

    private void CheckAdjacentInNeighborCells(int cellRow, int cellCol, string centerCorner, int targetColorID, List<BlockInfo> blocksToRemove)
    {
        // Kiểm tra các block kế cận trong cell liền kề
        switch (centerCorner)
        {
            case "TopLeft":
                CheckNeighborCellCorner(cellRow, cellCol - 1, "TopRight", targetColorID, blocksToRemove); // Trái
                CheckNeighborCellCorner(cellRow - 1, cellCol, "BottomLeft", targetColorID, blocksToRemove); // Trên
                break;
            case "TopRight":
                CheckNeighborCellCorner(cellRow, cellCol + 1, "TopLeft", targetColorID, blocksToRemove); // Phải
                CheckNeighborCellCorner(cellRow - 1, cellCol, "BottomRight", targetColorID, blocksToRemove); // Trên
                break;
            case "BottomLeft":
                CheckNeighborCellCorner(cellRow, cellCol - 1, "BottomRight", targetColorID, blocksToRemove); // Trái
                CheckNeighborCellCorner(cellRow + 1, cellCol, "TopLeft", targetColorID, blocksToRemove); // Dưới
                break;
            case "BottomRight":
                CheckNeighborCellCorner(cellRow, cellCol + 1, "BottomLeft", targetColorID, blocksToRemove); // Phải
                CheckNeighborCellCorner(cellRow + 1, cellCol, "TopRight", targetColorID, blocksToRemove); // Dưới
                break;
        }
    }

    private void CheckNeighborCellCorner(int row, int col, string corner, int targetColorID, List<BlockInfo> blocksToRemove)
    {
        Cell neighborCell = boardManager.GetCellAt(row, col);
        if (neighborCell != null)
        {
            CheckCorner(neighborCell, corner, targetColorID, blocksToRemove);
        }
    }

    private void CheckCorner(Cell cell, string corner, int targetColorID, List<BlockInfo> blocksToRemove)
    {
        if (cell.HasBlockOfColorAtCorner(corner, targetColorID))
        {
            AddBlockToRemoveList(cell, corner, targetColorID, blocksToRemove);
        }
    }

    private void AddBlockToRemoveList(Cell cell, string corner, int targetColorID, List<BlockInfo> blocksToRemove)
    {
        GameObject block = cell.GetBlockAtCorner(corner);
        if (block != null)
        {
            // Kiểm tra nếu chưa có trong danh sách
            bool alreadyExists = false;
            foreach (var existing in blocksToRemove)
            {
                if (existing.cell == cell && existing.corner == corner)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                BlockColor blockColor = block.GetComponent<BlockColor>();
                if (blockColor != null && blockColor.colorID == targetColorID)
                {
                    blocksToRemove.Add(new BlockInfo
                    {
                        cell = cell,
                        block = block,
                        corner = corner,
                        colorID = targetColorID
                    });
                }
            }
        }
    }

    private void ReturnToOriginalPosition()
    {
        rectTransform.SetParent(startParent);
        rectTransform.position = startPos;
    }

    // Struct để lưu thông tin block
    public struct BlockInfo
    {
        public Cell cell;
        public GameObject block;
        public string corner;
        public int colorID;
    }
}