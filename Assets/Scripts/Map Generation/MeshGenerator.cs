using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] height_map, float height_multiplier, AnimationCurve height_curve, int level_of_detail) // generates a terrain mesh from the provided height map and parameters
    {
        int width = height_map.GetLength(0);
        int height = height_map.GetLength(1);
        int vertex_index = 0;
        int mesh_simplification_incremement = (level_of_detail == 0) ? 1 : level_of_detail * 2; // determines how many grid points to skip for a simplified mesh
        int vertices_per_line = (width - 1) / mesh_simplification_incremement + 1;
        float top_left_x = (width - 1) / -2f;
        float top_left_z = (height - 1) / 2f;

        MeshData mesh_data = new MeshData(vertices_per_line, vertices_per_line);

        for (int y = 0; y < height; y += mesh_simplification_incremement) // loop through the height map to generate vertices and triangles
        {
            for (int x = 0; x < width; x += mesh_simplification_incremement)
            {
                mesh_data.vertices[vertex_index] = new Vector3((top_left_x + x), (height_curve.Evaluate(height_map[x, y]) * height_multiplier), (top_left_z - y)); // calculate the vertex position based on the height map and scaling parameters
                mesh_data.UVs[vertex_index] = new Vector2(x / (float)width, y / (float)height); // assigns UV coordinates for texturing

                if (x < width - 1 && y < height - 1) // generate triangles for the mesh (except for the last row and column)
                {
                    // add two triangles to form a quad for the current grid square
                    mesh_data.AddTriangle(vertex_index, vertex_index + vertices_per_line + 1, vertex_index + vertices_per_line);
                    mesh_data.AddTriangle(vertex_index + vertices_per_line + 1, vertex_index, vertex_index + 1);
                }

                vertex_index++; // move to the next vertex

            }
        }

        return mesh_data;

    }

}

public class MeshData // this class represents the data required to create a Unity Mesh
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] UVs;

    int triangle_index;

    public MeshData(int mesh_width, int mesh_height) // constructor to initialize mesh data arrays based on dimensions.
    {
        vertices = new Vector3[mesh_width * mesh_height];
        triangles = new int[(mesh_width - 1) * (mesh_height - 1) * 6]; // 6 indices per quad (2 triangles)
        UVs = new Vector2[mesh_width * mesh_height];
    }

    public void AddTriangle(int a, int b, int c) // adds a triangle to the mesh by specifying three vertex indices.
    {
        triangles[triangle_index] = a;
        triangles[triangle_index + 1] = b;
        triangles[triangle_index + 2] = c;
        triangle_index += 3;
    }

    public Mesh CreateMesh() // creates a Unity mesh object from the mesh data.
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals(); // normals are used to calculate lighting
        return mesh;
    }
}