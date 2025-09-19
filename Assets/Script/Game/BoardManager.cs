using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public static List<BoardManager> Instances = new List<BoardManager>();
    public static BoardManager ActiveBoard { get; private set; }

    public int rows = 5, cols = 5;
    public GameObject cellPrefab;
    public GameObject[] blockPrefabs;
    public float spawnChance = 0.7f;

    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    private bool isResetting = false;
    private bool hasInitialized = false; // Flag check
    private bool isFirstCreation = true;
    public GameObject gameOverPanel;

    private void Awake()
    {
        Instances.Add(this);

        if (ActiveBoard == null && gameObject.activeInHierarchy)
        {
            SetAsActiveBoard();
        }
    }

    private void OnEnable()
    {
        SetAsActiveBoard();

        //  Nếu chưa có board, tạo mới
        if (!hasInitialized)
        {
            CreateBoard();
            hasInitialized = true;
        }
    }

    private void Start()
    {
        if (!hasInitialized)
        {
            CreateBoard();
            hasInitialized = true;
        }
        gameOverPanel.SetActive(false);
    }

    private void OnDisable()
    {
        if (ActiveBoard == this)
        {
            ActiveBoard = null;
        }
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
        StopAllBoardCoroutines();

        if (ActiveBoard == this)
        {
            ActiveBoard = null;
        }
    }

    public void SetAsActiveBoard()
    {
        if (!gameObject.activeInHierarchy) return;

        ActiveBoard = this;
        Debug.Log($"Active board set to: {gameObject.name}");
        NotifyAllDraggableCells();
    }
    private void NotifyAllDraggableCells()
    {
        DraggableCell[] allDraggableCells = FindObjectsOfType<DraggableCell>();
        foreach (DraggableCell draggableCell in allDraggableCells)
        {
            draggableCell.UpdateBoardReference(this);
        }
    }

    void CreateBoard()
    {
        if (isResetting) return;

        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        if (!grid)
        {
            Debug.LogError("Add GridLayoutGroup");
            return;
        }
        ClearAllCells();
        for (int i = 0; i < rows * cols; i++)
        {
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.name = $"Cell_{i}";
            EnsureCorners(cell);
            SpawnRandomBlocks(cell, false);
        }
        isFirstCreation = false;
        Debug.Log($"Board created: {gameObject.name}, FirstCreation: {isFirstCreation}");
    }

    private void ClearAllCells()
    {
        StopAllBoardCoroutines();

        foreach (Transform child in transform)
        {
            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private void StopAllBoardCoroutines()
    {
        foreach (Coroutine coroutine in activeCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();
        StopAllCoroutines();
    }

    private new Coroutine StartCoroutine(IEnumerator routine)
    {
        Coroutine coroutine = base.StartCoroutine(routine);
        activeCoroutines.Add(coroutine);
        return coroutine;
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
                // áp dụng hiệu ứng spawn nếu không phải lần đầu tạo board
                if (!isFirstCreation)
                {
                    StartCoroutine(PlaySpawnEffect(block));
                }
                if (!isDraggable && block.GetComponent<DraggableCell>() != null)
                {
                    Destroy(block.GetComponent<DraggableCell>());
                }
                cellComp.AddBlock(block, corner);
            }
        }
    }

    // Hiệu ứng khi spawn block mới
    private IEnumerator PlaySpawnEffect(GameObject block)
    {
        if (block == null) yield break;
        Vector3 originalScale = Vector3.one;
        block.transform.localScale = Vector3.zero;
        float duration = 0.3f;
        float t = 0;
        while (t < duration && block != null)
        {
            t += Time.deltaTime;
            block.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t / duration);
            yield return null;
        }
        if (block != null)
        {
            block.transform.localScale = originalScale;
        }
    }
    public void CheckMatchesInFourDirections(Cell centerCell, int colorID)
    {
        List<Cell> matchedCells = new List<Cell>();
        int index = centerCell.transform.GetSiblingIndex();
        int centerRow = index / cols;
        int centerCol = index % cols;
        CheckDirection(centerRow, centerCol, 0, -1, colorID, matchedCells);
        CheckDirection(centerRow, centerCol, 0, 1, colorID, matchedCells);
        CheckDirection(centerRow, centerCol, -1, 0, colorID, matchedCells);
        CheckDirection(centerRow, centerCol, 1, 0, colorID, matchedCells);
        if (centerCell.HasBlockOfColor(colorID))
        {
            matchedCells.Add(centerCell);
        }
        if (matchedCells.Count >= 2)
        {
            PlayExplosionEffects(matchedCells, colorID);
            foreach (Cell matchedCell in matchedCells)
            {
                matchedCell.RemoveBlocksOfColor(colorID);
            }
            Debug.Log($"Cleared {matchedCells.Count} cells of color {colorID}");
        }
    }

    // Hiệu ứng nổ khi match
    private void PlayExplosionEffects(List<Cell> matchedCells, int colorID)
    {
        foreach (Cell cell in matchedCells)
        {
            foreach (var block in cell.GetAllBlocks())
            {
                BlockColor blockColor = block.GetComponent<BlockColor>();
                if (blockColor != null && blockColor.colorID == colorID)
                {
                    ExplosionEffect explosion = block.GetComponent<ExplosionEffect>();
                    if (explosion != null)
                    {
                        explosion.PlayExplosion();
                    }
                    else
                    {
                        // Tạo hiệu ứng nổ nếu chưa có component
                        ExplosionEffect newExplosion = block.AddComponent<ExplosionEffect>();
                        newExplosion.PlayExplosion();
                    }
                }
            }
        }
    }

    public Cell GetNearestCell(Vector3 pos, float maxDist = 100f)
    {
        if (ActiveBoard != this) return null;
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
        if (ActiveBoard != this) return null;
        if (row < 0 || row >= rows || col < 0 || col >= cols) return null;
        int index = row * cols + col;
        return index < transform.childCount ? transform.GetChild(index).GetComponent<Cell>() : null;
    }

    // Kiểm tra matches theo 4 hướng
    private void CheckDirection(int startRow, int startCol, int rowDir, int colDir, int colorID, List<Cell> matchedCells)
    {
        int currentRow = startRow + rowDir;
        int currentCol = startCol + colDir;
        while (true)
        {
            Cell neighborCell = GetCellAt(currentRow, currentCol);
            if (neighborCell == null || !neighborCell.HasBlockOfColor(colorID))
            {
                break;
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
        StartCoroutine(RespawnBlocksWithEffect(blocksToRespawn));
    }

    // Hiệu ứng khi respawn block
    private IEnumerator RespawnBlocksWithEffect(List<DraggableCell.BlockInfo> blocksToRespawn)
    {
        yield return new WaitForSeconds(1);
        foreach (var blockInfo in blocksToRespawn)
        {
            if (blockInfo.cell != null && !string.IsNullOrEmpty(blockInfo.corner))
            {
                SpawnNewBlockAtCorner(blockInfo.cell.gameObject, blockInfo.corner);
            }
        }
        CheckForChainReactions(blocksToRespawn);
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
            StartCoroutine(PlaySpawnEffect(newBlock));
            Cell cellComp = cell.GetComponent<Cell>();
            if (cellComp != null)
            {
                cellComp.AddBlock(newBlock, corner);
            }
        }
    }

    public void ResetBoard()
    {
        AudioManager.Instance.PlayVFX("Match");
        StopAllCoroutines();
        StartCoroutine(ResetBoardWithEffects());
    }

    private IEnumerator ResetBoardWithEffects()
    {
        // PHÁT HIỆU ỨNG NỔ CHO TẤT CẢ BLOCK HIỆN TẠI
        foreach (Transform cellTransform in transform)
        {
            Cell cell = cellTransform.GetComponent<Cell>();
            if (cell != null)
            {
                foreach (var block in cell.GetAllBlocks())
                {
                    ExplosionEffect explosion = block.GetComponent<ExplosionEffect>();
                    if (explosion != null)
                    {
                        explosion.PlayExplosion();
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        // Xóa tất cả cell và tạo lại board
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        yield return null;
        CreateBoard();
    }

    private void CheckForChainReactions(List<DraggableCell.BlockInfo> respawnedBlocks)
    {
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
    public bool IsBoardFull()
    {
        Cell[] allCells = FindObjectsOfType<Cell>();
        int totalCells = allCells.Length;
        int filledCells = 0;

        foreach (Cell cell in allCells)
        {
            if (cell.blocks.Count > 0)
            {
                filledCells++;
            }
            else
            {
                Debug.Log($"Empty cell found: {cell.gameObject.name}");
            }
        }

        Debug.Log($"Global check: {filledCells}/{totalCells} cells filled");
        return filledCells == totalCells;
    }

    public void CheckIfBoardIsFull()
    {
        int expectedCells = rows * cols;
        int cellsWithBlocks = 0;
        for (int i = 0; i < Mathf.Min(transform.childCount, expectedCells); i++)
        {
            Transform child = transform.GetChild(i);
            Cell cell = child.GetComponent<Cell>();
            if (cell != null)
            {
                if (cell.blocks.Count > 0)
                {
                    cellsWithBlocks++;
                    Debug.Log($"✓ Cell {i}: {child.name} has blocks");
                }
                else
                {
                    Debug.LogWarning($"✗ Cell {i}: {child.name} is EMPTY");
                }
            }
        }
        Debug.Log($"Designed check: {cellsWithBlocks}/{expectedCells} have blocks");
        if (cellsWithBlocks == expectedCells)
        {
            Debug.Log("🎮 GAME OVER - BOARD IS FULL!");
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game Over Panel is not assigned in BoardManager.");
        }
    }
}