using UnityEngine;
using System.Collections.Generic;

public class Cell : MonoBehaviour
{
    public Dictionary<string, GameObject> blocks = new Dictionary<string, GameObject>();
    public void AddBlock(GameObject block, string corner)
    {
        if (!blocks.ContainsKey(corner))
        {
            blocks.Add(corner, block);
        }
    }
    public void RemoveBlock(string corner)
    {
        if (blocks.ContainsKey(corner))
        {
            blocks.Remove(corner);
        }
    }

    public void RemoveBlock(GameObject block)
    {
        foreach (var pair in blocks)
        {
            if (pair.Value == block)
            {
                blocks.Remove(pair.Key);
                break;
            }
        }
    }
    public GameObject GetBlockAtCorner(string corner)
    {
        return blocks.ContainsKey(corner) ? blocks[corner] : null;
    }

    public List<GameObject> GetAllBlocks()
    {
        return new List<GameObject>(blocks.Values);
    }

    public bool HasBlocks()
    {
        return blocks.Count > 0;
    }

    public bool HasBlockOfColor(int colorID)
    {
        foreach (var block in blocks.Values)
        {
            BlockColor bc = block.GetComponent<BlockColor>();
            if (bc != null && bc.colorID == colorID)
                return true;
        }
        return false;
    }
    public bool HasBlockOfColorAtCorner(string corner, int colorID)
    {
        if (!blocks.ContainsKey(corner)) return false;

        GameObject block = blocks[corner];
        BlockColor blockColor = block.GetComponent<BlockColor>();
        return blockColor != null && blockColor.colorID == colorID;
    }
    public List<GameObject> GetBlocksOfColor(int colorID)
    {
        List<GameObject> coloredBlocks = new List<GameObject>();
        foreach (var block in blocks.Values)
        {
            BlockColor blockColor = block.GetComponent<BlockColor>();
            if (blockColor != null && blockColor.colorID == colorID)
                coloredBlocks.Add(block);
        }
        return coloredBlocks;
    }
    public void RemoveBlocksOfColor(int colorID)
    {
        List<string> cornersToRemove = new List<string>();

        foreach (var pair in blocks)
        {
            BlockColor bc = pair.Value.GetComponent<BlockColor>();
            if (bc != null && bc.colorID == colorID)
            {
                Destroy(pair.Value);
                cornersToRemove.Add(pair.Key);
            }
        }

        foreach (string corner in cornersToRemove)
        {
            blocks.Remove(corner);
        }
    }
    public void ClearAllBlocks()
    {
        foreach (var block in blocks.Values)
        {
            Destroy(block);
        }
        blocks.Clear();
    }
    public bool IsCornerEmpty(string corner)
    {
        return !blocks.ContainsKey(corner);
    }

    public string GetNearestCorner(Vector2 localPos)
    {
        float minDist = Mathf.Infinity;
        string nearestCorner = "";

        string[] corners = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };

        foreach (string corner in corners)
        {
            Transform cornerTransform = transform.Find(corner);
            if (cornerTransform != null)
            {
                float dist = Vector2.Distance(localPos, cornerTransform.localPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestCorner = corner;
                }
            }
        }

        return nearestCorner;
    }
    public List<GameObject> GetAllBlocksOfColor(int colorID)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (var pair in blocks)
        {
            var blockColor = pair.Value.GetComponent<BlockColor>();
            if (blockColor != null && blockColor.colorID == colorID)
            {
                result.Add(pair.Value);
            }
        }
        return result;
    }
}