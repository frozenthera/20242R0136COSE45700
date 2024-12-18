using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MeshHandler
{
    readonly static int[] dx = { 0, 1, 0, 1 };
    readonly static int[] dy = { 0, 0, 1, 1 };

    public static Mesh GetOutlineMeshFromQuadPoints(IEnumerable<Vector2Int> quads) => CreateOutlineMesh(GetMeshFromQuadPoints(quads));
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

            int index0 = vertices.IndexOf(points[0]);
            int index1 = vertices.IndexOf(points[1]);
            int index2 = vertices.IndexOf(points[2]);
            int index3 = vertices.IndexOf(points[3]);

            triangles.Add(index0);
            triangles.Add(index2);
            triangles.Add(index1);

            triangles.Add(index3);
            triangles.Add(index1);
            triangles.Add(index2);
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
    private static Mesh CreateOutlineMesh(Mesh mesh)
    {
        Dictionary<Edge, int> edgeDict = new Dictionary<Edge, int>();
        List<Vector3> outlineVertices = new List<Vector3>();
        List<int> outlineIndices = new List<int>();

        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        // Collect edges and count occurrences
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Edge[] edges = new Edge[]
            {
                new Edge(triangles[i], triangles[i + 1]),
                new Edge(triangles[i + 1], triangles[i + 2]),
                new Edge(triangles[i + 2], triangles[i])
            };

            foreach (var edge in edges)
            {
                if (edgeDict.ContainsKey(edge))
                    edgeDict[edge]++;
                else
                    edgeDict[edge] = 1;
            }
        }

        // Filter edges that are part of the outline (only appear once)
        foreach (var kvp in edgeDict)
        {
            if (kvp.Value == 1) // Edge is only connected to one face
            {
                outlineVertices.Add(vertices[kvp.Key.v1]);
                outlineVertices.Add(vertices[kvp.Key.v2]);
                outlineIndices.Add(outlineIndices.Count);
                outlineIndices.Add(outlineIndices.Count);
            }
        }

        // Create the outline mesh
        Mesh outlineMesh = new Mesh();
        outlineMesh.vertices = outlineVertices.ToArray();
        outlineMesh.SetIndices(outlineIndices.ToArray(), MeshTopology.Lines, 0);
        outlineMesh.RecalculateBounds();

        return outlineMesh;
    }

    private struct Edge
    {
        public int v1, v2;

        public Edge(int v1, int v2)
        {
            // Ensure consistent ordering for dictionary
            if (v1 < v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
            else
            {
                this.v1 = v2;
                this.v2 = v1;
            }
        }

        public override int GetHashCode()
        {
            return v1.GetHashCode() ^ v2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge)
            {
                Edge other = (Edge)obj;
                return (v1 == other.v1 && v2 == other.v2) || (v1 == other.v2 && v2 == other.v1);
            }
            return false;
        }
    }
}
