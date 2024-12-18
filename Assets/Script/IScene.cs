using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScene
{
    /// <summary>
    /// Resources 폴더 내부 씬 프리팹의 기본 경로 Resources/(AssetPath)/(씬타입).prefab으로 기본 탐색됨
    /// 하지만 씬 각각에 이와 동일한 프로퍼티를 두어 경로를 직접 지정해 주는 것을 추천
    /// </summary>
    public static string AssetPath => "Scenes";

    /// <summary>
    /// 씬 최초 생성 시
    /// </summary>
    public void Load() { }

    /// <summary>
    /// 씬 시작 시
    /// </summary>
    /// <param name="info"> 씬 시작 시 세팅에 필요한 정보 </param>
    public void Init(object info);

    /// <summary>
    /// 후행 팝업이 꺼지며 팝업에서 준 메시지 처리
    /// </summary>
    /// <param name="message"> 후행 팝업에서 전달한 메시지 </param>
    public void Resume(object message) { }

    /// <summary>
    /// 씬 종료 시
    /// </summary>
    public void Finish() { }
}