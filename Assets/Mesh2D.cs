using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Mesh2D : ScriptableObject
{
    [System.Serializable]
    public class Vertex
    {
        public Vector2 point;
        public Vector2 normal;
        public float u;
    }

    public List<Vertex> vertices;
    public List<int> lineIndices;

    public int VertexCount => vertices.Count;
    public int LineCount => lineIndices.Count;

    public float CalculateUSpan()
    {
        float distance = 0f;
        for (int i = 0; i < LineCount; i += 2)
        {
            Vector2 a = vertices[lineIndices[i]].point;
            Vector2 b = vertices[lineIndices[i + 1]].point;
            distance += Vector2.Distance(a, b);
        }
        return distance;
    }
}
