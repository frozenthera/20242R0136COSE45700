using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

public class ProblemModel
{
    private int minDepth;
    private int maxDepth;

    private int cubeSize;

    private int curProblemIdx;
    private uint[] problems;

    public Action<int> OnScoreChanged;
    private int curScore = 0;
    public int CurScore
    {
        get => curScore;
        private set
        {
            curScore = value;
            OnScoreChanged?.Invoke(curScore);
        }
    }

    public uint CurProblem { get; private set; }
    public ProblemModel(int minDepth = 1, int maxDepth = 3)
    {
        this.minDepth = minDepth;
        this.maxDepth = maxDepth;
    }

    public void Init(CubeInfo cube)
    {
        cubeSize = cube.size;
        GenerateProjection(cube);
    }

    public void Reset()
    {
        CurScore = 0;
        curProblemIdx = -1;
        CurProblem = 0;
    }

    readonly AxisDir[] TraverseOrder = {AxisDir.none, AxisDir.posZ, AxisDir.posX, AxisDir.posX, AxisDir.posX, AxisDir.posZ };
    private Dictionary<uint, List<int>> phaseDict;
    public void GenerateProjection(CubeInfo cube)
    {
        problems = new uint[24];
        phaseDict = new();

        var cur = cube;
        foreach (var axis in TraverseOrder)
        {
            cur.RotateCube(axis);
            for (int i = 0; i < 4; i++)
            {
                uint phase = cur.SideProjectionUint();
                if(!phaseDict.ContainsKey(phase))
                {
                    phaseDict[phase] = new List<int>();
                }
                phaseDict[phase].Add(cube.phaseNode);
                problems[cube.phaseNode] = phase;

                cur.RotateCube(AxisDir.posY);
            }
        }       

        ConstructDepthGraph();
        curProblemIdx = cube.phaseNode;
    }

    private Vector3Int RefVecRotation(Vector3Int value, AxisDir dir)
    {
        return dir switch
        {
            AxisDir.posX => new Vector3Int(value.x, -value.z, value.y),
            AxisDir.negX => new Vector3Int(value.x, value.z, -value.y),
            AxisDir.posY => new Vector3Int(value.z, value.y, -value.x),
            AxisDir.negY => new Vector3Int(-value.z, value.y, value.x),
            AxisDir.posZ => new Vector3Int(-value.y, value.x, value.z),
            AxisDir.negZ => new Vector3Int(value.y, -value.x, value.z),
            _ => new Vector3Int(value.x, value.y, value.z)
        };
    }


    private int[][] depthGraph;
    private List<(int, uint)>[] depthDict;
    private void ConstructDepthGraph()
    {
        depthGraph = new int[24][];
        depthDict = new List<(int, uint)>[24];

        for(int i = 0; i < 24; ++i)
        {
            depthGraph[i] = new int[24];
            for(int j = 0; j < 24; ++j)
            {
                if (i == j) depthGraph[i][j] = 0;
                else depthGraph[i][j] = 100;
            }
        }

        for(int i = 0; i < 24; ++i)
        {
            for (int j = 1; j < 7; ++j)
            {
                depthGraph[i][Util.PhaseFunction[i, j]] = 1;
            }
        }

        for(int k = 0; k < 24; ++k)
        {
            for(int i = 0; i < 24; ++i)
            {
                for(int j = 0; j < 24; ++j)
                {
                    depthGraph[i][j] = Mathf.Min(depthGraph[i][j], depthGraph[i][k] + depthGraph[k][j]);
                }
            }
        }

        for(int i = 0; i < 24; i++)
        {
            depthGraph[i][i] = 999;
        }

        for(int i = 0; i < 24; i++)
        {
            depthDict[i] = new();
            foreach (var pair in phaseDict)
            {
                uint phase = pair.Key;
                List<int> candidate = pair.Value;

                if (phase == problems[i]) continue;

                int minDist = 999;
                foreach(var item in candidate)
                {
                    minDist = Mathf.Min(minDist, depthGraph[i][item]);
                }
                depthDict[i].Add((minDist, phase));
            }
            depthDict[i].Sort();
        }
    }

    public void GenerateNextProblem(int prob_idx)
    {
        curProblemIdx = prob_idx;
        CurProblem = GetNextProblem();
    }

    int BinarySearchTuple(List<(int, uint)> list, int target)
    {
        int left = 0;
        int right = list.Count - 1;
        int resultIndex = list.Count;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (list[mid].Item1 == target)
            {
                resultIndex = mid;
                right = mid - 1;
            }
            else if (list[mid].Item1 < target)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return resultIndex;
    }

    private uint GetNextProblem()
    {
        //Debug.Log($"Current node : {curProblemIdx}");
        if(curProblemIdx != -1)
        {
            //Debug.Log($"{Convert.ToString(problems[curProblemIdx], 2)}");
            int st = BinarySearchTuple(depthDict[curProblemIdx], minDepth);
            int ed = BinarySearchTuple(depthDict[curProblemIdx], maxDepth + 1);

            int randIdx = Random.Range(st, ed);

            //Debug.Log($"start : {st}  end : {ed} rand : {randIdx} / {depthDict[curProblemIdx][randIdx].Item1}" +
                //$"{Convert.ToString(depthDict[curProblemIdx][randIdx].Item2, 2)} ");
            return depthDict[curProblemIdx][randIdx].Item2;
        }
        return problems[Random.Range(0, 24)];
    }

    public bool IsCorrect(CubeInfo cube)
    {
        return (problems[cube.phaseNode] ^ CurProblem) == 0;
    }

    public void ProcessJudge(bool isCorrect)
    {
        if(isCorrect)
        {
            CurScore += GetProblemScore();
        }
    }

    public int GetProblemScore()
    {
        return 5;
    }

    public Mesh GetProblemMesh() => MeshHandler.GetOutlineMeshFromQuadPoints(BitmaskToVec2Int(CurProblem, cubeSize));

    public static List<Vector2Int> BitmaskToVec2Int(uint flag, int size)
    {
        var res = new List<Vector2Int>();

        for (int i = 0; i < size * size; ++i)
        {
            if (flag % 2 == 1)
            {
                res.Add(new Vector2Int(i % size, i / size));
            }
            flag /= 2;
        }
        return res;
    }
}
