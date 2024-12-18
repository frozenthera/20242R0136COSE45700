using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class GamePresenter : MonoBehaviour, IDisposable
{
    GameSceneInfo _info;

    ProblemModel _problemModel;
    LifeModel _lifeModel;

    [SerializeField] GameView _gameView;

    [Header("Properties")]
    [SerializeField] LifeScriptableObject _lifeSO;
    [SerializeField] float RotateSpeed = 3f;
    [Range(3, 5)][SerializeField] int size = 3;
    [Tooltip("Å¥ºê »ý¼º °¹¼ö\n1 ~ size^3 À¸·Î ClampµÊ")]
    [SerializeField] int numOfDepth = 5;
    [SerializeField] int CubeRegenCycle = 3;
    [Space(10)]
    [SerializeField] bool isPaused = false;

    public event Action<int, int> OnLifeChange;
    public event Action OnTickChanged;

    CubeInfo cubeInfo;
    Queue<AxisDir> rotQueue = new();
    float totalGameTime = 0;
    int CurCubeCycle = 0;

    CancellationTokenSource g_cts;
    public void Init(GameSceneInfo info)
    {
        size = info.cubeSize;

        _problemModel = new ProblemModel();

        _gameView.RetryButton.onClick.RemoveAllListeners();
        _gameView.HomeButton.onClick.RemoveAllListeners();

        _gameView.RetryButton.onClick.AddListener(StartGameCycle);
        _gameView.RetryButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));
        _gameView.HomeButton.onClick.AddListener(MoveToHome);
        _gameView.HomeButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));

        StartGameCycle();
    }

    public void StartGameCycle()
    {
        g_cts = new CancellationTokenSource();

        _problemModel.OnScoreChanged += _gameView.SetScore;

        _lifeModel = new LifeModel(_lifeSO ??= new LifeScriptableObject().Default());
        _lifeModel.OnLifeChanged += _gameView.SetLife;
        _lifeModel.OnLifeExhausted += LifeEnd;

        _gameView.Init(_lifeModel.MaxLife);
        _gameView.CaptureButton.onClick.AddListener(JudgeAnswer);
        _gameView.CaptureButton.onClick.AddListener(() => SoundManager.PlayEffect("click"));

        totalGameTime = 0;
        CurCubeCycle = 0;

        InputHandler.Instance.OnDragEnd += OnDragInput;

        GenerateCubeInfo();
        SetNextProblem();

        OnTickChanged = DecreaseLife;

        SerialRotation().Forget();
        Tick().Forget();
    }

    public void DisposeGameCycle()
    {
        //Debug.Log("DISPOSED");
        g_cts?.Cancel();

        _problemModel.Reset();
        _gameView.Reset();
        _lifeModel.Reset();

        rotQueue.Clear();

        InputHandler.Instance.ResetHandler();
    }

    private void GenerateCubeInfo()
    {
        CurCubeCycle = 0;
        cubeInfo = new CubeInfo(size, numOfDepth);
        _problemModel.Init(cubeInfo);
        _gameView.SetViewWithCubeInfo(cubeInfo);
    }

    public void OnDragInput(Vector2 st, Vector2 ed)
    {
        SoundManager.PlayEffect("swipe");
        rotQueue.Enqueue(DetermineAxisDir(st, ed));
    }
    public void SetNextProblem()
    {
        if (cubeInfo == null) return;
        _problemModel.GenerateNextProblem(cubeInfo.phaseNode);
        _gameView.ProblemMeshFilter.mesh = _problemModel.GetProblemMesh();
    }

    public void LifeEnd()
    {
        SoundManager.PlayEffect("clear");
        _gameView.SetEndGameRect(_problemModel.CurScore);
        TryUpdateScore(_problemModel.CurScore, size).Forget();
        DisposeGameCycle();
    }

    public void DecreaseLife()
    {
        _lifeModel.Life -= _lifeModel.GetDecreaseRate(totalGameTime) * Time.deltaTime;
    }

    public void JudgeAnswer()
    {
        var res = _problemModel.IsCorrect(cubeInfo);

        if(res)
        {
            SoundManager.PlayEffect("success");
        }
        else
        {
            SoundManager.PlayEffect("wrong");
        }

        _problemModel.ProcessJudge(res);
        _lifeModel.ProcessJudge(res);

        if(_lifeModel.Life <= 0)
        {
            return;
        }

        UpdateCubeCycle();
        SetNextProblem();
    }

    public void MoveToHome()
    {
        Debug.Log("MoveToHome");
        GameManager.Instance.ReturnHome();
    }

    private UniTaskVoid TryUpdateScore(int score, int diff) => NetworkManager.Instance.TryUpdateUserScore(NetworkManager.Instance.g_UID, diff, score);

    private async UniTask SerialRotation()
    {
        while (!g_cts.IsCancellationRequested)
        {
            await UniTask.WaitUntil(() => rotQueue.Count > 0, cancellationToken : g_cts.Token);

            if(rotQueue.TryDequeue(out var res))
            {
                cubeInfo.RotateCube(res);
                await _gameView.RotateCubeObject(res, RotateSpeed, g_cts);
                _gameView.ProjectionMeshFilter.mesh = MeshHandler.GetMeshFromQuadPoints(cubeInfo.SideProjectionSet());
            }
        }
    }

    private async UniTask Tick()
    {
        await UniTask.RunOnThreadPool(async () =>
        {
            while(!g_cts.IsCancellationRequested)
            {
                await UniTask.NextFrame();
                if (!isPaused)
                {
                    totalGameTime += Time.deltaTime;
                    OnTickChanged?.Invoke();
                }
            }
        }, false);
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

    private void UpdateCubeCycle()
    {
        CurCubeCycle += 1;
        if (CurCubeCycle < CubeRegenCycle) return;

        GenerateCubeInfo();
    }

    public void Dispose()
    {
        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GamePresenter))]
public class GamePresenterEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GamePresenter gamePresenter = (GamePresenter)target;

        GUILayout.Space(10);

        if(GUILayout.Button("Start Game Cycle"))
        {
            gamePresenter.StartGameCycle();
        }

        if (GUILayout.Button("Dispose Game Cycle"))
        {
            gamePresenter.DisposeGameCycle();
        }
    }
}
#endif
