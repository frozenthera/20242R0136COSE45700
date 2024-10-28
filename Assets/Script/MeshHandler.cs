using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshHandler
{
    readonly static int[] dx = { 0, 1, 0, 1 };
    readonly static int[] dy = { 0, 0, 1, 1 };
    public static Mesh GetMeshFromQuadPoints(IEnumerable<Vector2Int> quads)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3[] points = new Vector3[4];
        foreach (var square in quads)
        {
            var x = square.x;
            var y = square.y;

            for (int i = 0; i < 4; i++)
            {
                points[i] = new Vector3(x + dx[i], 0, y + dy[i]);
                if (!vertices.Contains(points[i]))
                {
                    vertices.Add(points[i]);
                }
            }

            int index1 = vertices.IndexOf(points[0]);
            int index2 = vertices.IndexOf(points[1]);
            int index3 = vertices.IndexOf(points[2]);
            int index4 = vertices.IndexOf(points[3]);

            triangles.Add(index1);
            triangles.Add(index3);
            triangles.Add(index2);

            triangles.Add(index2);
            triangles.Add(index3);
            triangles.Add(index4);
        }

        return CreateMesh(vertices, triangles);
    }

    public static Mesh CreateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh CombineMesh(Mesh m1, Mesh m2)
    {
        int m1VertCnt = m1.vertices.Length;
        for(int i = 0; i < m2.triangles.Length; ++i)
        {
            m2.triangles[i] += m1VertCnt;
        }

        m1.vertices = m1.vertices.Concat(m2.vertices).ToArray();

        m1.subMeshCount = 2;
        m1.SetTriangles(m1.triangles, 0);
        m1.SetTriangles(m2.triangles, 1);

        m1.RecalculateNormals();

        return m1;
    }
}
