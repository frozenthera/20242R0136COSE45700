using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    static string DEFAULT_NAME = "DefaultUserName";

    [SerializeField] RectTransform loadingRect;

    private async void Start()
    {
        Application.targetFrameRate = 60;
        SoundManager.PlayBGM("bgm");
        SetLoadingRect(true);
        try
        {
            await NetworkManager.Instance.TryConnectServer();
            await TrySetUID();
        }
        finally
        {
            SceneManager.Instance.ChangeScene<HomeScene>(new HomeSceneInfo(3));
            SetLoadingRect(false);
        }
    }

    public void StartGame(int diff)
    {
        SetLoadingRect(true);
        SceneManager.Instance.ChangeScene<GameScene>(new GameSceneInfo(diff));
        SetLoadingRect(false);
    }

    public void ReturnHome()
    {
        SetLoadingRect(true);
        SceneManager.Instance.ChangeScene<HomeScene>(new HomeSceneInfo());
        SetLoadingRect(false);
    }

    public async UniTask TrySetUID()
    {
        try
        {
            if (NetworkManager.Instance.NetworkStatus != NetStatus.Available)
            {
                throw new Exception("Network Unavailable");
            }
            if (PlayerPrefs.HasKey("UID"))
            {
                NetworkManager.Instance.g_UID = PlayerPrefs.GetString("UID");
                var username = await NetworkManager.Instance.GetUserInfo(NetworkManager.Instance.g_UID);
                PlayerPrefs.SetString("Username", username.name);
            }
            else
            {
                var res = await NetworkManager.Instance.RegisterUser(DEFAULT_NAME);
                PlayerPrefs.SetString("UID", res);
                PlayerPrefs.SetString("Username", DEFAULT_NAME);
            }
        }
        catch
        {
            throw;
        }
    }

    public void SetLoadingRect(bool value)
    {
        loadingRect.gameObject.SetActive(value);
    }

}

