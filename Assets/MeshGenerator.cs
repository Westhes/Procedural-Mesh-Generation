using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [Range(0.1f, 10f)]
    public float radius = 1;

    [Range(0.1f, 10f)]
    public float height = 1f;

    [Range(1, 10)]
    public int segments = 1;


    [Range(1, 360)]
    public int sides = 6;
    
    private MeshFilter MeshFilter { get; set; }
    private Mesh mesh;

    private Vector3[] Vertices { get; set; }
    private int[] Triangles;


    // Start is called before the first frame update
    void Start()
    {
        MeshFilter = GetComponent<MeshFilter>();
        CreateCylinder();
    }

    private void CreateCylinder()
    {
        mesh = new Mesh();

        // Calculate vertice positions
        Vertices = new Vector3[segments * sides + segments];
        int vIndex = 0;
        for (int iSegment = 0; iSegment < segments; iSegment++)
        {
            var segmentHeight = height / segments * iSegment;
            
            // Create segments
            var center = Vector3.up * segmentHeight;
            
            //if (i == 0 || i == segments)
            Vertices[vIndex++] = center;

            for (int side = 0; side < sides; side++)
            {
                var degrees = 360f / sides * side;
                var vertex = Rotate(Vector3.right * radius, degrees) + center;
                Vertices[vIndex++] = vertex;
            }
        }


        // Create triangles
        int tIndex = 0;
        vIndex = 0;
        Triangles = new int[3 * ((sides * 2) + (sides * 2 * (segments - 1)))];
        for (int seg = 0; seg < segments; seg++)
        {
            // Draw top or bottom
            bool isStartOrEnd = (seg == 0 || seg == segments -1);
            if (isStartOrEnd)
            {
                // Calculate the range between the first and last segment
                int startIndex = (sides + 1) * seg;
                int endIndex = (sides + 1) * (seg + 1);
                // 2 circles = almost a cylinder
                for (int j = startIndex; j < endIndex - 1; j++)
                {
                    // Avoid getting out of bounds and center vertex
                    int next = (j + 2 < endIndex) ? j + 2 : startIndex + 1;

                    Triangles[tIndex++] = startIndex;

                    // Flip the triangle sequence if it's the final layer
                    int topLayer = (seg == segments - 1) ? 0 : 1;
                    Triangles[tIndex++ + topLayer] = j + 1;
                    Triangles[tIndex++ - topLayer] = next;
                }
            }

            // Draw sides
            if (seg > 0)
            {
                // Skip the center vertex
                vIndex++;

                // Fill corners
                for (int j = 0; j < sides; j++)
                {
                    // Prevent looping
                    int nextVertex = vIndex + 1;
                    if (nextVertex >= seg + sides * seg)
                        nextVertex -= sides;

                    // Prevent looping
                    var VertexAbove = vIndex + sides + 2;
                    if (VertexAbove > (seg + 1) * sides + seg)
                        VertexAbove -= sides;

                    Triangles[tIndex] = vIndex;
                    Triangles[tIndex + 1] = Triangles[tIndex + 4] = nextVertex;
                    Triangles[tIndex + 2] = Triangles[tIndex + 3] = vIndex + sides + 1;
                    Triangles[tIndex + 5] = VertexAbove;

                    vIndex++;
                    tIndex += 6;
                }
            }
            
        }

        ApplyMesh();
    }

    private void ApplyMesh()
    {
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.RecalculateNormals();

        MeshFilter.mesh = mesh;
    }


    public static Vector3 Rotate(Vector3 v, float degrees)
    {
        return Quaternion.Euler(0, degrees, 0) * v;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && MeshFilter != null)
            CreateCylinder();
    }
}
