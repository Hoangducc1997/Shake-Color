using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Dictionary<string, GameObject> blocks = new Dictionary<string, GameObject>();

    public void AddBlock(GameObject block, string corner)
    {
        if (!blocks.ContainsKey(corner)) blocks.Add(corner, block);
    }

    public void RemoveBlock(string corner)
    {
        if (blocks.ContainsKey(corner)) blocks.Remove(corner);
    }

    public void RemoveBlock(GameObject block)
    {
        string keyToRemove = null;
        foreach (var kv in blocks)
        {
            if (kv.Value == block) { keyToRemove = kv.Key; break; }
        }
        if (keyToRemove != null) blocks.Remove(keyToRemove);
    }

    public GameObject GetBlockAtCorner(string corner)
    {
        return blocks.ContainsKey(corner) ? blocks[corner] : null;
    }

    public List<GameObject> GetAllBlocks()
    {
        return new List<GameObject>(blocks.Values);
    }

    public string GetNearestCorner(Vector3 localPos)
    {
        float minDist = float.MaxValue;
        string nearest = null;
        foreach (var kv in blocks)
        {
            Vector3 pos = kv.Value.transform.localPosition;
            float dist = Vector3.Distance(localPos, pos);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = kv.Key;
            }
        }
        return nearest;
    }

    public bool HasBlocks() => blocks.Count > 0;
}
