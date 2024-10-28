using UnityEngine;

public static class Util
{
    public static readonly int[,] PhaseFunction = new int[24, 7]
    {
        //yielded {self, +x, +y, +z, -x, -y, -z} rotation result node
        {0, 9,  1,  4, 19, 3, 22},
        {1, 5, 2,  16, 15, 0,21 },
        {2, 17, 3,  12, 11, 1, 20 },
        {3, 13,  0,  8, 7, 2, 23 },

        {4, 8, 5,  21, 16, 7, 15 },
        {5, 22, 6,  17, 1, 4, 14},
        {6, 18,  7,  2, 10, 5, 13 },
        {7, 3, 4,  11, 20, 6, 12 },

        {8, 12, 9,  22, 4, 11, 19 },
        {9, 23,  10, 5, 0, 8, 18 },
        {10, 6,  11, 1, 14, 9, 17},
        {11, 2, 8,  15, 21, 10, 16},

        {12, 16, 13, 23, 8, 15, 7},
        {13, 20,  14, 9, 3, 12, 6},
        {14, 10,  15, 0, 18, 13, 5},
        {15, 1, 12, 19, 22, 14, 4},

        {16, 4, 17, 20, 12, 19, 11},
        {17, 21, 18, 13, 2, 16, 10},
        {18, 14,  19, 3, 6, 17, 9},
        {19, 0,  16, 7, 23, 18, 8},

        {20, 7, 21, 10, 13, 23, 2},
        {21, 11, 22, 14, 17, 20, 1},
        {22, 15, 23, 18, 5, 21, 0},
        {23, 19,  20, 6, 9, 22, 3},
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