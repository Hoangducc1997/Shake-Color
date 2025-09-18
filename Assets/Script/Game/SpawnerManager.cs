using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance;
    public GameObject cellPrefab;
    public GameObject[] blockPrefabs;
    public float spawnChance = 0.7f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnCell();
    }

    public void SpawnCell()
    {
        GameObject cell = Instantiate(cellPrefab, transform, false);
        EnsureCorners(cell);

        // Spawn block ngẫu nhiên (có thể kéo thả)
        SpawnRandomBlocks(cell, true);

        // Thêm component DraggableCell cho toàn bộ cell
        if (cell.GetComponent<DraggableCell>() == null)
        {
            cell.AddComponent<DraggableCell>();
        }
    }

    void EnsureCorners(GameObject cell)
    {
        string[] corners = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
        foreach (string name in corners)
        {
            Transform t = cell.transform.Find(name);
            if (t == null)
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

    // Sửa phần spawn block trong SpawnRandomBlocks
    void SpawnRandomBlocks(GameObject cell, bool isDraggable)
    {
        string[] corners = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
        List<string> availableCorners = new List<string>(corners);
        Cell cellComp = cell.GetComponent<Cell>();

        int blockCount = Random.Range(1, 5);

        for (int i = 0; i < blockCount; i++)
        {
            if (availableCorners.Count == 0) break;

            int cornerIndex = Random.Range(0, availableCorners.Count);
            string corner = availableCorners[cornerIndex];
            availableCorners.RemoveAt(cornerIndex);

            Transform point = cell.transform.Find(corner);
            if (point != null && blockPrefabs.Length > 0 && Random.value <= spawnChance)
            {
                int randBlock = Random.Range(0, blockPrefabs.Length);
                GameObject block = Instantiate(blockPrefabs[randBlock], point);
                block.transform.localPosition = Vector3.zero;
                block.transform.localScale = Vector3.one;

                // Xóa component DraggableCell từ các block riêng lẻ
                if (block.GetComponent<DraggableCell>() != null)
                {
                    Destroy(block.GetComponent<DraggableCell>());
                }

                cellComp.AddBlock(block, corner);
            }
        }
    }
}