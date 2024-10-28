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
    private List<uint> problems;

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

    public uint CurProblem => problems[curProblemIdx];
    public ProblemModel(int minDepth = 1, int maxDepth = 5)
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
    }

    readonly AxisDir[] TraverseOrder = {AxisDir.none, AxisDir.posZ, AxisDir.posX, AxisDir.posX, AxisDir.posX, AxisDir.posZ };
    public void GenerateProjection(CubeInfo cube)
    {
        problems = new List<uint>();

        var cur = cube;
        foreach (var axis in TraverseOrder)
        {
            cur.RotateCube(axis);
            for (int i = 0; i < 4; i++)
            {
                problems.Add(cur.SideProjectionUint());
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
    private void ConstructDepthGraph()
    {
        depthGraph = new int[24][];
        
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
    }

    public void GenerateNextProblem()
    {
        curProblemIdx = GetNextProblemIdx();
    }

    private int GetNextProblemIdx(int depth = 0)
    {
        Debug.Log($"Current node : {curProblemIdx}");
        if(depth == 0 && curProblemIdx != -1)
        {
            var candidates = new List<int>();
            
            for(int i = 0; i < 24; ++i)
            {
                if (minDepth <= depthGraph[curProblemIdx][i] && depthGraph[curProblemIdx][i] <= maxDepth)
                {
                    candidates.Add(i);
                    //Debug.Log($"{i} : Phase : {Convert.ToString(problems[i], 2)}");
                }   
            }
            int randIdx = Random.Range(0, candidates.Count);
            Debug.Log($"Next node : {candidates[randIdx]} Depth : {depthGraph[curProblemIdx][candidates[randIdx]]}");
            Debug.Log($"FLAG {Convert.ToString(problems[candidates[randIdx]], 2)}");
            return candidates[randIdx];
        }
        return Random.Range(0, 24);
    }

    public bool IsCorrect(CubeInfo cube)
    {
        return (cube.SideProjectionUint() ^ problems[curProblemIdx]) == 0;
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

    public Mesh GetProblemMesh() => MeshHandler.GetMeshFromQuadPoints(BitmaskToVec2Int(CurProblem, cubeSize));

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
