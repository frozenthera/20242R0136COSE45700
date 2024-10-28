using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubeGenerator
{
    GameObject cubePrefab;
    private int allowedMinLen;
    private int allowedMaxLen;

    public CubeGenerator(GameObject cubePrefab) : this(3, 5, cubePrefab) { }
       
    public CubeGenerator(int _allowedMinimum, int _allowedMaximum, GameObject cubePrefab)
    {
        allowedMinLen = _allowedMinimum;
        allowedMaxLen = _allowedMaximum;
        this.cubePrefab = cubePrefab;
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
    public CubeInfo GenerateCube(int size, int genNum = 1)
    {
        var res = new CubeInfo(size);
        var st = new Stack<Vector3Int>();
        var posHasCube = new Dictionary<Vector3Int, bool>();

        genNum = Mathf.Clamp(genNum, 1, size * size * size);
        size = Mathf.Clamp(size, allowedMinLen, allowedMaxLen);

        var startPos = Vector3Int.right * Random.Range(0, size)
                        + Vector3Int.up * Random.Range(0, size)
                        + Vector3Int.forward * Random.Range(0, size);

        if (genNum > 0)
        {
            res.cubes.Add(startPos);
            st.Push(startPos);
            posHasCube[startPos] = true;
        }
        for (int i = 1; i < genNum; ++i)
        {
            if(st.TryPeek(out var cur))
            {
                var next = GetNextCube(cur, size, posHasCube);
                if(next.x < 0)
                {
                    st.Pop();
                    i--;
                }
                else
                {
                    res.cubes.Add(next);
                    st.Push(next);
                    posHasCube[next] = true;
                }
            }
            else
            {
                break;    
            }
        }
        return res;
    }

    private int[] dx = { 1, 0, 0, -1, 0, 0 };
    private int[] dy = { 0, 1, 0, 0, -1, 0 };
    private int[] dz = { 0, 0, 1, 0, 0, -1 };
    private Vector3Int GetNextCube(Vector3Int cur, int size, Dictionary<Vector3Int, bool> posHasCube)
    {
        List<Vector3Int> availableMove = new();
        for(int i = 0; i <  dx.Length; ++i)
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

    public GameObject GenerateCubeGameObject(CubeInfo cube)
    {
        GameObject res = new GameObject();
        Vector3 offset = Vector3.one * (cube.size / 2);
        foreach (var item in cube.cubes)
        {
            Vector3 vec = item - offset;
            Object.Instantiate(cubePrefab, vec, Quaternion.identity, res.transform);
        }
        res.transform.localScale = Vector3.one * 0.6f;
        return res;
    }
}
