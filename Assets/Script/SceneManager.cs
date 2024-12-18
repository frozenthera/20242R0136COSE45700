using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneManager : Singleton<SceneManager>
{
    private readonly Dictionary<Type, (GameObject obj, IScene scene)> _sceneDict;
    private readonly Stack<(GameObject obj, IScene scene)> _sceneStack;

    public SceneManager()
    {
        _sceneStack = new();
        _sceneDict = new();
    }
        
    public bool CheckSceneActivated<SceneT>() where SceneT : MonoBehaviour, IScene
    {
        return _sceneStack.Any(stackContent => stackContent.scene is SceneT);
    }

    public void ChangeScene<SceneT>(object info) where SceneT : MonoBehaviour, IScene
    {
        SceneControl<SceneT>(info, true);
    }

    public void PushScene<SceneT>(object info) where SceneT : MonoBehaviour, IScene
    {
        SceneControl<SceneT>(info, false);
    }

    private void SceneControl<SceneT>(object info, bool emptyStack) where SceneT : MonoBehaviour, IScene
    {
        GameObject obj;
        IScene scene;
        if (_sceneDict.TryGetValue(typeof(SceneT), out var temp) && temp.obj != null)
        {
            (obj, scene) = temp;
        }
        else
        {
            var path = typeof(SceneT).GetProperty("AssetPath")?.GetValue(null) as string ??
                       $"{IScene.AssetPath}/{typeof(SceneT).FullName}";
            obj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(path));
            obj.name = typeof(SceneT).FullName;
            scene = obj.GetComponent<SceneT>();
            scene.Load();

            _sceneDict[typeof(SceneT)] = (obj, scene);
        }

        //Debug.Log(_sceneStack.Count);
        if (emptyStack) while (_sceneStack.Count > 0) PopScene();

        obj.SetActive(true);
        scene.Init(info);

        _sceneStack.Push((obj, scene));
    }

    public void PopScene()
    {
        var (obj, scene) = _sceneStack.Pop();
        scene.Finish();
        obj.SetActive(false);
    }

    public void PopScene(object message)
    {
        var (obj, scene) = _sceneStack.Pop();
        scene.Finish();
        obj.SetActive(false);

        _sceneStack.Peek().scene.Resume(message);
    }

    public TScene Get<TScene>() => (TScene)_sceneDict[typeof(TScene)].scene;
}