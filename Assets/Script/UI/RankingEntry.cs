using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RankingEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI RankingNum;
    [SerializeField] TextMeshProUGUI RankingName;
    [SerializeField] TextMeshProUGUI RankingScore;

    public void SetInfo(RankEntry entry) => SetInfo(entry.rank, entry.name, entry.max_score);
    public void SetInfo(int num, string name, int score)
    {
        RankingNum.text = num.ToString();
        RankingName.text = name;
        RankingScore.text = score.ToString();
    }
}
