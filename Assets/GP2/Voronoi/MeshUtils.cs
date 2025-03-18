using UnityEngine;
using System.Collections.Generic;

public static class MeshUtils
{
    public static Mesh ExtrudePolygon(List<Vector2> vertices, float height)
    {
        Mesh mesh = new Mesh();

        int vertexCount = vertices.Count;
        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();

        // Create separate vertices for each face to prevent shared normals
        for (int i = 0; i < vertexCount; i++)
        {
            Vector2 v = vertices[i];
            meshVertices.Add(new Vector3(v.x, 0, v.y));        // Bottom vertex
            meshVertices.Add(new Vector3(v.x, height, v.y));   // Top vertex
            meshVertices.Add(new Vector3(v.x, 0, v.y));        // Unwelded Bottom vertex
            meshVertices.Add(new Vector3(v.x, height, v.y));   // Unwelded Top vertex
        }

        // Create side faces
        for (int i = 0; i < vertexCount; i++)
        {
            int next = (i + 1) % vertexCount; // Wrap around to create a closed shape

            int bottomA = i * 4;
            int topA = bottomA + 1;
            int bottomB = next * 4;
            int topB = bottomB + 1;

            int bottomA_unwelded = bottomA + 2;
            int topA_unwelded = topA + 2;
            int bottomB_unwelded = bottomB + 2;
            int topB_unwelded = topB + 2;

            // First triangle (bottomA, topA, topB)
            meshTriangles.Add(bottomA_unwelded);
            meshTriangles.Add(topA_unwelded);
            meshTriangles.Add(topB_unwelded);

            // Second triangle (bottomA, topB, bottomB)
            meshTriangles.Add(bottomA_unwelded);
            meshTriangles.Add(topB_unwelded);
            meshTriangles.Add(bottomB_unwelded);
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
