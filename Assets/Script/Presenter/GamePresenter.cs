using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GamePresenter : SingletonBehaviour<GamePresenter>
{
    CubeGenerator _cubeGenerator;
    ProblemModel _problemModel;
    LifeModel _lifeModel;

    GameView _gameView => GameView.Instance;

    [Header("Properties")]
    [SerializeField] Transform CubeOriginPos;
    [SerializeField] GameObject CubePrefab;
    [SerializeField] float RotateSpeed = 3f;
    [SerializeField] int MaximumLife = 100;
    [Range(3, 5)] [SerializeField] int size = 3;
    [Tooltip("1 ~ size^3 À¸·Î ClampµÊ")]
    [SerializeField] int numOfDepth = 5;

    [Space(10)]
    [Header("Mesh Filters")]
    [SerializeField] MeshFilter problemMeshFilter;
    [SerializeField] MeshFilter projectionMeshFilter;

    public event Action<bool> OnJudge;
    public event Action<int, int> OnLifeChange;
    public event Action OnTickChanged;

    CubeInfo cubeInfo;
    GameObject cubeGameObject;
    Queue<AxisDir> rotQueue = new();

    float totalGameTime = 0;
    [SerializeField] bool isPaused = false;

    CancellationTokenSource g_cts;
    private void Start()
    {
        _lifeModel = new LifeModel(MaximumLife);
        _problemModel = new ProblemModel();
        _cubeGenerator = new CubeGenerator(CubePrefab);

        OnJudge += _problemModel.ProcessJudge;
        OnJudge += _lifeModel.ProcessJudge;
        
        _lifeModel.OnLifeChanged += _gameView.SetLife;
        _lifeModel.OnLifeChanged += OnLifeEnd;

        _problemModel.OnScoreChanged += _gameView.SetScore;

        _gameView.RetryButton.onClick.AddListener(StartGameCycle);
        _gameView.HomeButton.onClick.AddListener(MoveToHome);

        _gameView.SetMaxLife(_lifeModel.MaxLife);
    }

    [ContextMenu("Start Game Cycle")]
    public void StartGameCycle()
    {
        _gameView.SetGame(false);

        g_cts = new CancellationTokenSource();

        cubeInfo = _cubeGenerator.GenerateCube(size, numOfDepth);
        _problemModel.Init(cubeInfo);

        //problemMeshFilter.transform.position = -new Vector3(1, 0, 1) * ((float)cubeInfo.size / 2);
        //projectionMeshFilter.transform.position = -new Vector3(1, 0, 1) * ((float)cubeInfo.size / 2);

        cubeGameObject = _cubeGenerator.GenerateCubeGameObject(cubeInfo);
        cubeGameObject.transform.position = CubeOriginPos.position;

        _gameView.CaptureButton.onClick.AddListener(JudgeAnswer);

        InputHandler.Instance.OnDragEnd += OnDragInput;

        SetNextProblem();
        projectionMeshFilter.mesh = MeshHandler.GetMeshFromQuadPoints(cubeInfo.SideProjectionSet());

        OnTickChanged += DecreaseLife;

        SerialRotation().Forget();
        Tick().Forget();
    }

    [ContextMenu("End Game Cycle")]
    public void DisposeGameCycle()
    {
        g_cts?.Cancel();

        _lifeModel.Reset();
        _problemModel.Reset();

        rotQueue.Clear();
        totalGameTime = 0;
        problemMeshFilter.mesh = null;
        projectionMeshFilter.mesh = null;

        _gameView.CaptureButton.onClick.RemoveAllListeners();

        OnTickChanged -= DecreaseLife;
        InputHandler.Instance.OnDragEnd -= OnDragInput;

        Destroy(cubeGameObject);
    }

    public void OnDragInput(Vector2 st, Vector2 ed) => rotQueue.Enqueue(DetermineAxisDir(st, ed));

    public void SetNextProblem()
    {
        _problemModel.GenerateNextProblem();
        problemMeshFilter.mesh = _problemModel.GetProblemMesh();
    }

    public void OnLifeEnd(float _life)
    {
        if (_life > 0) return;

        _gameView.SetGame(true);
        _gameView.SetEndGameRect(_problemModel.CurScore);
        DisposeGameCycle();
    }

    public void DecreaseLife()
    {
        _lifeModel.Life -= _lifeModel.GetDecreaseRate(totalGameTime) * Time.deltaTime;
    }

    public void JudgeAnswer()
    {
        var res = _problemModel.IsCorrect(cubeInfo);
        OnJudge?.Invoke(res);

        SetNextProblem();
    }

    public void MoveToHome()
    {
        Debug.Log("Move to Home");
    }

    private async UniTask RotateCubeObject(AxisDir dir)
    {
        Quaternion fromRotation = cubeGameObject.transform.rotation;

        var axisValue = Util.AxisDirToValue(dir);
        float time = 0f;
        while(time * RotateSpeed < 1f)
        {
            time += Time.deltaTime;
            cubeGameObject.transform.Rotate(axisValue, 90 * Time.deltaTime * RotateSpeed, Space.World);
            await UniTask.NextFrame(cancellationToken : g_cts.Token);
        }
        cubeGameObject.transform.rotation = fromRotation * Quaternion.Inverse(fromRotation) * Quaternion.Euler(axisValue * 90f) * fromRotation;
        projectionMeshFilter.mesh = MeshHandler.GetMeshFromQuadPoints(cubeInfo.SideProjectionSet());
    }

    private async UniTask SerialRotation()
    {
        while (!g_cts.IsCancellationRequested)
        {
            await UniTask.WaitUntil(() => rotQueue.Count > 0, cancellationToken : g_cts.Token);

            if(rotQueue.TryDequeue(out var res))
            {
                cubeInfo.RotateCube(res);
                await RotateCubeObject(res);
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
}
