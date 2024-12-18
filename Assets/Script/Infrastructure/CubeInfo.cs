using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CubeInfo
{
    public int size { get; private set; }
    public List<Vector3Int> cubes { get; private set; }
    public int phaseNode { get; private set; }
    public int numOfCube { get; private set; }

    public CubeInfo(int size, int numOfCube)
    {
        this.size = size;
        this.numOfCube = numOfCube;
        cubes = new List<Vector3Int>();
        phaseNode = 0;
        GenerateCube();
    }

    public CubeInfo(int size, BitArray flag)
    {
        this.size = size;
        phaseNode = 0;
        GenerateCubeFromFlag(flag);
        this.numOfCube = cubes.Count;
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var item in cubes)
        {
            stringBuilder.Append(item.ToString());
            stringBuilder.Append('\t');
        }
        return stringBuilder.ToString();
    }

    public CubeInfo RotateCube(AxisDir rotDir = AxisDir.none)
    {
        var temp = new List<Vector3Int>();
        var refValue = Util.AxisDirToValue(rotDir);
        var offset = Util.GetCubeOffset(size);
        foreach (var item in cubes)
        {
            Vector3 vec = item - offset;
            vec = Quaternion.AngleAxis(90, refValue) * vec;
            vec += offset * 1.01f;
            temp.Add(Vector3Int.FloorToInt(vec));
        }
        cubes = temp;

        phaseNode = Util.PhaseFunction[phaseNode, Util.RotatePhaseNode(rotDir)];
        //Debug.Log($"{rotDir} {phaseNode}");
        return this;
    }

    public uint SideProjectionUint()
    {
        uint bitFlag = 0;
        foreach (var item in cubes)
        {
            bitFlag |= (uint)1 << (item.z * size + item.x);
        }
        return bitFlag;
    }

    public HashSet<Vector2Int> SideProjectionSet()
    {
        var res = new HashSet<Vector2Int>();
        foreach (var item in cubes)
        {
            res.Add(new Vector2Int(item.x, item.z));
        }
        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="size">Cubic Length</param>
    /// <param name="genNum">Number of cubes you want to generate</param>
    /// <returns>
    /// List of cube coordinate<br>
    /// Returns can't be directly rotated by Rotation matrix
    /// </returns>
    public void GenerateCube()
    {
        cubes = new();
        var st = new Stack<Vector3Int>();
        var posHasCube = new Dictionary<Vector3Int, bool>();

        var genNum = Mathf.Clamp(numOfCube, 1, size * size * size);
        size = Mathf.Clamp(size, 3, 5);

        var startPos = Vector3Int.right * Random.Range(0, size)
                        + Vector3Int.up * Random.Range(0, size)
                        + Vector3Int.forward * Random.Range(0, size);

        if (genNum > 0)
        {
            cubes.Add(startPos);
            st.Push(startPos);
            posHasCube[startPos] = true;
        }
        for (int i = 1; i < genNum; ++i)
        {
            if (st.TryPeek(out var cur))
            {
                var next = GetNextCube(cur, size, posHasCube);
                if (next.x < 0)
                {
                    st.Pop();
                    i--;
                }
                else
                {
                    cubes.Add(next);
                    st.Push(next);
                    posHasCube[next] = true;
                }
            }
            else
            {
                break;
            }
        }
    }

    private int[] dx = { 1, 0, 0, -1, 0, 0 };
    private int[] dy = { 0, 1, 0, 0, -1, 0 };
    private int[] dz = { 0, 0, 1, 0, 0, -1 };
    private Vector3Int GetNextCube(Vector3Int cur, int size, Dictionary<Vector3Int, bool> posHasCube)
    {
        List<Vector3Int> availableMove = new();
        for (int i = 0; i < dx.Length; ++i)
        {
            var next = new Vector3Int(cur.x + dx[i], cur.y + dy[i], cur.z + dz[i]);
            if (posHasCube.ContainsKey(next)) continue;
            if (next.x < 0 || next.y < 0 || next.z < 0 || next.x >= size || next.y >= size || next.z >= size) continue;

            availableMove.Add(next);
        }
        return availableMove.Count > 0
                ? availableMove[Random.Range(0, availableMove.Count)]
                : -Vector3Int.one;
    }

    public void GenerateCubeFromFlag(BitArray flag)
    {
        cubes = new();
        for(int i = 0; i < flag.Length; i++)
        {
            if (flag[i]) cubes.Add(new Vector3Int(i % 5, i / 25,  i % 25 / 5));
        }
    }
}