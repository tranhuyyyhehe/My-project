using UnityEngine;
using UnityEngine.EventSystems;

public class BlockDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector2Int[] Cells;
    public int Width;
    public int Height;

    RectTransform rt;
    RectTransform slot;
    RectTransform dragLayer;
    RectTransform canvasRect;
    GameManager game;
    bool dragging;

    public void Init(Vector2Int[] cells, RectTransform slotRt, RectTransform dragLayerRt, RectTransform canvasRt, GameManager gm)
    {
        Cells = cells;
        Width = 0;
        Height = 0;
        foreach (var c in cells)
        {
            Width = Mathf.Max(Width, c.x + 1);
            Height = Mathf.Max(Height, c.y + 1);
        }
        rt = (RectTransform)transform;
        slot = slotRt;
        dragLayer = dragLayerRt;
        canvasRect = canvasRt;
        game = gm;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (!game.CanInput) return;
        dragging = true;
        rt.SetParent(dragLayer, true);
        rt.SetAsLastSibling();
        rt.localScale = Vector3.one;
        Move(e);
        game.Board.ShowPreview(this);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!dragging) return;
        Move(e);
        game.Board.ShowPreview(this);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!dragging) return;
        dragging = false;
        game.Board.ClearPreview();
        if (!game.TryPlace(this)) ReturnToSlot();
    }

    void Move(PointerEventData e)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, e.position, null, out lp);
        rt.position = canvasRect.TransformPoint(lp + Vector2.up * GameConfig.DragOffsetY);
    }

    void ReturnToSlot()
    {
        rt.SetParent(slot, false);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one * GameConfig.TrayScale;
    }
}
