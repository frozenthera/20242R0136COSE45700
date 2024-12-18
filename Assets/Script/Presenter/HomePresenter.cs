using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HomePresenter : MonoBehaviour, IDisposable
{
    [SerializeField] HomeView _homeView;
    [SerializeField] float RotateSpeed = 3f;

    private int difficulty = 3;
    private Queue<AxisDir> rotQueue = new();
    private CubeInfo cubeInfo;

    private CustomPresenter _customPresenter;
    private LeaderboardPresenter _leaderboardPresenter;
    private SettingPresenter _settingPresenter;

    //magic number
    private readonly string cubemask = "10101000000000000000111110000100000000000000000001000011010110101101011111100000000000000000000100000000000000000000000001111";

    private CancellationTokenSource g_cts;

    public void Init(int _difficulty, CustomPresenter _cp, LeaderboardPresenter _lp, SettingPresenter _sp)
    {
        _customPresenter = _cp;
        _leaderboardPresenter = _lp;
        _settingPresenter = _sp;

        OnSetDifficulty(_difficulty);

        _homeView.CustomButton.onClick.AddListener(OpenCustomPanel);
        _homeView.RankingButton.onClick.AddListener(OpenLeaderboardPanel);
        _homeView.SettingButton.onClick.AddListener(OpenSettingPanel);
        _homeView.CustomButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));
        _homeView.RankingButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));
        _homeView.SettingButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));

        bool[] bits = new bool[125];
        for (int i = 0; i < cubemask.Length; i++)
        {
            bits[i] = cubemask[cubemask.Length - i - 1] == '1';
        }
        cubeInfo = new CubeInfo(5, new BitArray(bits));

        _homeView.Init(cubeInfo);
    }

    public void OnOpen()
    {
        g_cts = new CancellationTokenSource();
        SerialRotation(g_cts).Forget();

        InputHandler.Instance.OnDragEnd += OnDragInput;
        InputHandler.Instance.OnTouchEnd += StartGame;
        InputHandler.Instance.OnTouchEnd += (_) => SoundManager.PlayEffect("click");

        _homeView.OnOpen();
    }

    public void OnClose()
    {
        g_cts?.Cancel();
        _homeView.OnClose();
        InputHandler.Instance.ResetHandler();
        Debug.Log("HOME CLOSED");
    }

    public void Dispose()
    {
        _homeView.Dispose();
    }

    public void StartGame(Vector2 _)
    {
        OnClose();
        GameManager.Instance.StartGame(MapPhaseToDiff(cubeInfo.phaseNode));
    }

    private int MapPhaseToDiff(int phase)
    {
        return phase switch
        {
            >= 0 and <= 3 or >= 20 and <= 23 => 3,
            >= 8 and <= 11 or >= 16 and <= 19 => 5,
            >= 4 and <= 15 => 4,
            _  => 3
        };
    }

    public void OpenCustomPanel()
    {
        OnClose();
        _customPresenter.OnOpen();
    }

    public void OpenLeaderboardPanel()
    {
        OnClose();
        _leaderboardPresenter.OnOpen();
    }
    public void OpenSettingPanel()
    {
        OnClose();
        _settingPresenter.OnOpen();
    }

    public void OnSetDifficulty(int _diff)
    {
        _diff = _diff % 3 + 3; 
        difficulty = _diff;

        SetUserScore(difficulty).Forget();
    }

    public async UniTaskVoid SetUserScore(int difficulty)
    {
        User res;
        try
        {
            res = await NetworkManager.Instance.GetUserInfo(NetworkManager.Instance.g_UID);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            _homeView.SetHighScore(0, true);
            return;
        }
        
        _homeView.SetHighScore(
            difficulty switch
            { 
                3 => res.max_score_3,
                4 => res.max_score_4,
                5 => res.max_score_5,
                _ => res.max_score_3            
            }
        );
    }

    public void OnDragInput(Vector2 st, Vector2 ed)
    {
        SoundManager.PlayEffect("swipe");
        rotQueue.Enqueue(DetermineAxisDir(st, ed));
    }

    private async UniTask SerialRotation(CancellationTokenSource cts)
    {
        while (!cts.Token.IsCancellationRequested)    
        {
            await UniTask.WaitUntil(() => rotQueue.Count > 0);

            if (!cts.Token.IsCancellationRequested && rotQueue.TryDequeue(out var res))
            {
                cubeInfo.RotateCube(res);
                await _homeView.RotateCubeObject(res, RotateSpeed, cts);
            }
            _homeView.ProjectionMeshFilter.mesh = MeshHandler.GetMeshFromQuadPoints(cubeInfo.SideProjectionSet());
            OnSetDifficulty(MapPhaseToDiff(cubeInfo.phaseNode));
        }
    }

    private AxisDir DetermineAxisDir(Vector2 st, Vector2 ed)
    {
        Vector2 diff = ed - st;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            return diff.x > 0 ? AxisDir.negY : AxisDir.posY;
        }
        if (st.x < Screen.width / 2)
        {
            return diff.y > 0 ? AxisDir.negZ : AxisDir.posZ;
        }
        return diff.y > 0 ? AxisDir.posX : AxisDir.negX;
    }
}

