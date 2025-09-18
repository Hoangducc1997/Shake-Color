using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance;

    public GameObject cellPrefab;
    public GameObject[] blockPrefabs; // 4 màu block
    public float spawnChance = 0.7f;  // tỉ lệ spawn từng block

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

        Cell cellComp = cell.GetComponent<Cell>();
        if (cellComp == null) Debug.LogError("Cell prefab missing Cell component!");

        // spawn ngẫu nhiên 1->4 block
        SpawnRandomBlocks(cell);
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
        List<string> availableCorners = new List<string>(corners);
        Cell cellComp = cell.GetComponent<Cell>();

        int blockCount = Random.Range(1, 5); // 1->4 block ngẫu nhiên

        for (int i = 0; i < blockCount; i++)
        {
            if (availableCorners.Count == 0) break;

            int cornerIndex = Random.Range(0, availableCorners.Count);
            string corner = availableCorners[cornerIndex];
            availableCorners.RemoveAt(cornerIndex);

            Transform point = cell.transform.Find(corner);
            if (point != null && blockPrefabs.Length > 0)
            {
                if (Random.value <= spawnChance)
                {
                    int randBlock = Random.Range(0, blockPrefabs.Length);
                    GameObject block = Instantiate(blockPrefabs[randBlock], point);
                    block.transform.localPosition = Vector3.zero;
                    block.transform.localScale = Vector3.one;

                    cellComp.AddBlock(block, corner); // thêm corner khi thêm block

                }
            }
        }
    }
}
