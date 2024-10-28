using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CubeInfo
{
    public int size { get; private set; }
    public List<Vector3Int> cubes { get; private set; }
    public int phaseNode { get; private set; }

    public CubeInfo(int size)
    {
        this.size = size;
        cubes = new List<Vector3Int>();
        phaseNode = 0;
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
        var offset = Vector3.one * (size / 2);
        foreach (var item in cubes)
        {
            Vector3 vec = item - offset;
            vec = Quaternion.AngleAxis(90, refValue) * vec;
            vec += offset * 1.01f;
            temp.Add(Vector3Int.FloorToInt(vec));
        }
        cubes = temp;

        phaseNode = Util.PhaseFunction[phaseNode, Util.RotatePhaseNode(rotDir)];
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
}