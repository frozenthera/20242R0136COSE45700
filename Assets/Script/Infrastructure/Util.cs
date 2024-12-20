using UnityEngine;

public static class Util
{
    public static readonly int[,] PhaseFunction = new int[24, 7]
    {
        //yielded {self, +x, +y, +z, -x, -y, -z} rotation result node
        {0, 9,  1,  4, 19, 3, 14},
        {1, 5, 2,  16, 15, 0, 10}, 
        {2, 17, 3,  12, 11, 1, 6}, 
        {3, 13,  0,  8, 7, 2, 18}, 

        {4, 8, 5,  21, 16, 7, 0}, 
        {5, 22, 6, 17, 1, 4, 9},
        {6, 18, 7, 2, 10, 5, 23},
        {7, 3, 4, 11, 20, 6, 19},

        {8, 12, 9, 22, 4, 11,3},
        {9, 23, 10, 5, 0, 8, 13},
        {10, 6, 11, 1, 14, 9, 20},
        {11, 2, 8, 15, 21, 10,7},

        {12, 16, 13, 23, 8, 15, 2},
        {13, 20, 14, 9, 3, 12, 17},
        {14, 10, 15, 0, 18, 13, 21},
        {15, 1, 12, 19, 22, 14, 11},

        {16, 4, 17, 20, 12, 19, 1},
        {17, 21, 18, 13, 2, 16, 5},
        {18, 14, 19, 3, 6, 17, 22},
        {19, 0, 16, 7, 23, 18, 15},

        {20, 7, 21, 10, 13, 23, 16},
        {21, 11, 22, 14, 17, 20, 4},
        {22, 15, 23, 18, 5, 21, 8},
        {23, 19, 20, 6, 9, 22, 12},
    };

    public static int RotatePhaseNode(AxisDir rotDir)
    {
        return rotDir switch
        {
            AxisDir.posX => 1,
            AxisDir.posY => 2,
            AxisDir.posZ => 3,
            AxisDir.negX => 4,
            AxisDir.negY => 5,
            AxisDir.negZ => 6,
            _ => 0
        };
    }

    public static Vector3 AxisDirToValue(AxisDir dir)
    {
        return dir switch
        {
            AxisDir.posX => Vector3.right,
            AxisDir.negX => Vector3.left,
            AxisDir.posY => Vector3.up,
            AxisDir.negY => Vector3.down,
            AxisDir.posZ => Vector3.forward,
            AxisDir.negZ => Vector3.back,
            _ => Vector3.zero,
        };
    }

    public static Vector3 GetCubeOffset(int size)
    {
        return Vector3.one * ((float)(size - 1) / 2);
    }
}

public enum AxisDir
{
    none,
    posX,
    negX,
    posY,
    negY,
    posZ,
    negZ
}