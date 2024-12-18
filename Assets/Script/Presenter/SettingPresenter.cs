using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SettingPresenter : MonoBehaviour, IDisposable
{
    [SerializeField] SettingView _settingView;
    private HomePresenter _homePresenter;

    public void Init(HomePresenter _hp)
    {
        _homePresenter = _hp;

        _settingView.BackButton.onClick.RemoveAllListeners();
        _settingView.Master.onValueChanged.RemoveAllListeners();
        _settingView.Master.onValueChanged.RemoveAllListeners();
        _settingView.Vfx.onValueChanged.RemoveAllListeners();

        _settingView.BackButton.onClick.AddListener(CloseSettingPanel);
        _settingView.BackButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));

        _settingView.Master.value = SoundManager.Instance.MasterVolume;
        _settingView.Bgm.value = SoundManager.Instance.BgmVolume;
        _settingView.Vfx.value = SoundManager.Instance.VfxVolume;

        _settingView.Master.onValueChanged.AddListener(SoundManager.ChangeMasterVolume);
        _settingView.Bgm.onValueChanged.AddListener(SoundManager.ChangeBgmVolume);
        _settingView.Vfx.onValueChanged.AddListener(SoundManager.ChangeVfxVolume);
    }

    public void OnOpen()
    {
        _settingView.OnOpen();
    }

    public void Dispose()
    {
        
    }

    public void CloseSettingPanel()
    {
        _settingView.OnClose();
        _homePresenter.OnOpen();
    }
}
