using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPresenter : MonoBehaviour, IDisposable
{
    [SerializeField] CustomView _customView;

    private HomePresenter _homePresenter;

    public void Init(HomePresenter _hp)
    {
        _homePresenter = _hp;

        _customView.BackButton.onClick.RemoveAllListeners();
        _customView.BackButton.onClick.AddListener(CloseCustomPanel);
        _customView.BackButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));

        _customView.UsernameChangeButton.onClick.RemoveAllListeners();
        _customView.UsernameChangeButton.onClick.AddListener(ChangeUsername);
        _customView.UsernameChangeButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));
    }

    public void OnOpen()
    {
        var uid = NetworkManager.Instance.g_UID ?? "-";
        var username = PlayerPrefs.HasKey("Username") ? PlayerPrefs.GetString("Username") : "Default";
        _customView.Init(uid, username);
    }

    public void Dispose()
    {

    }

    public void CloseCustomPanel()
    {
        _customView.OnClose();
        _homePresenter.OnOpen();
    }

    public void ChangeUsername()
    {
        var changed = _customView.UsernameInputField.text;
        NetworkManager.Instance.UpdateUserInfo(NetworkManager.Instance.g_UID, changed);
    }

}
