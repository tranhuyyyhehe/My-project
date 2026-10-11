using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardController Board;
    public BlockSpawner Spawner;
    public ScoreManager Score;
    public UIManager UI;

    public bool CanInput { get; private set; }

    void Start()
    {
        UI.ReplayButton.onClick.AddListener(NewGame);
        NewGame();
    }

    public void NewGame()
    {
        StopAllCoroutines();
        Board.ResetBoard();
        Score.ResetScore();
        UI.HideReplay();
        Spawner.SpawnSet();
        CanInput = true;
    }

    public bool TryPlace(BlockDrag p)
    {
        if (!CanInput) return false;
        int c, r;
        Board.GetOrigin(p, out c, out r);
        if (!Board.CanPlace(p.Cells, c, r)) return false;
        Board.Place(p.Cells, c, r);
        Score.Add(p.Cells.Length);
        Spawner.Remove(p);
        StartCoroutine(AfterPlace());
        return true;
    }

    IEnumerator AfterPlace()
    {
        CanInput = false;
        System.Collections.Generic.List<int> rows;
        System.Collections.Generic.List<int> cols;
        Board.FindFullLines(out rows, out cols);
        if (rows.Count + cols.Count > 0)
        {
            Board.HighlightLines(rows, cols);
            yield return new WaitForSeconds(GameConfig.ClearDelay);
            Score.Add(Board.ClearLines(rows, cols));
        }
        if (Spawner.Remaining == 0) Spawner.SpawnSet();
        if (Spawner.AnyFits(Board))
        {
            CanInput = true;
        }
        else
        {
            UI.ShowReplay();
        }
    }
}
