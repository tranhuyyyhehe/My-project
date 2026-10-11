using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Shapes
{
    static readonly string[][] Bases =
    {
        new[] { "#" },
        new[] { "##" },
        new[] { "###" },
        new[] { "####" },
        new[] { "#####" },
        new[] { "##", "##" },
        new[] { "###", "###", "###" },
        new[] { "#.", "##" },
        new[] { "#.", "#.", "##" },
        new[] { "###", ".#." },
        new[] { "##.", ".##" }
    };

    static readonly bool[] Mirror =
    {
        false, false, false, false, false, false, false, false, true, false, true
    };

    static List<Vector2Int[]> all;

    public static List<Vector2Int[]> All
    {
        get
        {
            if (all == null) Build();
            return all;
        }
    }

    static void Build()
    {
        all = new List<Vector2Int[]>();
        var seen = new HashSet<string>();
        for (int i = 0; i < Bases.Length; i++)
        {
            var start = Parse(Bases[i]);
            var variants = new List<Vector2Int[]>();
            var cur = start;
            for (int k = 0; k < 4; k++)
            {
                variants.Add(cur);
                cur = Rotate(cur);
            }
            if (Mirror[i])
            {
                cur = Flip(start);
                for (int k = 0; k < 4; k++)
                {
                    variants.Add(cur);
                    cur = Rotate(cur);
                }
            }
            foreach (var v in variants)
                if (seen.Add(Key(v))) all.Add(v);
        }
    }

    static Vector2Int[] Parse(string[] rows)
    {
        var l = new List<Vector2Int>();
        for (int r = 0; r < rows.Length; r++)
            for (int c = 0; c < rows[r].Length; c++)
                if (rows[r][c] == '#') l.Add(new Vector2Int(c, r));
        return Normalize(l);
    }

    static Vector2Int[] Normalize(IEnumerable<Vector2Int> pts)
    {
        var list = pts.ToList();
        int mx = list.Min(p => p.x);
        int my = list.Min(p => p.y);
        return list.Select(p => new Vector2Int(p.x - mx, p.y - my))
                   .OrderBy(p => p.y).ThenBy(p => p.x).ToArray();
    }

    static Vector2Int[] Rotate(Vector2Int[] s)
    {
        return Normalize(s.Select(p => new Vector2Int(-p.y, p.x)));
    }

    static Vector2Int[] Flip(Vector2Int[] s)
    {
        return Normalize(s.Select(p => new Vector2Int(-p.x, p.y)));
    }

    static string Key(Vector2Int[] s)
    {
        return string.Join(";", s.Select(p => p.x + "," + p.y).ToArray());
    }
}
