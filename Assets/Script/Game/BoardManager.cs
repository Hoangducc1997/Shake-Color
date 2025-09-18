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
        if (!grid) { Debug.LogError("Add GridLayoutGroup"); return; }

        for (int i = 0; i < rows * cols; i++)
        {
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.name = $"Cell_{i}";
            EnsureCorners(cell);
            SpawnRandomBlocks(cell);
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
                RectTransform rt = point.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
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

    void SpawnRandomBlocks(GameObject cell)
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

    public void CheckAndClearMatches(Cell cell, int colorID)
    {
        List<Cell> matched = new List<Cell> { cell };
        int index = cell.transform.GetSiblingIndex();
        int row = index / cols;
        int col = index % cols;

        for (int c = 0; c < cols; c++)
        {
            if (c == col) continue;
            Cell other = GetCellAt(row, c);
            if (other.HasBlocks())
            {
                foreach (var b in other.GetAllBlocks())
                {
                    BlockColor bc = b.GetComponent<BlockColor>();
                    if (bc != null && bc.colorID == colorID)
                    {
                        matched.Add(other);
                        break;
                    }
                }
            }
        }

        for (int r = 0; r < rows; r++)
        {
            if (r == row) continue;
            Cell other = GetCellAt(r, col);
            if (other.HasBlocks())
            {
                foreach (var b in other.GetAllBlocks())
                {
                    BlockColor bc = b.GetComponent<BlockColor>();
                    if (bc != null && bc.colorID == colorID)
                    {
                        matched.Add(other);
                        break;
                    }
                }
            }
        }

        if (matched.Count >= 2)
        {
            foreach (Cell c in matched)
            {
                foreach (var b in c.GetAllBlocks())
                {
                    BlockColor bc = b.GetComponent<BlockColor>();
                    if (bc != null && bc.colorID == colorID)
                    {
                        Destroy(b);
                        c.RemoveBlock(b);
                    }
                }
            }
            Debug.Log($"Cleared {matched.Count} cells of color {colorID}");
        }
    }

    public Cell GetCellAt(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols) return null;
        int index = row * cols + col;
        return index < transform.childCount ? transform.GetChild(index).GetComponent<Cell>() : null;
    }

    public void HandleBlockDrop(Cell cell, BlockColor dragBlock, Vector3 dropPos)
    {
        string nearestCorner = cell.GetNearestCorner(cell.transform.InverseTransformPoint(dropPos));
        if (string.IsNullOrEmpty(nearestCorner)) return;
        GameObject targetBlock = cell.GetBlockAtCorner(nearestCorner);
        if (targetBlock == null) return;
        BlockColor bc = targetBlock.GetComponent<BlockColor>();
        if (bc != null && bc.colorID == dragBlock.colorID)
        {
            Destroy(targetBlock);
            cell.RemoveBlock(nearestCorner);
            Debug.Log("Block matched and destroyed!");
        }
    }
}
