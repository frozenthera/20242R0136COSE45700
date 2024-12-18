using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScene
{
    /// <summary>
    /// Resources ���� ���� �� �������� �⺻ ��� Resources/(AssetPath)/(��Ÿ��).prefab���� �⺻ Ž����
    /// ������ �� ������ �̿� ������ ������Ƽ�� �ξ� ��θ� ���� ������ �ִ� ���� ��õ
    /// </summary>
    public static string AssetPath => "Scenes";

    /// <summary>
    /// �� ���� ���� ��
    /// </summary>
    public void Load() { }

    /// <summary>
    /// �� ���� ��
    /// </summary>
    /// <param name="info"> �� ���� �� ���ÿ� �ʿ��� ���� </param>
    public void Init(object info);

    /// <summary>
    /// ���� �˾��� ������ �˾����� �� �޽��� ó��
    /// </summary>
    /// <param name="message"> ���� �˾����� ������ �޽��� </param>
    public void Resume(object message) { }

    /// <summary>
    /// �� ���� ��
    /// </summary>
    public void Finish() { }
}