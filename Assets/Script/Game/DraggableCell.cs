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
    private BoardManager boardManager;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        sourceCell = GetComponent<Cell>();
        boardManager = BoardManager.Instance;
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
            // DI CHUYỂN BLOCKS SANG BOARD
            MoveBlocksToTargetCell(targetCell);

            // KIỂM TRA MATCH CHỈ VỚI CÁC BLOCK TRÊN BOARD (KHÔNG BAO GỒM BLOCK MỚI)
            List<BlockInfo> blocksToRemove = CheckForMatchesWithExistingBlocks(targetCell);

            if (blocksToRemove.Count >= 2)
            {
                // CÓ MATCH: Xóa blocks và spawn cell mới
                RemoveMatchedBlocks(blocksToRemove);
                foreach (var spawner in SpawnerManager.Instances)
                {
                    spawner.RestartSpawner();
                }

                Destroy(gameObject);
            }
            else
            {
                // KHÔNG CÓ MATCH: Giữ nguyên blocks trên board
                foreach (var spawner in SpawnerManager.Instances)
                {
                    spawner.RestartSpawner();
                }

                Destroy(gameObject);
            }
        }
        else
        {
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

            Transform cornerTransform = targetCell.transform.Find(corner);
            if (cornerTransform != null)
            {
                block.transform.SetParent(cornerTransform);
                block.transform.localPosition = Vector3.zero;
                block.transform.localScale = Vector3.one;

                targetCell.AddBlock(block, corner);
            }
        }

        sourceCell.blocks.Clear();
    }

    private List<BlockInfo> CheckForMatchesWithExistingBlocks(Cell newCell)
    {
        List<BlockInfo> blocksToRemove = new List<BlockInfo>();

        // CHỈ KIỂM TRA VỚI CÁC BLOCK ĐÃ CÓ TRÊN BOARD (KHÔNG BAO GỒM BLOCK MỚI)
        foreach (Cell boardCell in boardManager.GetAllCells())
        {
            // BỎ QUA CELL MỚI (vừa được thả)
            if (boardCell == newCell) continue;

            foreach (var pair in boardCell.blocks)
            {
                GameObject block = pair.Value;
                BlockColor blockColor = block.GetComponent<BlockColor>();
                if (blockColor != null)
                {
                    // Kiểm tra xem block trên board có match với block mới không
                    CheckMatchesWithNewCell(boardCell, pair.Key, blockColor.colorID, newCell, blocksToRemove);
                }
            }
        }

        return blocksToRemove;
    }

    private void CheckMatchesWithNewCell(Cell boardCell, string boardCorner, int boardColorID, Cell newCell, List<BlockInfo> blocksToRemove)
    {
        // Kiểm tra từng block trong cell mới
        foreach (var newPair in newCell.blocks)
        {
            GameObject newBlock = newPair.Value;
            BlockColor newBlockColor = newBlock.GetComponent<BlockColor>();

            if (newBlockColor != null && newBlockColor.colorID == boardColorID)
            {
                // Kiểm tra xem 2 block có kế cận không
                if (AreBlocksAdjacent(boardCell, boardCorner, newCell, newPair.Key))
                {
                    // Thêm cả 2 block vào danh sách xóa
                    AddBlockToRemoveList(boardCell, boardCorner, boardColorID, blocksToRemove);
                    AddBlockToRemoveList(newCell, newPair.Key, boardColorID, blocksToRemove);
                }
            }
        }
    }

    private bool AreBlocksAdjacent(Cell cell1, string corner1, Cell cell2, string corner2)
    {
        // Lấy vị trí cell
        int index1 = cell1.transform.GetSiblingIndex();
        int row1 = index1 / boardManager.cols;
        int col1 = index1 % boardManager.cols;

        int index2 = cell2.transform.GetSiblingIndex();
        int row2 = index2 / boardManager.cols;
        int col2 = index2 % boardManager.cols;

        // Kiểm tra cell có kế cận không
        bool cellsAdjacent = (Mathf.Abs(row1 - row2) == 1 && col1 == col2) ||
                            (Mathf.Abs(col1 - col2) == 1 && row1 == row2);

        if (!cellsAdjacent) return false;

        // Kiểm tra corner có kế cận không
        return AreCornersAdjacent(corner1, corner2, row1, col1, row2, col2);
    }

    private bool AreCornersAdjacent(string corner1, string corner2, int row1, int col1, int row2, int col2)
    {
        // TopLeft của cell (0,0) kế cận với TopRight của cell (0,-1)
        // BottomRight của cell (0,0) kế cận với BottomLeft của cell (1,0)
        // v.v.

        if (row1 == row2) // Cùng hàng
        {
            if (col1 == col2 - 1) // Cell1 bên trái Cell2
            {
                return (corner1 == "TopRight" && corner2 == "TopLeft") ||
                       (corner1 == "BottomRight" && corner2 == "BottomLeft");
            }
            else if (col1 == col2 + 1) // Cell1 bên phải Cell2
            {
                return (corner1 == "TopLeft" && corner2 == "TopRight") ||
                       (corner1 == "BottomLeft" && corner2 == "BottomRight");
            }
        }
        else if (col1 == col2) // Cùng cột
        {
            if (row1 == row2 - 1) // Cell1 bên trên Cell2
            {
                return (corner1 == "BottomLeft" && corner2 == "TopLeft") ||
                       (corner1 == "BottomRight" && corner2 == "TopRight");
            }
            else if (row1 == row2 + 1) // Cell1 bên dưới Cell2
            {
                return (corner1 == "TopLeft" && corner2 == "BottomLeft") ||
                       (corner1 == "TopRight" && corner2 == "BottomRight");
            }
        }

        return false;
    }

    private void RemoveMatchedBlocks(List<BlockInfo> blocksToRemove)
    {
        // Đếm số lượng block theo màu
        Dictionary<int, int> colorCount = new Dictionary<int, int>();

        foreach (BlockInfo blockInfo in blocksToRemove)
        {
            if (blockInfo.cell != null && blockInfo.block != null)
            {
                // Đếm số lượng block theo màu
                if (!colorCount.ContainsKey(blockInfo.colorID))
                    colorCount[blockInfo.colorID] = 0;
                colorCount[blockInfo.colorID]++;

                // Xóa block
                blockInfo.cell.RemoveBlock(blockInfo.corner);
                Destroy(blockInfo.block);
            }
        }

        // TRỪ điểm mục tiêu
        foreach (var pair in colorCount)
        {
            GoalManager.Instance.SubtractTargetScore(pair.Key, pair.Value);
        }
    }

    private void AddBlockToRemoveList(Cell cell, string corner, int targetColorID, List<BlockInfo> blocksToRemove)
    {
        GameObject block = cell.GetBlockAtCorner(corner);
        if (block != null)
        {
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

    private void ReturnToOriginalPosition()
    {
        rectTransform.SetParent(startParent);
        rectTransform.position = startPos;
    }

    public struct BlockInfo
    {
        public Cell cell;
        public GameObject block;
        public string corner;
        public int colorID;
    }
}