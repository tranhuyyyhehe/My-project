using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{
    public RectTransform GridRect;
    public Image[] Fills;
    public Sprite NormalSprite;
    public Sprite HighlightSprite;

    readonly bool[,] occ = new bool[GameConfig.Size, GameConfig.Size];

    public void ResetBoard()
    {
        System.Array.Clear(occ, 0, occ.Length);
        Refresh();
    }

    void Paint(int r, int c, Sprite s)
    {
        var im = Fills[r * GameConfig.Size + c];
        im.sprite = s;
        im.enabled = s != null;
    }

    public void Refresh()
    {
        for (int r = 0; r < GameConfig.Size; r++)
            for (int c = 0; c < GameConfig.Size; c++)
                Paint(r, c, occ[r, c] ? NormalSprite : null);
    }

    public void GetOrigin(BlockDrag p, out int col, out int row)
    {
        float cs = GameConfig.CellSize;
        float half = cs * GameConfig.Size / 2f;
        Vector3 local = GridRect.InverseTransformPoint(p.transform.position);
        float ox = local.x - p.Width * cs / 2f;
        float oy = local.y + p.Height * cs / 2f;
        col = Mathf.RoundToInt((ox + half) / cs);
        row = Mathf.RoundToInt((half - oy) / cs);
    }

    public bool CanPlace(Vector2Int[] cells, int col, int row)
    {
        foreach (var cell in cells)
        {
            int c = col + cell.x;
            int r = row + cell.y;
            if (c < 0 || c >= GameConfig.Size || r < 0 || r >= GameConfig.Size || occ[r, c])
                return false;
        }
        return true;
    }

    public void ShowPreview(BlockDrag p)
    {
        Refresh();
        int c, r;
        GetOrigin(p, out c, out r);
        if (!CanPlace(p.Cells, c, r)) return;
        foreach (var cell in p.Cells)
            Paint(r + cell.y, c + cell.x, HighlightSprite);
    }

    public void ClearPreview()
    {
        Refresh();
    }

    public void Place(Vector2Int[] cells, int col, int row)
    {
        foreach (var cell in cells)
            occ[row + cell.y, col + cell.x] = true;
        Refresh();
    }

    public void FindFullLines(out List<int> rows, out List<int> cols)
    {
        rows = new List<int>();
        cols = new List<int>();
        for (int r = 0; r < GameConfig.Size; r++)
        {
            bool full = true;
            for (int c = 0; c < GameConfig.Size; c++)
                if (!occ[r, c]) { full = false; break; }
            if (full) rows.Add(r);
        }
        for (int c = 0; c < GameConfig.Size; c++)
        {
            bool full = true;
            for (int r = 0; r < GameConfig.Size; r++)
                if (!occ[r, c]) { full = false; break; }
            if (full) cols.Add(c);
        }
    }

    bool[,] Mask(List<int> rows, List<int> cols)
    {
        var m = new bool[GameConfig.Size, GameConfig.Size];
        foreach (int r in rows)
            for (int c = 0; c < GameConfig.Size; c++) m[r, c] = true;
        foreach (int c in cols)
            for (int r = 0; r < GameConfig.Size; r++) m[r, c] = true;
        return m;
    }

    public void HighlightLines(List<int> rows, List<int> cols)
    {
        var m = Mask(rows, cols);
        for (int r = 0; r < GameConfig.Size; r++)
            for (int c = 0; c < GameConfig.Size; c++)
                if (m[r, c]) Paint(r, c, HighlightSprite);
    }

    public int ClearLines(List<int> rows, List<int> cols)
    {
        var m = Mask(rows, cols);
        int n = 0;
        for (int r = 0; r < GameConfig.Size; r++)
            for (int c = 0; c < GameConfig.Size; c++)
                if (m[r, c]) { occ[r, c] = false; n++; }
        Refresh();
        return n;
    }

    public bool CanFit(Vector2Int[] cells)
    {
        for (int r = 0; r < GameConfig.Size; r++)
            for (int c = 0; c < GameConfig.Size; c++)
                if (CanPlace(cells, c, r)) return true;
        return false;
    }
}
