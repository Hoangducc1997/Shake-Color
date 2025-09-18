using UnityEngine;
using UnityEngine.EventSystems;

public class CellDrop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        DraggableCell block = eventData.pointerDrag.GetComponent<DraggableCell>();
        if (block != null)
        {
            // Đặt block vào vị trí cell này
            block.transform.position = transform.position;
            block.transform.SetParent(transform);
        }
    }
}
