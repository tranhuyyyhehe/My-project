using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text ScoreText;
    public TMP_Text BestText;

    const string BestKey = "BlockBlastBest";
    int score;
    int best;

    void Awake()
    {
        best = PlayerPrefs.GetInt(BestKey, 0);
        Refresh();
    }

    public void ResetScore()
    {
        score = 0;
        Refresh();
    }

    public void Add(int value)
    {
        score += value;
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt(BestKey, best);
            PlayerPrefs.Save();
        }
        Refresh();
    }

    void Refresh()
    {
        ScoreText.text = score.ToString();
        BestText.text = best.ToString();
    }
}
