using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [SerializeField] GameObject rankingEntryPrefab;
    [SerializeField] RankingEntry userRankingEntry;
    [SerializeField] RectTransform scrollRect;
    [SerializeField] Button backButton;
    [SerializeField] TextMeshProUGUI errMsg;
    [SerializeField] TextMeshProUGUI curDiffTxt;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] int maxEntry = 10;
    public Button BackButton => backButton;
    public Button PrevButton => prevButton;
    public Button NextButton => nextButton;
    List<Poolable> entryList;
    
    public void SetLeaderBoardErr(string _errMsg)
    {
        errMsg.text = "Unable to Connect Server";
        errMsg.gameObject.SetActive(true);
        Debug.Log(_errMsg);
    }

    /// <summary>
    /// Generate Current User's Ranking Entry
    /// </summary>
    public void SetLeaderBoardUser(int userRank, string username, int userScore)
    {
        userRankingEntry.SetInfo(userRank, username, userScore);
    }

    /// <summary>
    /// Generate Leading User's Ranking Entry
    /// </summary>
    public void SetLeaderBoardGlobal(List<RankEntry> globalList)
    {
        errMsg.text = "Connecting...";
        errMsg.gameObject.SetActive(true);

        if (entryList != null)
        {
            foreach (var item in entryList)
            {
                item.ReleaseObject();
            }
            entryList.Clear();
        }
        else
        {
            entryList = new();
        }
        
        globalList.Sort((x, y) => x.rank - y.rank);
        int entryCnt = Mathf.Min(globalList.Count, maxEntry);
        for(int i = 0; i  < entryCnt; i++)
        {
            var obj = ObjectPool.Instance.GetGO("RankingEntry");
            obj.transform.parent = scrollRect;
            obj.transform.localScale = Vector3.one;
            entryList.Add(obj.GetComponent<Poolable>());
            obj.GetComponent<RankingEntry>().SetInfo(globalList[i]);
        }

        Vector2 sizeDelta = scrollRect.sizeDelta;
        sizeDelta.y = (75 + 20) * entryCnt - 20;
        scrollRect.sizeDelta = sizeDelta;

        errMsg.gameObject.SetActive(false);
    }

    public void SetDifficultyText(int diff)
    {
        curDiffTxt.text = $"{diff}x{diff}";
    }


    public void OnClose()
    {
        for(int i = scrollRect.childCount-1; i >= 0; i--)
        {
            foreach (var item in entryList)
            {
                item.ReleaseObject();
            }
            entryList.Clear();
            //scrollRect.GetChild(i).GetComponent<Poolable>().ReleaseObject();
        }
        gameObject.SetActive(false);
    }

    public void OnOpen()
    {
        gameObject.SetActive(true);
    }
}
