using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector3 startPos;
    private Transform startParent;
    private Cell sourceCell;
    private BoardManager boardManager;
    private Coroutine jellyCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        sourceCell = GetComponent<Cell>();
        // Tìm boardManager khi khởi tạo
        FindActiveBoard();
    }
    private void FindActiveBoard()
    {
        // Ưu tiên lấy BoardManager đang active
        boardManager = BoardManager.ActiveBoard;
        // Nếu không có, tìm boardManager trong các instance đang active
        if (boardManager == null)
        {
            foreach (BoardManager board in BoardManager.Instances)
            {
                if (board.gameObject.activeInHierarchy)
                {
                    boardManager = board;
                    break;
                }
            }
        }
        if (boardManager == null && BoardManager.Instances.Count > 0)
        {
            boardManager = BoardManager.Instances[0];
        }
        if (boardManager != null)
        {
            Debug.Log($"DraggableCell found board: {boardManager.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("DraggableCell: No board found!");
        }
    }

    private void OnEnable()
    {
        // Khi được kích hoạt, tìm boardManager
        FindActiveBoard();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rectTransform.position;
        startParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        rectTransform.SetParent(canvas.transform);

        // Hiệu ứng jelly khi bắt đầu kéo
        foreach (var pair in sourceCell.blocks)
        {
            pair.Value.GetComponent<JellyEffect>()?.PlayJelly();
        }
        AudioManager.Instance.PlayVFX("Pop");
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // Hiệu ứng jelly trong khi kéo
        if (jellyCoroutine == null)
        {
            jellyCoroutine = StartCoroutine(PlayJellyCoroutine());
            AudioManager.Instance.PlayVFX("Pop");
        }

    }

    private IEnumerator PlayJellyCoroutine()
    {
        foreach (var pair in sourceCell.blocks)
        {
            var jelly = pair.Value.GetComponent<JellyEffect>();
            if (jelly != null)
            {
                jelly.PlayJelly();
            }
        }
        yield return new WaitForSeconds(0.68f);
        jellyCoroutine = null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        AudioManager.Instance.PlayVFX("Pop");
        // Hiệu ứng jelly khi kết thúc kéo
        foreach (var pair in sourceCell.blocks)
        {
            pair.Value.GetComponent<JellyEffect>()?.PlayJelly();
        }
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        if (boardManager == null)
        {
            boardManager = BoardManager.ActiveBoard;
            if (boardManager == null && BoardManager.Instances.Count > 0)
            {
                boardManager = BoardManager.Instances[0];
            }
            if (boardManager == null)
            {
                ReturnToOriginalPosition();
                return;
            }
        }
        Cell targetCell = boardManager.GetNearestCell(rectTransform.position);
        if (targetCell != null && !targetCell.HasBlocks())
        {
            MoveBlocksToTargetCell(targetCell);
            List<BlockInfo> blocksToRemove = CheckForMatchesWithExistingBlocks(targetCell);

            if (blocksToRemove.Count >= 2)
            {
                PlayExplosionEffects(blocksToRemove);
                RemoveMatchedBlocks(blocksToRemove);
                ResetAllSpawners();
                Destroy(gameObject);
            }
            else
            {
                PlayDropEffect(targetCell);
                ResetAllSpawners();
                Destroy(gameObject);
            }
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }
    public void UpdateBoardReference(BoardManager newBoard)
    {
        boardManager = newBoard;
        Debug.Log($"DraggableCell updated board reference to: {newBoard.gameObject.name}");
    }

    // Hiệu ứng nổ khi có match
    private void PlayExplosionEffects(List<BlockInfo> blocksToRemove)
    {
        foreach (BlockInfo blockInfo in blocksToRemove)
        {
            if (blockInfo.block != null)
            {
                ExplosionEffect explosion = blockInfo.block.GetComponent<ExplosionEffect>();
                if (explosion != null)
                {
                    explosion.PlayExplosion();
                }
                else
                {
                    // Tự động thêm component ExplosionEffect nếu chưa có
                    ExplosionEffect newExplosion = blockInfo.block.AddComponent<ExplosionEffect>();
                    newExplosion.PlayExplosion();
                }
            }
        }
    }


    private void PlayDropEffect(Cell targetCell)
    {
        foreach (var block in targetCell.GetAllBlocks())
        {
            StartCoroutine(PlayBounceEffect(block));
        }
    }

    private IEnumerator PlayBounceEffect(GameObject block)
    {
        Vector3 originalScale = block.transform.localScale;
        float duration = 0.2f;
        // Scale to lên
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            block.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, t / duration);
            yield return null;
        }

        // Scale về
        t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            block.transform.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, t / duration);
            yield return null;
        }
        block.transform.localScale = originalScale;
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
        foreach (Cell boardCell in boardManager.GetAllCells())
        {
            if (boardCell == newCell) continue;

            foreach (var pair in boardCell.blocks)
            {
                GameObject block = pair.Value;
                BlockColor blockColor = block.GetComponent<BlockColor>();
                if (blockColor != null)
                {
                    CheckMatchesWithNewCell(boardCell, pair.Key, blockColor.colorID, newCell, blocksToRemove);
                }
            }
        }
        return blocksToRemove;
    }

    private void CheckMatchesWithNewCell(Cell boardCell, string boardCorner, int boardColorID, Cell newCell, List<BlockInfo> blocksToRemove)
    {
        foreach (var newPair in newCell.blocks)
        {
            GameObject newBlock = newPair.Value;
            BlockColor newBlockColor = newBlock.GetComponent<BlockColor>();

            if (newBlockColor != null && newBlockColor.colorID == boardColorID)
            {
                if (AreBlocksAdjacent(boardCell, boardCorner, newCell, newPair.Key))
                {
                    AddBlockToRemoveList(boardCell, boardCorner, boardColorID, blocksToRemove);
                    AddBlockToRemoveList(newCell, newPair.Key, boardColorID, blocksToRemove);
                }
            }
        }
    }
    private bool AreBlocksAdjacent(Cell cell1, string corner1, Cell cell2, string corner2)
    {
        int index1 = cell1.transform.GetSiblingIndex();
        int row1 = index1 / boardManager.cols;
        int col1 = index1 % boardManager.cols;
        int index2 = cell2.transform.GetSiblingIndex();
        int row2 = index2 / boardManager.cols;
        int col2 = index2 % boardManager.cols;
        bool cellsAdjacent = (Mathf.Abs(row1 - row2) == 1 && col1 == col2) ||
                            (Mathf.Abs(col1 - col2) == 1 && row1 == row2);
        if (!cellsAdjacent) return false;
        return AreCornersAdjacent(corner1, corner2, row1, col1, row2, col2);
    }

    private bool AreCornersAdjacent(string corner1, string corner2, int row1, int col1, int row2, int col2)
    {
        if (row1 == row2)
        {
            if (col1 == col2 - 1)
            {
                return (corner1 == "TopRight" && corner2 == "TopLeft") ||
                       (corner1 == "BottomRight" && corner2 == "BottomLeft");
            }
            else if (col1 == col2 + 1)
            {
                return (corner1 == "TopLeft" && corner2 == "TopRight") ||
                       (corner1 == "BottomLeft" && corner2 == "BottomRight");
            }
        }
        else if (col1 == col2)
        {
            if (row1 == row2 - 1)
            {
                return (corner1 == "BottomLeft" && corner2 == "TopLeft") ||
                       (corner1 == "BottomRight" && corner2 == "TopRight");
            }
            else if (row1 == row2 + 1)
            {
                return (corner1 == "TopLeft" && corner2 == "BottomLeft") ||
                       (corner1 == "TopRight" && corner2 == "BottomRight");
            }
        }
        return false;
    }
    private void RemoveMatchedBlocks(List<BlockInfo> blocksToRemove)
    {
        Dictionary<int, int> colorCount = new Dictionary<int, int>();

        foreach (BlockInfo blockInfo in blocksToRemove)
        {
            if (blockInfo.cell != null && blockInfo.block != null)
            {
                if (!colorCount.ContainsKey(blockInfo.colorID))
                    colorCount[blockInfo.colorID] = 0;
                colorCount[blockInfo.colorID]++;
                blockInfo.cell.RemoveBlock(blockInfo.corner);
                Destroy(blockInfo.block);
            }
        }
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

    private void ResetAllSpawners()
    {
        foreach (var spawner in SpawnerManager.Instances)
        {
            spawner.RestartSpawner();
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