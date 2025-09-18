using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector3 startPos;
    private Transform startParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rectTransform.position;
        startParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        rectTransform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        Cell nearestCell = BoardManager.Instance.GetNearestCell(rectTransform.position);

        if (nearestCell != null && nearestCell.blocks.Count == 0)
        {
            rectTransform.SetParent(nearestCell.transform);
            rectTransform.anchoredPosition = Vector2.zero;

            BlockColor bc = GetComponent<BlockColor>();
            if (bc != null)
                BoardManager.Instance.CheckAndClearMatches(nearestCell, bc.colorID);

            this.enabled = false;

            SpawnerManager spawner = startParent.GetComponent<SpawnerManager>();
            if (spawner != null)
                spawner.SpawnCell();

            return;
        }

        rectTransform.SetParent(startParent);
        rectTransform.position = startPos;
    }
}
