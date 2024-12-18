using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameView : MonoBehaviour
{
    [Header("pages")]
    [SerializeField] RectTransform IngameRect;
    [SerializeField] RectTransform EndgameRect;

    [Header("Ingame")]    
    [SerializeField] Button _captureButton;
    [SerializeField] Button _pauseButton;

    [SerializeField] GameObject cubePrefab;
    [SerializeField] GameObject wireframePrefab;
    [SerializeField] Transform CubeOriginPos;
    [Space(10)]
    [Header("Mesh Filters")]
    [SerializeField] MeshFilter problemMeshFilter;
    [SerializeField] MeshFilter projectionMeshFilter;
    public MeshFilter ProjectionMeshFilter => projectionMeshFilter;
    public MeshFilter ProblemMeshFilter => problemMeshFilter;

    [Space(10)]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI lifeText;
    [SerializeField] TextMeshProUGUI judgeText;

    [Space(10)]
    [SerializeField] Slider lifeSlider_left;
    [SerializeField] Slider lifeSlider_right;

    [Header("Endgame")]
    [SerializeField] TextMeshProUGUI endGameScoreText;
    [SerializeField] Button retryButton;
    [SerializeField] Button homeButton;

    public Button CaptureButton => _captureButton;
    public Button RetryButton => retryButton;
    public Button HomeButton => homeButton;
    private int maxLife;
    private GameObject cubeGameObject;
    List<Poolable> cubeObjects;
    public void SetMaxLife(int maxLife)
    {
        this.maxLife = maxLife;
    }

    public void SetLife(float cur)
    {
        lifeText.text = $"{((int)cur)} / {maxLife}";
        lifeSlider_right.value = cur / maxLife;
        lifeSlider_left.value = cur / maxLife;
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void Init(int MaxLife)
    {   
        cubeObjects = new();

        SetMaxLife(MaxLife);

        IngameRect.gameObject.SetActive(true);
        EndgameRect.gameObject.SetActive(false);
    }

    public void SetViewWithCubeInfo(CubeInfo cubeInfo)
    {
        if (cubeGameObject != null)
        {
            foreach (var item in cubeObjects)
            {
                item?.ReleaseObject();
            }
            cubeObjects.Clear();
            Destroy(cubeGameObject);
        }

        cubeGameObject = GenerateCubeGameObject(cubeInfo);
        cubeGameObject.transform.position = CubeOriginPos.position;
        projectionMeshFilter.mesh = MeshHandler.GetMeshFromQuadPoints(cubeInfo.SideProjectionSet());
        AliasObjectSize(cubeInfo.size);
    }

    public void SetEndGameRect(int score)
    {
        endGameScoreText.text = $"SCORE\n\n{score}";

        IngameRect.gameObject.SetActive(false);
        EndgameRect.gameObject.SetActive(true);
    }

    public void Reset()
    {
        problemMeshFilter.mesh = null;
        projectionMeshFilter.mesh = null;
        CaptureButton.onClick.RemoveAllListeners();

        foreach (var item in cubeObjects)
        {
            item?.ReleaseObject();
        }
        cubeObjects.Clear();
        Destroy(cubeGameObject);
    }

    public async UniTask RotateCubeObject(AxisDir dir, float RotateSpeed, CancellationTokenSource cts)
    {
        Quaternion fromRotation = cubeGameObject.transform.rotation;
        var axisValue = Util.AxisDirToValue(dir);
        float time = 0f;
        while (time * RotateSpeed < 1f)
        {
            if (cts.IsCancellationRequested || cubeGameObject == null) return;

            time += Time.deltaTime;
            cubeGameObject.transform.Rotate(axisValue, 90 * Time.deltaTime * RotateSpeed, Space.World);
            await UniTask.NextFrame();
        }
        cubeGameObject.transform.rotation = fromRotation * Quaternion.Inverse(fromRotation) * Quaternion.Euler(axisValue * 90f) * fromRotation;
    }

    private void AliasObjectSize(int cubeSize)
    {
        Vector3 aliasedSize = 2.1f / cubeSize * Vector3.one;
        cubeGameObject.transform.localScale = aliasedSize;
        projectionMeshFilter.transform.localScale = aliasedSize;
        problemMeshFilter.transform.localScale = aliasedSize;
    }

    public GameObject GenerateCubeGameObject(CubeInfo cube)
    {
        GameObject res = new GameObject();
        Vector3 offset = Util.GetCubeOffset(cube.size);
        foreach (var item in cube.cubes)
        {
            Vector3 vec = item - offset;
            var go = ObjectPool.Instance.GetGO("Cube");
            go.transform.parent = res.transform;
            go.transform.position = vec;
            cubeObjects.Add(go.GetComponent<Poolable>());
            //Instantiate(cubePrefab, vec, Quaternion.identity, res.transform);
        }
        GenerateWireframeCubeObject(res, cube);
        return res;
    }

    public void GenerateWireframeCubeObject(GameObject root, CubeInfo cube)
    {
        var go = Instantiate(wireframePrefab, Vector3.zero, Quaternion.identity, root.transform);
        go.transform.localScale = Vector3.one * (cube.size + 0.02f);
        return;
    }

    //private async UniTask FadeJudgeText(CancellationToken token)
    //{
    //    float alpha = 1f;
    //    while(alpha > 0f && !token.IsCancellationRequested)
    //    {
    //        await UniTask.Yield();
    //        alpha -= judgeFadeTime * Time.deltaTime;
    //        judgeText.alpha = alpha;
    //    }
    //}
}
