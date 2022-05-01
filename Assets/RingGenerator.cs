using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RingGenerator : MonoBehaviour
{
    public enum UvProjection 
    {
        AngularRadial,
        ProjectZ
    }
    
    [SerializeField, Range(0.01f, 2f)] float radiusInner = 1f;
    [SerializeField, Range(0.01f, 2f)] float thickness = 0.25f;
    [SerializeField, Range(3, 256)] int angularSegments = 32;
    [SerializeField] UvProjection uvProjection = UvProjection.AngularRadial;

    float RadiusOuter => radiusInner + thickness;
    float tau => Mathf.PI * 2;

    Mesh mesh;
    List<Vector3> points;
    List<int> triangles;
    List<Vector2> uvs;
    List<Vector3> normals;

    float AngleFromSegment(int segment, int segmentCount) => tau * (segment / (float)segmentCount);

    Vector2 GetUnitVectorByAngle(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

    void DrawWireCircle(Vector3 pos, Quaternion rot, float radius, int detail)
    {
        for (int i = 0; i < detail; i += 1)
        {
            Vector3 pointFrom = GetUnitVectorByAngle(AngleFromSegment(i, detail)) * radius;
            Vector3 pointTo = GetUnitVectorByAngle(AngleFromSegment((i + 1) % detail, detail))  * radius;
            Gizmos.DrawLine(pos + rot * pointFrom, pos + rot * pointTo);
        }
    }

    void OnDrawGizmosSelected() 
    {
        DrawWireCircle(transform.position, transform.rotation, radiusInner, angularSegments);
        DrawWireCircle(transform.position, transform.rotation, RadiusOuter, angularSegments);
    }

    void Awake() 
    {
        mesh = new Mesh();
        mesh.name = "Procedural Ring";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void Update() 
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        mesh.Clear();

        List<Vector3> outerVertices = new List<Vector3>();
        List<Vector3> innerVertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        List<Vector2> outerUVs = new List<Vector2>();
        List<Vector2> innerUVs = new List<Vector2>();

        // the first vertex is split to fix uvs so [0] and [angularSegments] are the same
        int innerOffset = angularSegments + 1; 

        for (int i = 0; i < angularSegments + 1; i += 1)
        {
            Vector2 unitVector = GetUnitVectorByAngle(AngleFromSegment(i % angularSegments, angularSegments));
            outerVertices.Add(unitVector * RadiusOuter);
            innerVertices.Add(unitVector * radiusInner);

            if (i < angularSegments)
            {
                // triangle vertices are specified in left hand order (clockwise)
                //
                //  (a)------(b) - outer ring
                //   |       /|
                //   |      / |
                //   |     /  |
                //   |    /   |    trianlge1 = a b c
                //   |   /    |    triangle2 = c b d
                //   |  /     |
                //   | /      | 
                //   |/       | 
                //  (c)------(d) - innner ring
                //
                int a = i, b = i + 1;
                int c = i + innerOffset, d = i + 1 + innerOffset;

                triangles.Add(a); triangles.Add(b); triangles.Add(c); 
                triangles.Add(c); triangles.Add(b); triangles.Add(d); 
            }

            normals.Add(new Vector3(0, 0, 1));
            normals.Add(new Vector3(0, 0, 1));

            switch (uvProjection)
            {
            case UvProjection.AngularRadial:
                float t = i / (float)angularSegments;
                outerUVs.Add(new Vector2(t,1));
                innerUVs.Add(new Vector2(t,0));
                break;
            case UvProjection.ProjectZ:
                // unitVector ranges from -1,-1 to 1,1 so math it to 0,0 to 1,1
                outerUVs.Add(unitVector * 0.5f + Vector2.one * 0.5f);
                innerUVs.Add(unitVector * (radiusInner / RadiusOuter) * 0.5f + Vector2.one * 0.5f);
                break;
            }
        }
        points = outerVertices;
        points.AddRange(innerVertices);

        uvs = outerUVs;
        uvs.AddRange(innerUVs);

        mesh.SetVertices(points);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        // mesh.RecalculateNormals();
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
    }
}
