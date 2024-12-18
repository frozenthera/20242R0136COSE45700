using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class HomeScene : SingletonBehaviour<HomeScene>, IScene
{
    public HomeSceneInfo _homeSceneInfo { get; private set; }

    [SerializeField] HomePresenter _homePresenter;
    [SerializeField] CustomPresenter _customPresenter;
    [SerializeField] LeaderboardPresenter _leaderboardPresenter;
    [SerializeField] SettingPresenter _settingPresenter;

    public void Init(object info)
    {
        _homeSceneInfo = info as HomeSceneInfo ?? new HomeSceneInfo();

        _homePresenter.Init(_homeSceneInfo.currentMode, _customPresenter, _leaderboardPresenter, _settingPresenter);
        _customPresenter.Init(_homePresenter);
        _leaderboardPresenter.Init(_homePresenter);
        _settingPresenter.Init(_homePresenter);

        _homePresenter.OnOpen();
    }
    
    public void Finish()
    {
        _homePresenter.Dispose();
        InputHandler.Instance.ResetHandler();
    }
}

public class HomeSceneInfo
{
    public int currentMode = 3;
    public HomeSceneInfo()
    {
        currentMode = 3;
    }

    public HomeSceneInfo(int currentMode)
    {
        this.currentMode = currentMode;
    }
}
