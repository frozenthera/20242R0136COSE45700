using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HomeView : MonoBehaviour, IDisposable
{
    public Button CustomButton => customButton;
    public Button RankingButton => rankingButton;
    public Button SettingButton => settingButton;

    [SerializeField] Transform CubeOriginPos;
    [Space(10)]
    [Header("Mesh Filters")]
    [SerializeField] MeshFilter projectionMeshFilter;
    public MeshFilter ProjectionMeshFilter => projectionMeshFilter;
    private GameObject cubeGameObject;

    [SerializeField] TextMeshProUGUI highScoreText;

    [SerializeField] Button customButton;
    [SerializeField] Button rankingButton;
    [SerializeField] Button settingButton;

    List<Poolable> cubeObjects;
    public void Init(CubeInfo cubeInfo)
    {
        cubeObjects = new List<Poolable>();
        SetViewWithCubeInfo(cubeInfo);
    }

    public void OnClose()
    {
        //gameObject.SetActive(false);
    }

    public void OnOpen()
    {
        gameObject.SetActive(true);
    }

    public void Dispose()
    {
        customButton.onClick.RemoveAllListeners();
        rankingButton.onClick.RemoveAllListeners();
        settingButton.onClick.RemoveAllListeners();

        projectionMeshFilter.mesh = null;

        foreach(var cube in cubeObjects)
        {
            cube?.ReleaseObject();
        }
    }

    public void SetHighScore(int score, bool err = false)
    {
        if (err)
        {
            highScoreText.text = "-";
            return;
        }
        highScoreText.text = score.ToString();
    }

    public async UniTask RotateCubeObject(AxisDir dir, float RotateSpeed, CancellationTokenSource cts)
    {
        Quaternion fromRotation = cubeGameObject.transform.rotation;
        var axisValue = Util.AxisDirToValue(dir);
        float time = 0f;
        while (time * RotateSpeed < 1f)
        {
            if (cubeGameObject == null || cts.Token.IsCancellationRequested) break;
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
        return res;
    }

    public void SetViewWithCubeInfo(CubeInfo cubeInfo)
    {
        if (cubeGameObject != null) Destroy(cubeGameObject);

        cubeGameObject = GenerateCubeGameObject(cubeInfo);
        cubeGameObject.transform.position = CubeOriginPos.position;
        projectionMeshFilter.mesh = MeshHandler.GetMeshFromQuadPoints(cubeInfo.SideProjectionSet());
        AliasObjectSize(cubeInfo.size);
    }
}
