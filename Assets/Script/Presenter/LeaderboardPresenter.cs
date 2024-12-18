using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LeaderboardPresenter : MonoBehaviour, IDisposable
{
    [SerializeField] LeaderboardView _leaderboardView;
    private HomePresenter _homePresenter;
    private int curDiff = 3;
    public void Init(HomePresenter _hp)
    {
        _homePresenter = _hp;

        _leaderboardView.BackButton.onClick.RemoveAllListeners();
        _leaderboardView.BackButton.onClick.AddListener(OnClose);
        _leaderboardView.BackButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));

        _leaderboardView.PrevButton.onClick.RemoveAllListeners();
        _leaderboardView.PrevButton.onClick.AddListener(() =>
        {
            curDiff = (curDiff - 1) % 3 + 3;
            SetLeaderBoard().Forget();
        });

        _leaderboardView.NextButton.onClick.RemoveAllListeners();
        _leaderboardView.NextButton.onClick.AddListener(() =>
        {
            curDiff = (curDiff + 1) % 3 + 3;
            SetLeaderBoard().Forget();
        });
    }

    public void OnOpen()
    {
        Debug.Log("Open LeaderBoard!");
        _leaderboardView.OnOpen();
        SetLeaderBoard().Forget();
    }

    public async UniTaskVoid SetLeaderBoard()
    {
        try
        {
            var rank = await NetworkManager.Instance.FindUserRank(curDiff);
            var username = PlayerPrefs.GetString("Username");

            _leaderboardView.SetDifficultyText(curDiff);
            _leaderboardView.SetLeaderBoardUser(rank.rank, username, rank.max_score);

            var g_List = await NetworkManager.Instance.GetRankingInfo(curDiff);
            _leaderboardView.SetLeaderBoardGlobal(g_List);
        }
        catch (Exception ex)
        {
            _leaderboardView.SetLeaderBoardErr(ex.Message);
        }
    }

    public void Dispose()
    {
        
    }

    public void OnClose()
    {
        _leaderboardView.OnClose();
        _homePresenter.OnOpen();
    }
}
