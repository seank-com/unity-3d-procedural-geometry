using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadGenerator : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> points;
    List<int> triangles;
    List<Vector2> uvs;
    List<Vector3> normals;

    void Awake() 
    {
        Debug.Log("Awake");

        mesh = new Mesh();
        mesh.name = "Procedural Quad";

        points = new List<Vector3>();
        points.Add(new Vector3(-1, 1));
        points.Add(new Vector3( 1, 1));
        points.Add(new Vector3(-1,-1));
        points.Add(new Vector3( 1,-1));

        // triangle vertices are specified in left hand order
        triangles = new List<int>();
        triangles.Add(1);triangles.Add(0);triangles.Add(2);
        triangles.Add(3);triangles.Add(1);triangles.Add(2);

        uvs = new List<Vector2>();
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));

        normals = new List<Vector3>();
        normals.Add(new Vector3(0, 0, 0));
        normals.Add(new Vector3(0, 0, 0));
        normals.Add(new Vector3(0, 0, 0));
        normals.Add(new Vector3(0, 0, 0));

        Debug.Log($"points.Count = {points.Count} uvs.Count = {uvs.Count}");
        Debug.Assert(points.Count == uvs.Count);
    }

    void Start() 
    {
        Debug.Log("Start");

        mesh.SetVertices(points);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        // mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
