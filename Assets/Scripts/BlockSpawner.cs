using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSpawner : MonoBehaviour
{
    public RectTransform[] Slots;
    public RectTransform DragLayer;
    public RectTransform CanvasRect;
    public Sprite CellSprite;
    public GameManager Game;

    readonly List<BlockDrag> pieces = new List<BlockDrag>();

    public int Remaining { get { return pieces.Count; } }

    public void SpawnSet()
    {
        ClearAll();
        for (int i = 0; i < GameConfig.PieceCount; i++) Spawn(Slots[i]);
    }

    public void ClearAll()
    {
        foreach (var p in pieces)
            if (p != null) Destroy(p.gameObject);
        pieces.Clear();
    }

    public void Remove(BlockDrag p)
    {
        pieces.Remove(p);
        Destroy(p.gameObject);
    }

    public bool AnyFits(BoardController board)
    {
        foreach (var p in pieces)
            if (board.CanFit(p.Cells)) return true;
        return pieces.Count == 0;
    }

    void Spawn(RectTransform slot)
    {
        var cells = Shapes.All[Random.Range(0, Shapes.All.Count)];
        float cs = GameConfig.CellSize;

        var go = new GameObject("Piece", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(slot, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

        var drag = go.AddComponent<BlockDrag>();
        drag.Init(cells, slot, DragLayer, CanvasRect, Game);

        int w = drag.Width;
        int h = drag.Height;
        rt.sizeDelta = new Vector2(w * cs, h * cs);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one * GameConfig.TrayScale;

        var hit = new GameObject("Hit", typeof(RectTransform), typeof(Image));
        var hr = (RectTransform)hit.transform;
        hr.SetParent(rt, false);
        hr.anchorMin = hr.anchorMax = hr.pivot = new Vector2(0.5f, 0.5f);
        hr.anchoredPosition = Vector2.zero;
        hr.sizeDelta = new Vector2((w + 1) * cs, (h + 1) * cs);
        var hi = hit.GetComponent<Image>();
        hi.color = new Color(1f, 1f, 1f, 0f);
        hi.raycastTarget = true;

        foreach (var cell in cells)
        {
            var cg = new GameObject("Cell", typeof(RectTransform), typeof(Image));
            var cr = (RectTransform)cg.transform;
            cr.SetParent(rt, false);
            cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(cs, cs);
            cr.anchoredPosition = new Vector2((cell.x - (w - 1) / 2f) * cs, -(cell.y - (h - 1) / 2f) * cs);
            var ci = cg.GetComponent<Image>();
            ci.sprite = CellSprite;
            ci.raycastTarget = false;
        }

        pieces.Add(drag);
    }
}
