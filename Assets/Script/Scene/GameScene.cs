using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class GameScene : SingletonBehaviour<GameScene>, IScene
{
    public GameSceneInfo _gameSceneInfo {  get; private set; }
    [SerializeField] GamePresenter _gamePresenter;

    public void Init(object info)
    {
        _gameSceneInfo = info as GameSceneInfo ?? new GameSceneInfo();
        _gamePresenter.Init(_gameSceneInfo);
    }
}

public class GameSceneInfo
{
    public int cubeSize;

    public GameSceneInfo()
    {
        cubeSize = 3;
    }

    public GameSceneInfo(int _cubeSize)
    {
        cubeSize = _cubeSize;
    }
}
