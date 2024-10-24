using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] height_map, float height_multiplier, AnimationCurve height_curve, int level_of_detail)
    {
        int width = height_map.GetLength(0);
        int height = height_map.GetLength(1);
        int vertex_index = 0;
        int mesh_simplification_incremement = (level_of_detail == 0) ? 1 : level_of_detail * 2; // (level_of_detail == 0)?1: checks if level_of_detail is 0, if it is, it uses 1, if it isn't, it uses mesh_simplification_incremement
        int vertices_per_line = (width - 1) / mesh_simplification_incremement + 1;
        float top_left_x = (width - 1) / -2f; // you need the f next to the number so the calculation isn't rounded when getting stored as a variable as if it was an integer calculation.
        float top_left_z = (height - 1) / 2f;

        MeshData mesh_data = new MeshData(vertices_per_line, vertices_per_line);

        for (int y = 0; y < height; y += mesh_simplification_incremement)
        {
            for (int x = 0; x < width; x += mesh_simplification_incremement)
            {
                mesh_data.vertices[vertex_index] = new Vector3((top_left_x + x), (height_curve.Evaluate(height_map[x, y]) * height_multiplier), (top_left_z - y));
                mesh_data.UVs[vertex_index] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    mesh_data.AddTriangle(vertex_index, vertex_index + vertices_per_line + 1, vertex_index + vertices_per_line);
                    mesh_data.AddTriangle(vertex_index + vertices_per_line + 1, vertex_index, vertex_index + 1);
                }

                vertex_index++;

            }
        }

        return mesh_data;

    }

}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] UVs;

    int triangle_index;

    public MeshData(int mesh_width, int mesh_height)
    {
        vertices = new Vector3[mesh_width * mesh_height];
        triangles = new int[(mesh_width - 1) * (mesh_height - 1) * 6];
        UVs = new Vector2[mesh_width * mesh_height];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangle_index] = a;
        triangles[triangle_index + 1] = b;
        triangles[triangle_index + 2] = c;
        triangle_index += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();
        return mesh;
    }

}