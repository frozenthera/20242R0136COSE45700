using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingView : MonoBehaviour
{
    [SerializeField] Button backButton;
    [SerializeField] Slider master;
    [SerializeField] Slider bgm;
    [SerializeField] Slider vfx;

    public Slider Master => master;
    public Slider Bgm => bgm;
    public Slider Vfx => vfx;

    public Button BackButton => backButton;
    public void OnClose()
    {
        gameObject.SetActive(false);
    }

    public void OnOpen()
    {
        gameObject.SetActive(true);
    }
}
