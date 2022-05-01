using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class RoadSegment : MonoBehaviour
{
    [SerializeField] Mesh2D shape2D;
    [SerializeField, Range(1, 32)] int segmentCount = 8;
    [SerializeField, Range(0, 1)] float tTest = 0;
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    Vector3 GetPos(int i)
    {
        return (i == 0) ? startPoint.position :
               (i == 1) ? startPoint.TransformPoint(Vector3.forward * startPoint.localScale.z) :
               (i == 2) ? endPoint.TransformPoint(Vector3.back * endPoint.localScale.z) :
             /*(i == 3)*/ endPoint.position;
    }

    Mesh mesh;

    OrientedPoint GetBezierPoint(float t)
    {
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 position = Vector3.Lerp(d, e, t);
        Vector3 tangent = (e-d).normalized;
        Vector3 up = Vector3.Lerp(startPoint.up, endPoint.up, t).normalized;

        Quaternion rot = Quaternion.LookRotation(tangent, up);

        return new OrientedPoint(position, rot);
    }

    float GetApproximateBezierLength(int precision = 16)
    {
        float distance = 0f;
        float t = 0;
        int step = 0;
        Vector3 pt1 = GetBezierPoint(t).position;
        while (t < 1f)
        {
            step += 1; 
            t = step / precision;
            Vector3 pt2 = GetBezierPoint(t).position;
            distance += Vector3.Distance(pt1, pt2);
            pt1 = pt2;
        }
        return distance;
    }

    void Awake() 
    {
        mesh = new Mesh();
        mesh.name = "Segment";

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void Update() => GenerateMesh();

    void GenerateMesh()
    {
        mesh.Clear();

        float uSpan = shape2D.CalculateUSpan() * 0.1f;
        float vLength = GetApproximateBezierLength();
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int s = 0; s < segmentCount + 1; s += 1)
        {
            float segmentSize = 1.0f / segmentCount;
            float t = s * segmentSize;
            OrientedPoint op = GetBezierPoint(t);
            for (int i = 0; i < shape2D.VertexCount; i += 1)
            {
                verts.Add(op.LocalToWorldPosition(shape2D.vertices[i].point * 0.1f));
                normals.Add(op.LocalToWorldVertex(shape2D.vertices[i].normal));
                Vector2 uv = new Vector2(shape2D.vertices[i].u, t * vLength / uSpan);
                uvs.Add(uv);
            }
        }

        List<int> triangles = new List<int>();

        for (int s = 0; s < segmentCount; s += 1)
        {
            int rootIndex = s * shape2D.VertexCount;
            int rootIndexNext = (s + 1) * shape2D.VertexCount;
            
            for (int i = 0; i < shape2D.LineCount; i += 2)
            {
                int a = rootIndex + shape2D.lineIndices[i];
                int b = rootIndex + shape2D.lineIndices[i+1];
                int c = rootIndexNext + shape2D.lineIndices[i];
                int d = rootIndexNext + shape2D.lineIndices[i+1];
                triangles.Add(a); triangles.Add(c); triangles.Add(d);
                triangles.Add(a); triangles.Add(d); triangles.Add(b);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
    }

    void OnDrawGizmos() 
    {
        Handles.DrawBezier(
            GetPos(0),GetPos(3),
            GetPos(1),GetPos(2),
            Color.white,
            EditorGUIUtility.whiteTexture, 
            1f);
        
        Gizmos.color = Color.red;
        OrientedPoint test = GetBezierPoint(tTest);
        Gizmos.DrawSphere(test.position, 0.01f);
        Handles.PositionHandle(test.position, test.rotation);

        Gizmos.color = Color.cyan;
        Vector3[] verts = shape2D.vertices.Select(v => test.LocalToWorldPosition(v.point * 0.1f)).ToArray();
        for (int i = 0; i < shape2D.lineIndices.Count; i += 2)
        {
            Vector3 a = verts[shape2D.lineIndices[i]];
            Vector3 b = verts[shape2D.lineIndices[i+1]];
            Gizmos.DrawLine(a, b);
            //Gizmos.DrawSphere(test.LocalToWorld(shape2D.vertices[i].point * 0.1f), 0.01f);
        }
        Gizmos.color = Color.white;
    }
}

